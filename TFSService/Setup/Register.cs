using Microsoft.Win32;
using System;
using System.Configuration.Install;
using System.ComponentModel;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Setup
{
    [RunInstaller(true)]
    public class Register : Installer
    {
        #region Fields

        private const string _exeKey = "Exe";
        private const string _appKey = "AppName";

        #endregion

        #region overrided

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

#if DEBUG

            int processId = Process.GetCurrentProcess().Id;
            string message = string.Format("Please attach the debugger (elevated on Vista or Win 7) to process [{0}].", processId);
            MessageBox.Show(message, "Debug");

#endif

            var dict = this.Context.Parameters;
            if (dict.ContainsKey(_exeKey)
                && dict.ContainsKey(_appKey))
            {
                try
                {
                    RegisterAsStartUp(dict[_appKey], dict[_exeKey]);
                }
                catch (Exception e)
                {
                    this.Context.LogMessage($"{nameof(Install)}: {e}");
                    this.Context.LogMessage("Successfully register app as start-up");
                }
            }
            else
            {
                this.Context.LogMessage($"Required parameters not specisied:{_exeKey} or {_appKey}");
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);

            var dict = this.Context.Parameters;
            if (dict.ContainsKey(_appKey))
            {
                try
                {
                    UnregisterAsStartUp(dict[_appKey]);
                    this.Context.LogMessage("Successfully unregister app as start-up");
                }
                catch (Exception e)
                {
                    this.Context.LogMessage($"{nameof(Rollback)}: {e}");
                }
            }
            else
            {
                this.Context.LogMessage($"Required parameters not specisied: {_appKey}");
            }
        }

        #endregion

        #region logic

        /// <summary>
        /// Возвращает ключ с автозапуском
        /// </summary>
        /// <returns></returns>
        private RegistryKey GetKey => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void RegisterAsStartUp(string appName, string exe)
        {
            if (!File.Exists(exe))
                throw new FileNotFoundException(exe);

            var key = GetKey;
            UnregisterAsStartUp(appName, key);

            key.SetValue(appName, exe);
        }

        private void UnregisterAsStartUp(string appName, RegistryKey key = null)
        {
            if (key == null)
                key = GetKey;

            if (key.GetValue(appName) != null)
            {
                key.DeleteValue(appName);
            }
        }

        #endregion
    }
}
