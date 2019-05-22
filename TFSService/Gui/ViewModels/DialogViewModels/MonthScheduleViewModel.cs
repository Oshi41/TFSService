using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Common;
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
                    var first = _cache.FirstOrDefault(x => x.Days.Any(y => y.Time.SameMonth(Month)));

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

            var collection = new ConcurrentBag<DayViewModel>();
            var tasks = new List<Task>();

            while (start.Month == time.Month
                   && start <= now)
            {
                var startCopy = start;

                tasks.Add(
                    Task.Run(() =>
                    {
                        collection.Add(new DayViewModel(api, startCopy));
                    }));

                start = start.AddDays(1);
            }

            await Task.WhenAll(tasks);

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

    public class DayViewModel : BindableBase
    {
        public DateTime Time { get; }

        public bool IsHolliday { get; }

        public List<KeyValuePair<Revision, int>> Checkins { get; }

        public int Hours { get; }

        public int Capacity { get; }

        public DayViewModel(ITfsApi api, DateTime time)
        {
            Time = time.Date;
            IsHolliday = GetIsHolliday(Time);

            Checkins = api.GetCheckins(time, time.AddHours(24));
            Hours = Checkins.Select(x => x.Value).Sum();
            Capacity = api.GetCapacity();
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
