using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Gui.Helper
{
    /// <summary>
    /// Класс для выполнения действий с интервалом.
    /// Предназначен для Web запросов
    /// </summary>
    /// <typeparam name="TIn">Что передаем</typeparam>
    /// <typeparam name="TOut">Что получаем</typeparam>
    public class TimedAction<TIn, TOut>
    {
        /// <summary>
        /// Функция выполнения
        /// </summary>
        private readonly Func<TIn, TOut> _action;

        /// <summary>
        /// Таймер
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Контекст для GUI потока
        /// </summary>
        private readonly SynchronizationContext _sync;

        /// <summary>
        /// Параметр
        /// </summary>
        private TIn _param;

        /// <summary>
        /// Запрос на выполнение от пользователя
        /// </summary>
        private bool _sheduledByUser;

        /// <summary>
        /// Событие по окончанию асинхронного действия
        /// </summary>
        public event EventHandler<TOut> Performed;

        /// <param name="action">Длительное действие</param>
        /// <param name="interval">Интервал для выполнения действия</param>
        public TimedAction(Func<TIn, TOut> action, int interval = 400)
        {
            _action = action;
            _timer = new Timer(interval);
            _timer.Elapsed += OnPerform;

            _sync = SynchronizationContext.Current;
        }

        /// <summary>
        /// Асинхронно выполняем действие и запускаем событие в GUI потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPerform(object sender, ElapsedEventArgs e)
        {
            if (!_sheduledByUser)
                return;

            var result = await Task.Run(() => _action(_param));
            _sync.Post(state => Performed?.Invoke(this, result), null);
            _sheduledByUser = false;
        }

        /// <summary>
        /// Ставим событие в очередь, чтобы избежать ложных срабатываний
        /// </summary>
        /// <param name="param"></param>
        public void Shedule(TIn param)
        {
            _param = param;

            _timer.Stop();
            _timer.Start();

            _sheduledByUser = true;
        }
    }
}
