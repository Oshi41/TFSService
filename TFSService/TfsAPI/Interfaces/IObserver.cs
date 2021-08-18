using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TfsAPI.Interfaces
{
    public interface ICollectionObserver<T> : ITickable
    {
        bool Running { get; set; }

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