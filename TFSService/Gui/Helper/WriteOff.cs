using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Gui.Helper
{
    public class WriteOff
    {
        /// <summary>
        /// ID раочего элемента
        /// </summary>
        public int Id { get;  }

        /// <summary>
        /// Кол-во списанных часов
        /// </summary>
        public int Hours { get; private set; }

        /// <summary>
        /// Время списания часа
        /// </summary>
        public DateTime Time { get; }

        [JsonConstructor]
        private WriteOff(int id, int hours, DateTime time)
        {
            Id = id;
            Hours = hours;
            Time = time;
        }

        public WriteOff(int id, int hours)
            : this(id, hours, DateTime.Now)
        {
            
        }

        public void Increase(int hours)
        {
            Hours += hours;
        }
    }

    public class WriteOffCollection : ObservableCollection<WriteOff>
    {
        public void ScheduleWork(int id, int hours)
        {
            var first = this.FirstOrDefault(x => x.Id == id);
            if (first == null)
            {
                Add(new WriteOff(id, hours));
            }
            else
            {
                first.Increase(hours);
            }
        }

        public int GetTotalHours()
        {
            return this.Sum(x => x.Hours);
        }
    }
}
