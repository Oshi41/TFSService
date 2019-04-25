using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace Service
{
    public partial class TfsWatcher : ServiceBase
    {
        #region Fields

        

        #endregion

        public TfsWatcher()
        {
            InitializeComponent();

            CanHandleSessionChangeEvent = true;
            ServiceName = nameof(TfsWatcher);
        }

        #region protected Methods

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Trace.Write($"{DateTime.Now.ToLongTimeString()} Session state is changed : {changeDescription.Reason}");

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                case SessionChangeReason.SessionUnlock:
                case SessionChangeReason.ConsoleConnect:
                case SessionChangeReason.RemoteConnect:
                case SessionChangeReason.SessionRemoteControl:
                    OnLogOn(changeDescription);

                    break;

                case SessionChangeReason.SessionLogoff:
                case SessionChangeReason.SessionLock:
                case SessionChangeReason.ConsoleDisconnect:
                case SessionChangeReason.RemoteDisconnect:
                    OnLogOff(changeDescription);
                    break;
            }
        }

        protected override void OnStart(string[] args)
        {
            Trace.Write("On starting app");
        }

        protected override void OnStop()
        {
            Trace.Write("On shutdown");
        }

        #endregion

        #region private Methods

        private void OnLogOn(SessionChangeDescription changeDescription)
        {

        }

        private void OnLogOff(SessionChangeDescription changeDescription)
        {
            
        }

        #endregion
    }
}
