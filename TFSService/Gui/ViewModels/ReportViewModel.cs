using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xaml;
using Gui.Properties;
using Gui.Settings;
using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Common.Internal;
using Mvvm;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class ReportService : BindableBase, ITickable
    {
        private DateTime _lastUpdateTime;

        public ReportService()
        {
            Delay = TimeSpan.FromMinutes(1);
            Update();
        }

        private DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                if (SetProperty(ref _lastUpdateTime, value))
                {
                    using (var settings = new ReportSettings().Read<ReportSettings>())
                    {
                        settings.LastNotification = LastUpdateTime;
                    }
                }
            }
        }

        TimeSpan Remind { get; set; }

        bool ShouldRemind { get; set; }

        public async Task Tick()
        {
            if (!ShouldRemind)
                return;

            var now = DateTime.Now;

            if (LastUpdateTime.SameDay(now))
                return;

            Update();

            if (now.Hour >= Remind.Hours && now.Minute >= Remind.Minutes)
            {
                // todo show notification
            }
        }

        public TimeSpan Delay { get; }

        private void Update()
        {
            using (var settings = new ReportSettings().Read<ReportSettings>())
            {
                LastUpdateTime = settings.LastNotification;
                Remind = settings.Remind;
                ShouldRemind = settings.ShouldRemind;
            }
        }
    }

    public class ReportViewModel : BindableExtended
    {
        private readonly IConnect _connectService;
        private readonly IWriteOff _writeOff;
        private readonly IBuild _buildService;
        private readonly IChekins _chekins;
        private bool _busy;
        private List<WorkItemVm> _type;
        private string _html;
        private ObservableCollection<KeyValuePair<WorkItemVm, int>> _todayWork;
        private IList<Build> _builds;
        private TimeSpan _remind;
        private bool _enableRemind;
        private DateTime _lastUpdateTime;
        private List<Changeset> _checkins;

        public ObservableCollection<KeyValuePair<WorkItemVm, int>> TodayWork
        {
            get => _todayWork;
            set => SetProperty(ref _todayWork, value);
        }

        public ReportViewModel(IConnect connectService, IWriteOff writeOff, IBuild buildService, IChekins chekins)
        {
            _connectService = connectService;
            _writeOff = writeOff;
            _buildService = buildService;
            _chekins = chekins;

            Update();
        }

        private async Task Update()
        {
            Busy = true;

            var now = DateTime.Now;
            TodayWork = new ObservableCollection<KeyValuePair<WorkItemVm, int>>(
                await Task.Run(() => _writeOff
                    .GetWriteoffs(now, now).ToDictionary(x => new WorkItemVm(x.Key.WorkItem),
                        x => x.Value)
                    .ToList()));

            Builds = await _buildService.GetBuilds();

            Checkins = await Task.Run(() => _chekins.GetCheckins(DateTime.Today, now, _connectService.Name));

            var builder = new StringBuilder();

            foreach (var pair in TodayWork)
            {
                builder.Append("<li>");
                builder.Append($"<p>{Resources.AS_EnterDesciption}</p>");
                builder.Append($"<a href=\"{pair.Key.LinkUri}\">[{pair.Key.Item.Id}] {pair.Key.Item.Title}</a>");
                builder.Append("<p></p>");
                builder.Append("</li>");
            }

            foreach (var buildName in Builds.Select(x => x.Definition?.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct())
            {
                builder.Append("<li>");
                builder.Append($"<p>{Resources.AS_EnterDesciption}</p>");
                builder.Append($"<p>{Resources.AS_Branch} {buildName}</p>");
                builder.Append("<p></p>");
                builder.Append("</li>");
            }

            var result = string.Format(Properties.Resources.AS_ReportBase_Html, DateTime.Today.ToShortDateString(),
                builder.ToString());
            Html = result;

            using (var settings = new ReportSettings().Read<ReportSettings>())
            {
                Remind = settings.Remind;
                EnableRemind = settings.ShouldRemind;
            }

            Busy = false;
        }

        public List<Changeset> Checkins
        {
            get => _checkins;
            set => SetProperty(ref _checkins, value);
        }

        public IList<Build> Builds
        {
            get => _builds;
            set => SetProperty(ref _builds, value);
        }

        public string Html
        {
            get => _html;
            set => SetProperty(ref _html, value);
        }

        public bool Busy
        {
            get => _busy;
            set => SetProperty(ref _busy, value);
        }

        public TimeSpan Remind
        {
            get => _remind;
            set
            {
                if (SetProperty(ref _remind, value))
                {
                    if (Remind.TotalHours is > 24 or < 1)
                    {
                        Remind = TimeSpan.FromHours(12);
                        return;
                    }

                    using (var settings = new ReportSettings().Read<ReportSettings>())
                    {
                        settings.Remind = Remind;
                    }
                }
            }
        }

        public bool EnableRemind
        {
            get => _enableRemind;
            set
            {
                if (SetProperty(ref _enableRemind, value))
                {
                    using (var settings = new ReportSettings().Read<ReportSettings>())
                    {
                        settings.ShouldRemind = EnableRemind;
                    }
                }
            }
        }
    }
}