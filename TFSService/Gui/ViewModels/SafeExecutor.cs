using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Gui.ViewModels
{
    /// <summary>
    /// Выполняет действие в GUi потоке
    /// </summary>
    public class SafeExecutor
    {
        private readonly Dispatcher _uiDispather;

        public SafeExecutor()
        {
            _uiDispather = App.Current.MainWindow.Dispatcher;
        }

        public async Task<T> ExecuteInGuiThread<T>(Func<T> func)
        {
            if (_uiDispather == null
                || func == null)
            {
                return default;
            }

            T result = default;

            await _uiDispather.BeginInvoke(new Action(() => result = func()), DispatcherPriority.Loaded);
            
            return result;
        }
    }
}
