using Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Extentions;

namespace Gui.Settings
{
    public class DisplayTime : BindableBase
    {
        public Dictionary<DateTime, bool> SessionTimes { get; set; } = new Dictionary<DateTime, bool>();

        public void ClearPreviouse()
        {
            var keys = SessionTimes.Keys;

            var changed = false;

            foreach (var time in keys)
            {
                if (!time.IsToday())
                {
                    SessionTimes.Remove(time);
                    changed = true;
                }
            }

            if (changed)
                RaiseChange();
        }

        public void AddDate(bool isLogon)
        {
            SessionTimes[DateTime.Now] = isLogon;            
            RaiseChange();
        }

        public DateTime GetBegin()
        {
            foreach (var item in SessionTimes)
            {
                if (item.Value)
                    return item.Key;
            }

            return DateTime.MinValue;
        }

        public TimeSpan GetDisplayTime()
        {
            var copy = SessionTimes.ToDictionary(x => x.Key, x => x.Value);

            var duration = TimeSpan.Zero;
            var keyes = copy.Keys.OrderBy(x => x).ToList();

            if (!keyes.Any())
                return duration;

            // Дополняем последнюю информацию (как будто только что разлогинены)
            if (copy[keyes.Last()])
            {
                copy[DateTime.Now] = false;
            }

            for (int i = 0; i < keyes.Count; i++)
            {
                var begin = keyes[0];

                // Если залогинились, то ищем сл ближайший разлогон
                if (copy[begin])
                {
                    i++;

                    while (i < keyes.Count)
                    {
                        var end = keyes[i];

                        // нашли разлогинивание
                        if (!copy[begin])
                        {
                            var toAdd = end - begin;
                            duration.Add(toAdd.Duration());
                            break;
                        }
                    }
                }

            }

            return duration;
        }

        private void RaiseChange()
        {
            OnPropertyChanged(nameof(SessionTimes));
        }

    }
}
