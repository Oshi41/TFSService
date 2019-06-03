using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.TestManagement.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class CheckinHistoryViewModel : BindableExtended
    {
        private readonly ITfsApi _api;
        private MonthScheduleViewModel _schedule;
        private DateTime _month;

        /// <summary>
        /// Кэшированные месячные расписания
        /// </summary>
        private readonly List<MonthScheduleViewModel> _cache = new List<MonthScheduleViewModel>();

        public DateTime Month
        {
            get => _month;
            set
            {
                if (SetProperty(ref _month, value))
                {
                    var first = _cache.FirstOrDefault(x => x?.Days?.Any(y => y.Time.SameMonth(Month)) == true);

                    if (first == null)
                    {
                        first = new MonthScheduleViewModel(_api, Month);
                        _cache.Add(first);
                    }

                    Schedule = first;
                }
            }
        }

        public MonthScheduleViewModel Schedule
        {
            get => _schedule;
            set => SetProperty(ref _schedule, value);
        }

        public CheckinHistoryViewModel(ITfsApi api)
        {
            _api = api;
            Month = DateTime.Today;
        }
    }

    public class MonthScheduleViewModel : BindableBase
    {
        private ObservableCollection<DayViewModel> _days;
        private bool _isBusy = true;

        public ObservableCollection<DayViewModel> Days
        {
            get => _days;
            private set => SetProperty(ref _days, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public MonthScheduleViewModel(ITfsApi api, DateTime time)
        {
            Init(api, time);
        }

        private async void Init(ITfsApi api, DateTime time)
        {
            IsBusy = true;

            var start = new DateTime(time.Year, time.Month, 1);
            var now = DateTime.Now;

            var collection = new List<DayViewModel>();

            // чекины за месяц (один запрос к TFS)
            var checkins = await Task.Run(() => api.GetCheckins(start, now));
            var capacity = api.GetCapacity();

            while (start.Month == time.Month
                   && start <= now)
            {
                collection.Add(new DayViewModel(start, checkins, capacity));
                start = start.AddDays(1);
            }

            Days = new ObservableCollection<DayViewModel>(collection.OrderBy(x => x.Time));

            IsBusy = false;
        }

        public override string ToString()
        {
            if (Days.IsNullOrEmpty())
                return "";

            return $"{Days.Where(x => !x.IsHolliday).Sum(x => x.Hours)} часов " +
                   $"из {Days.Where(x => !x.IsHolliday).Sum(x => x.Capacity)} запланированных";
        }
    }

    public interface ITimable
    {
        DateTime Time { get; }
    }

    public class DayViewModel : BindableBase, ITimable
    {
        public DateTime Time { get; }

        public bool IsHolliday { get; }

        public List<KeyValuePair<Revision, int>> Checkins { get; }

        public int Hours { get; }

        public int Capacity { get; }

        public DayViewModel(DateTime time, List<KeyValuePair<Revision, int>> checkins, int capacity)
        {
            Capacity = capacity;

            Time = time.Date;
            IsHolliday = GetIsHolliday(Time);

            Checkins = checkins.Where(x => x.Key.Fields[CoreField.ChangedDate]?.Value is DateTime t
                    && t.IsToday(Time)).ToList();
            Hours = Checkins.Select(x => x.Value).Sum();
        }

        private static bool GetIsHolliday(DateTime time)
        {
            // Выходной
            if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            // TODO учитывать выходные

            return false;
        }
    }
}
