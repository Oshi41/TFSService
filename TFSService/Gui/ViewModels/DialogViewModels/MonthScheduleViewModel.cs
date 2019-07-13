using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ITfsApi _api;

        // Храним кэш заргуженный дней
        private readonly Dictionary<DateTime, List<DayViewModel>> _cache =
            new Dictionary<DateTime, List<DayViewModel>>();

        private bool _isBusy;
        private List<DayViewModel> _month;
        private DateTime _date;
        private DayViewModel _selectedDay;
        private int _sum;
        private int _sumCapacity;

        public MonthCheckinsViewModel(ITfsApi api)
        {
            _api = api;

            Date = DateTime.Now;
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

        private async void OnDateChanged()
        {
            IsBusy = true;

            var start = new DateTime(Date.Year, Date.Month, 1);
            // Последний день месяца
            var end = new DateTime(Date.Year, Date.Month + 1, 1).AddDays(-1);

            // ограничили сегодняшним днем
            if (end > DateTime.Now) end = DateTime.Now;

            // Выбрали будущий месяц, смысла в поиске нет
            if (start > end)
                return;

            if (!_cache.ContainsKey(start))
            {
                // чекины за месяц (один запрос к TFS)
                var checkins = await Task.Run(() => _api.GetWriteoffs(start, end));

                var collection = new List<DayViewModel>();

                var i = start;
                while (i <= end)
                {
                    // Получаю кол-во часов для дня из итерации TFS
                    var capacity = await Task
                        .Run(() => _api
                            ?.GetCapacity(i, i)
                            ?.FirstOrDefault()
                            ?.GetCapacity(_api.Name));

                    collection.Add(new DayViewModel(i, checkins, capacity ?? 0));
                    i = i.AddDays(1);
                }

                _cache[start] = collection;
            }

            // Выставляем новый месяц
            if (Month?.FirstOrDefault()?.Time.SameMonth(Date) != true)
            {
                Month = _cache[start].ToList();
                Sum = Month.Sum(x => x.Hours);
                SumCapacity = Month.Sum(x => x.Capacity);
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
            IsHolliday = GetIsHolliday(Time);

            Checkins = checkins.Where(x => x.Key.Fields[CoreField.ChangedDate]?.Value is DateTime t
                                           && t.IsToday(Time)).ToList();
            Hours = Checkins.Select(x => x.Value).Sum();
        }

        public bool IsHolliday { get; }

        public List<KeyValuePair<Revision, int>> Checkins { get; }

        public int Hours { get; }

        public int Capacity { get; }
        public DateTime Time { get; }

        private static bool GetIsHolliday(DateTime time)
        {
            // Выходной
            if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday) return true;

            // TODO учитывать выходные

            return false;
        }
    }
}