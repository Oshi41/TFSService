﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.TestManagement.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class MonthCheckinsViewModel : BindableExtended
    {
        private DateTime date;
        private DayViewModel selectedDay;
        private bool _isBusy;
        private List<DayViewModel> _month;
        private int sum;
        private readonly ITfsApi _api;

        // Храним кэш заргуженный дней
        private readonly Dictionary<DateTime, List<DayViewModel>> _cache = new Dictionary<DateTime, List<DayViewModel>>();

        public DateTime Date
        {
            get => date;
            set
            {
                if (SetProperty(ref date, value))
                {
                    OnDateChanged();
                }
            }
        }

        public DayViewModel SelectedDay { get => selectedDay; set => SetProperty(ref selectedDay, value); }

        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public List<DayViewModel> Month { get => _month; set => SetProperty(ref _month, value); }

        public int Sum { get => sum; set => SetProperty(ref sum, value); }

        public MonthCheckinsViewModel(ITfsApi api)
        {
            this._api = api;

            Date = DateTime.Now;
        }

        private async void OnDateChanged()
        {
            IsBusy = true;

            var start = new DateTime(Date.Year, Date.Month, 1);
            // Последний день месяца
            var end = (new DateTime(Date.Year, Date.Month + 1, 1)).AddDays(-1);

            // ограничили сегодняшним днем
            if (end > DateTime.Now)
            {
                end = DateTime.Now;
            }

            // Выбрали будущий месяц, смысла в поиске нет
            if (start > end)
                return;

            if (!_cache.ContainsKey(start))
            {
                // чекины за месяц (один запрос к TFS)
                var checkins = await Task.Run(() => _api.GetCheckins(start, end));
                var capacity = await Task.Run(() => _api.GetCapacity());

                var collection = new List<DayViewModel>();

                var i = start;
                while (i <= end)
                {
                    collection.Add(new DayViewModel(i, checkins, capacity));
                    i = i.AddDays(1);
                }

                _cache[start] = collection;
            }

            // Выставляем новый месяц
            if (Month?.FirstOrDefault()?.Time.SameMonth(Date) != true)
            {
                Month = _cache[start].ToList();
                Sum = Month.Sum(x => x.Hours);
            }

            SelectedDay = Month.FirstOrDefault(x => x.Time.IsToday(Date));

            IsBusy = false;
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