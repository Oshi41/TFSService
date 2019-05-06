using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Gui.Helper
{
    public class TimedAction<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _action;
        private readonly Timer _timer;
        private readonly SynchronizationContext _sync;

        private TIn _param;

        public event EventHandler<TOut> Performed;

        public TimedAction(Func<TIn, TOut> action, int interval = 200)
        {
            _action = action;
            _timer = new Timer(interval);
            _timer.Elapsed += OnPerform;

            _sync = SynchronizationContext.Current;
        }

        private async void OnPerform(object sender, ElapsedEventArgs e)
        {
            var result = await Task.Run(() => _action(_param));
            _sync.Post(state => Performed?.Invoke(this, result), null);
        }

        public void Shedule(TIn param)
        {
            _param = param;
            _timer.Stop();
            _timer.Start();
        }
    }
}
