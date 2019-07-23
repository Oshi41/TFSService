using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Interfaces;
using Timer = System.Timers.Timer;

namespace TfsAPI.ResumeTimer
{
    public class ResumeTimer  : IResumable
    {
        private readonly List<InvokationHistory> _histories = new List<InvokationHistory>();
        // Минутный интервал
        private Timer _timer = new Timer(1000 * 60);

        public ResumeTimer()
        {
            _timer.Elapsed += TryInvoke;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Pause()
        {
            _timer.Stop();
        }

        public void AddAction(TimeSpan span, Action action)
        {
            span = span.Duration();

            if (span.TotalMinutes < 1)
            {
                throw new Exception("Can't work with less than minute periods");
            }

            _histories.Add(new InvokationHistory(action, span));
        }

        private void TryInvoke(object sender, ElapsedEventArgs e)
        {
            _histories.ForEach(x => x.TryExecute());
        }
    }

    class InvokationHistory
    {
        public InvokationHistory(Action mainAction, TimeSpan span, DateTime? lastExecuted = null)
        {
            LastExecuted = lastExecuted ?? DateTime.MinValue;
            MainAction = mainAction;
            Span = span;
        }

        public TimeSpan Span { get; }
        public Action MainAction { get; }
        public DateTime LastExecuted { get; private set; }

        public bool TryExecute()
        {
            var time = DateTime.Now;

            var result = time - LastExecuted >= Span;

            if (result)
            {
                MainAction?.Invoke();
                LastExecuted = time;
            }

            return result;
        }
    }
}
