using System;
using Gui.Helper;

namespace Gui.Settings
{
    public class WriteOffSettings : SettingsBase
    {
        private TimeSpan _capacity = TimeSpan.FromHours(7);
        private TimeSpan _workTime = TimeSpan.FromHours(9);
        private WriteOffCollection _completedWork = new WriteOffCollection();

        public override string ConfigName()
        {
            return "writeoff.json";
        }
        public TimeSpan Capacity
        {
            get => _capacity;
            set => Set(ref _capacity, value);
        }

        public TimeSpan WorkTime
        {
            get => _workTime;
            set => Set(ref _workTime, value);
        }
        
        /// <summary>
        ///     Сколько часов было списано на разные рабочие элементы
        /// </summary>
        public WriteOffCollection CompletedWork
        {
            get => _completedWork;
            set => Set(ref _completedWork, value);
        }
    }
}