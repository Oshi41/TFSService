using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using TfsAPI.Extentions;

namespace Gui.Helper
{
    public class SystemHelper : IDisposable
    {
        /// <summary>
        /// Указатель на таймер
        /// </summary>
        private IntPtr _timer;

        #region Events

        /// <summary>
        /// Пользователь зашёл в систему
        /// </summary>
        public event EventHandler Logon;

        /// <summary>
        /// Пользователь вышел из системы
        /// </summary>
        public event EventHandler Logoff;

        /// <summary>
        /// Нуждно списать время
        /// </summary>
        public event EventHandler WriteOff;

        #endregion

        public SystemHelper()
        {
            Subscribe();
        }

        #region private methods

        private void Subscribe()
        {
            _timer = SystemEvents.CreateTimer(60 * 60 * 1000);

            SystemEvents.SessionSwitch += OnSessionSwitched;
            SystemEvents.TimerElapsed += FireWiteOffEvent;
            App.Current.Exit += OnExit;
        }

        private void Unsubscribe()
        {
            SystemEvents.KillTimer(_timer);

            SystemEvents.SessionSwitch -= OnSessionSwitched;
            SystemEvents.TimerElapsed -= FireWiteOffEvent;
            App.Current.Exit -= OnExit;
        }

        /// <summary>
        /// Сохраняю в файл начало рабочего дня
        /// </summary>
        private void TryStartWorkDay()
        {
            using (var settings = Settings.Settings.Read())
            {
                var now = DateTime.Now;

                if (!settings.Begin.IsToday(now))
                {
                    settings.Begin = now;

                    Trace.Write($"Let the work begin at {now.ToShortTimeString()}!");
                }
            }
        }

        #endregion

        #region handlers

        private void OnSessionSwitched(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                    case SessionSwitchReason.ConsoleConnect:
                    case SessionSwitchReason.RemoteConnect:
                    case SessionSwitchReason.SessionLogon:
                    case SessionSwitchReason.SessionRemoteControl:
                    case SessionSwitchReason.SessionUnlock:
                        TryStartWorkDay();
                        Logon?.Invoke(this, EventArgs.Empty);
                        break;

                    default:
                        Logoff?.Invoke(this, EventArgs.Empty);
                        break;
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Dispose();
        }

        private void FireWiteOffEvent(object sender, TimerElapsedEventArgs e)
        {
            using (var settings = Settings.Settings.Read())
            {
                settings.Completed++;
            }

            WriteOff?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
