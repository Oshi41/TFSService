using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class MonthScheduleViewModel : BindableExtended
    {
        private readonly ITfsApi _api;

        public ObservableCollection<DayViewModel> Days { get; } = new ObservableCollection<DayViewModel>();

        public MonthScheduleViewModel(ITfsApi api, DateTime time)
        {
            _api = api;

            // Запомнили номер месяца
            var month = time.Month;
            var today = DateTime.Today;

            // Выставим время в начало месяца
            time = new DateTime(time.Year, time.Month, 1);

            while (time.Month == month 
                   // Отрисовываем вплоть до сегодняшнего дня
                   && time <= today)
            {
                Days.Add(new DayViewModel(api, new DateTime(time.Year, month, time.Day)));

                time = time.AddDays(1);
            }
        }
    }

    public class DayViewModel : BindableBase
    {
        private readonly ITfsApi _api;

        public DateTime Time { get; }

        public bool IsHolliday { get; }

        public int Hours { get; }

        public List<Revision> Revisions { get; }

        public DayViewModel(ITfsApi api, DateTime time)
        {
            _api = api;
            Time = time.Date;

            var checkins = _api.GetCheckins(time, time.AddHours(24));
            Revisions = checkins.Keys.ToList();
            Hours = checkins.Values.Sum();

            IsHolliday = GetIsHolliday(Time);
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
