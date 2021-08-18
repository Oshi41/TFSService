using System;

namespace Gui.Settings
{
    public class ReportSettings : SettingsBase
    {
        private TimeSpan _remind = TimeSpan.FromHours(18);
        private bool _shouldRemind = false;
        private DateTime _lastNotification = DateTime.MinValue;

        public override string ConfigName()
        {
            return "remind.json";
        }


        public TimeSpan Remind
        {
            get => _remind;
            set => Set(ref _remind, value);
        }

        public bool ShouldRemind
        {
            get => _shouldRemind;
            set => Set(ref _shouldRemind, value);
        }

        public DateTime LastNotification
        {
            get => _lastNotification;
            set => Set(ref _lastNotification, value);
        }
    }
}