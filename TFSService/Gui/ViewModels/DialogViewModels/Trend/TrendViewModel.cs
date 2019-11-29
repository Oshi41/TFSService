using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels.Trend
{
    public class TrendViewModel : BindableExtended
    {
        private readonly ITfsApi _api;
        private readonly int _capacity;
        private readonly Dictionary<DateTime, ChartViewModel> _cache = new Dictionary<DateTime, ChartViewModel>();
        private bool _isBusy;
        private DateTime _selected;
        private ChartViewModel _chartVm;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public DateTime Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    OnDateChanged();
                }
            }
        }

        public ChartViewModel ChartVm
        {
            get => _chartVm;
            set => SetProperty(ref _chartVm, value);
        }

        public TrendViewModel(ITfsApi api, int capacity)
        {
            _api = api;
            _capacity = capacity;

            Selected = DateTime.Now;
        }

        private async void OnDateChanged()
        {
            IsBusy = true;

            var monthStart = new DateTime(Selected.Year, Selected.Month, 1);
            if (!_cache.TryGetValue(monthStart, out var result))
            {
                var end = monthStart.AddMonths(1).AddMinutes(-1);

                var chart = await Task.Run(() => _api.GetForMonth(monthStart, end, _capacity));

                result = new ChartViewModel(chart);
            }

            ChartVm = result;

            IsBusy = false;
        }
    }
}