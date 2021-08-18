using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TfsAPI.Interfaces;

namespace Gui.Helper
{
    public class TickerController
    {
        private readonly SynchronizationContext _context;
        // Работаю с мапой только в GUI потоке
        private readonly IDictionary<ITickable, DateTime> _lastTickTimes = new Dictionary<ITickable, DateTime>();

        public TickerController()
        {
            _context = SynchronizationContext.Current;
            
            new Thread(Run).Start();
        }

        public void Register(ITickable tickable)
        {
            _context.Post(state => _lastTickTimes[state as ITickable] = DateTime.MinValue, tickable);
        }

        private void Run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                var now = DateTime.Now;
                
                foreach (var tickable in _lastTickTimes.Keys.ToList())
                {
                    var prev = _lastTickTimes[tickable];

                    if ((now - prev).Duration() >= (tickable.Delay).Duration())
                    {
                        _context.Post(Execute, tickable);
                    }
                }
            }
        }

        /// <summary>
        /// Выполняется в GUI потоке
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(object obj)
        {
            if (obj is ITickable tickable)
            {
                tickable.Tick();
                _lastTickTimes[tickable] = DateTime.Now;
            }
        }
    }
}