using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TfsAPI.Interfaces
{
    public interface ICollectionObserver<T>
    {
        /// <summary>
        /// Стартануть сервис
        /// </summary>
        void Start();

        /// <summary>
        /// Приостановить сервис
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Задержка между запросами
        /// </summary>
        TimeSpan Delay { get; set; }

        /// <summary>
        /// Событие об имзенении статус
        /// </summary>
        event EventHandler<bool> StatusChanged;

        /// <summary>
        /// Регулярное действие обновления
        /// </summary>
        Task Tick();
        
        /// <summary>
        /// Наблюдаемая коллекция. Обычно сохраняется в файл
        /// </summary>
        List<T> Observed { get; }

        /// <summary>
        /// Запрос к серверу для получения актуальной коллекции
        /// </summary>
        /// <returns></returns>
        Task<IList<T>> Request();
    }
}