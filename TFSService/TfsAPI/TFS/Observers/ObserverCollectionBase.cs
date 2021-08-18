using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS.Observers
{
    public abstract class ObserverCollectionBase<T> : ICollectionObserver<T>
    {
        private readonly IEqualityComparer<T> _idComparer;
        private readonly IEqualityComparer<T> _changesComparer;
        private readonly Action _afterTick;

        /// <summary>
        /// Конструктор наблюдаемой коллекции
        /// </summary>
        /// <param name="idComparer">Компаратор для сравнения уникальности элементов</param>
        /// <param name="changesComparer">Компаратор для сравнения на наличие изменений</param>
        /// <param name="afterTick">Действие после обновления</param>
        /// <param name="saved">Список сохраненных наблюдаемых объектов</param>
        /// <param name="delay">Задержка между запросами</param>
        protected ObserverCollectionBase(IEqualityComparer<T> idComparer,
            IEqualityComparer<T> changesComparer,
            Action afterTick,
            IEnumerable<T> saved,
            TimeSpan delay,
            bool running)
        {
            _idComparer = idComparer;
            _changesComparer = changesComparer;
            _afterTick = afterTick;
            Observed = saved?.ToList() ?? new List<T>();
            Running = running;

            Delay = delay;
        }

        public TimeSpan Delay { get; set; }

        public bool Running { get; set; }

        public async Task Tick()
        {
            if (!Running)
                return;
            
            var oldCollection = Observed;
            var actual = await Request();

            var old = oldCollection.Except(actual, _idComparer).ToList();
            var add = actual.Except(oldCollection, _idComparer).ToList();
            var changed = actual.Intersect(oldCollection, _idComparer).Except(oldCollection, _changesComparer).ToList();

            if (!old.IsNullOrEmpty())
            {
                OnCollectionChanged(CollectionChangeAction.Remove, old);
            }

            if (!add.IsNullOrEmpty())
            {
                OnCollectionChanged(CollectionChangeAction.Add, add);
            }

            if (!changed.IsNullOrEmpty())
            {
                OnCollectionChanged(CollectionChangeAction.Refresh, changed);
            }

            Observed.Clear();
            Observed.AddRange(actual);

            _afterTick?.Invoke();
        }

        /// <summary>
        /// Запрашиваем коллекцию с сервера
        /// </summary>
        /// <returns></returns>
        public abstract Task<IList<T>> Request();

        /// <summary>
        /// Коллекцию наблюдаемых элементов
        /// </summary>
        /// <returns></returns>
        public List<T> Observed { get; }

        /// <summary>
        /// Действительное изменение в коллекции.
        /// </summary>
        /// <param name="action">Какое изменение произошло</param>
        /// <param name="collection">Изменённые объекты (для Remove значения старые!)</param>
        protected abstract void OnCollectionChanged(CollectionChangeAction action, IList<T> collection);
    }
}