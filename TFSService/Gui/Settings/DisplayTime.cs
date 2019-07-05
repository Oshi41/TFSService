using Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var keys = SessionTimes.Keys.ToList();

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

        public void AddDate(DateTime time, bool isLogon)
        {
            if (SessionTimes.ContainsKey(time))
            {
                Trace.Write($"{nameof(DisplayTime)}.{nameof(AddDate)}: Time is already recorded");
                return;
            }

            SessionTimes[time] = isLogon;            
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
                keyes = copy.Keys.OrderBy(x => x).ToList();
            }

            // Считаем, что нет ошибок в записи
            for (int i = 0; i < keyes.Count; i += 2)
            {
                if (i + 1 < keyes.Count)
                {
                    var session = keyes[i + 1] - keyes[i];
                    duration = duration.Add(session.Duration());
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
