using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Setup
{
    [RunInstaller(true)]
    public class Register : Installer
    {
        [Conditional("DEBUG")]
        private void ShowDebug()
        {
            var message = $"Please attach the debugger to process [{Process.GetCurrentProcess().Id}].";
            MessageBox.Show(message, "Wait for debug attaching");
        }

        private void Undo()
        {
            ShowDebug();

            try
            {
                UnregisterAsStartup();
                Context.LogMessage("Successfully unregistered app as start-up");
            }
            catch (Exception e)
            {
                Context.LogMessage($"{nameof(Undo)}: {e}");
            }
        }

        #region Fields

        public const string ExeName = "Gui.exe";
        public const string AppName = "TFS Service";

        public const string AssemblyKey = "assemblypath";

        #endregion

        #region overrided

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            ShowDebug();

            try
            {
                RegisterAsStartup();
                Context.LogMessage("Successfully register app as start-up");
            }
            catch (Exception e)
            {
                Context.LogMessage($"{nameof(Install)}: {e}");
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);

            Undo();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            Undo();
        }

        #endregion

        #region logic

        /// <summary>
        ///     Возвращает ключ с автозапуском
        /// </summary>
        /// <returns></returns>
        private RegistryKey GetKey =>
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void RegisterAsStartup()
        {
            // Получили путь к dll, откуда вызывается метод
            var executingDll = Context.Parameters[AssemblyKey];
            var parent = Path.GetDirectoryName(executingDll);
            var exe = Path.Combine(parent, ExeName);

            if (!File.Exists(exe))
                throw new FileNotFoundException(exe);

            // Нужно экранировать путь кавычками
            exe = $"\"{exe}\"";

            var key = GetKey;
            UnregisterAsStartup(key);

            key.SetValue(AppName, exe);
        }

        private void UnregisterAsStartup(RegistryKey key = null)
        {
            if (key == null)
                key = GetKey;

            if (key.GetValue(AppName) != null) key.DeleteValue(AppName);
        }

        #endregion
    }
}