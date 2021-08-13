using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gui.Settings;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Окошко для месячного списания трудозатрат
    /// </summary>
    public class MonthCheckinsViewModel : BindableExtended
    {
        private readonly IWriteOff _api;

        // Храним кэш заргуженный дней
        private readonly Dictionary<DateTime, List<DayViewModel>> _cache =
            new Dictionary<DateTime, List<DayViewModel>>();

        private DateTime _date;

        private bool _isBusy;
        private List<DayViewModel> _month;
        private DayViewModel _selectedDay;
        private int _sum;
        private int _sumCapacity;
        private int _dailyHours;

        public MonthCheckinsViewModel(IWriteOff api)
        {
            _api = api;

            Date = DateTime.Now;
            _dailyHours = (int)new WriteOffSettings().Read<WriteOffSettings>().Capacity.TotalHours;
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (SetProperty(ref _date, value)) OnDateChanged();
            }
        }

        public DayViewModel SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public List<DayViewModel> Month
        {
            get => _month;
            set => SetProperty(ref _month, value);
        }

        public int Sum
        {
            get => _sum;
            set => SetProperty(ref _sum, value);
        }

        public int SumCapacity
        {
            get => _sumCapacity;
            set => SetProperty(ref _sumCapacity, value);
        }

        public int DailyHours
        {
            get => _dailyHours;
            set
            {
                if (SetProperty(ref _dailyHours, value))
                {
                    using (var settings = new WriteOffSettings().Read<WriteOffSettings>())
                    {
                        settings.Capacity = TimeSpan.FromHours(DailyHours);
                    }

                    OnDateChanged(true);
                }
            }
        }
        
        private async void OnDateChanged(bool forced = false)
        {
            IsBusy = true;

            var start = new DateTime(Date.Year, Date.Month, 1);

            // Последний день месяца
            var end = start.AddMonths(1).AddMinutes(-1);

            // ограничили сегодняшним днем
            if (end > DateTime.Now) end = DateTime.Now;

            // Выбрали будущий месяц, смысла в поиске нет
            if (start > end)
                return;

            if (!_cache.ContainsKey(start) || forced)
            {
                // чекины за месяц (один запрос к TFS)
                var checkins = await Task.Run(() => _api.GetWriteoffs(start, end));

                var collection = new List<DayViewModel>();

                var i = start;
                while (i <= end)
                {
                    int? capacity = new WriteOffSettings().Read<WriteOffSettings>().Capacity.Hours;

                    // // Учитываю настройки пользователя
                    // var settings = ;
                    // if (settings.ByUser)
                    // {
                    //     capacity = settings.Hours;
                    // }
                    // else
                    // {
                    //     // Получаю кол-во часов для дня из итерации TFS
                    //     capacity = await Task
                    //         .Run(() => _api
                    //             ?.GetCapacity(i, i)
                    //             ?.FirstOrDefault()
                    //             ?.GetCapacity(_api.Name));
                    // }

                    collection.Add(new DayViewModel(i, checkins, capacity ?? 0));
                    i = i.AddDays(1);
                }

                _cache[start] = collection;
            }

            // Выставляем новый месяц
            if (Month?.FirstOrDefault()?.Time.SameMonth(Date) != true || forced)
            {
                Month = _cache[start].ToList();
                Sum = Month.Sum(x => x.Hours);
                SumCapacity = Month
                    .Where(x => !x.IsHolliday)
                    .Sum(x => x.Capacity);
            }

            SelectedDay = Month.FirstOrDefault(x => x.Time.IsToday(Date));

            IsBusy = false;
        }
    }

    public interface ITimable
    {
        DateTime Time { get; }
    }

    /// <summary>
    ///     Представление одного дня в календаре
    /// </summary>
    public class DayViewModel : BindableBase, ITimable
    {
        public DayViewModel(DateTime time, List<KeyValuePair<Revision, int>> checkins, int capacity)
        {
            Capacity = capacity;

            Time = time.Date;
            IsHolliday = Time.IsHoliday();

            Checkins = checkins.Where(x => x.Key.Fields[CoreField.ChangedDate]?.Value is DateTime t
                                           && t.IsToday(Time)).ToList();
            Hours = Checkins.Select(x => x.Value).Sum();
        }

        public bool IsHolliday { get; }

        public List<KeyValuePair<Revision, int>> Checkins { get; }

        public int Hours { get; }

        public int Capacity { get; }
        public DateTime Time { get; }
    }
}