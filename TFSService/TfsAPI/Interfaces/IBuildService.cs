using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;

namespace TfsAPI.Interfaces
{
    public interface IBuild : ITickable
    {
        /// <summary>
        /// Очередь внутренняя
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<Build> GetQueue();

        /// <summary>
        /// Очередь на сбор агентах
        /// </summary>
        /// <returns></returns>
        Task<IList<Build>> GetRunningBuilds();

        /// <summary>
        /// Поиск по сборкам
        /// </summary>
        /// <param name="actor">Кто поставил (по дефолту я)</param>
        /// <param name="from">За какое время от сегдоняшнего (по дефолту сегодня)</param>
        /// <returns></returns>
        Task<IList<Build>> GetBuilds(string actor = null, DateTime? from = null);
        

        /// <summary>
        /// Получаю все возможные определения сборок
        /// </summary>
        /// <returns>Список определений сборки</returns>
        Task<IList<BuildDefinitionReference>> GetAllDefentitions();

        /// <summary>
        /// Запрашиваем дефолтные значения для сборок
        /// </summary>
        /// <param name="source">Список сборок</param>
        /// <returns></returns>
        Task<IDictionary<BuildDefinitionReference, IDictionary<string, BuildDefinitionVariable>>> GetDefaultVariables(
            IEnumerable<BuildDefinitionReference> source);

        /// <summary>
        /// Обновляет переданные сборки
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        Task<IList<Build>> Update(IEnumerable<Build> old);

        /// <summary>
        /// Ставим сборку в очередь
        /// </summary>
        /// <param name="build">Какую сборку ставим</param>
        /// <returns></returns>
        Task<Build> Queue(Build build);

        /// <summary>
        /// Ставалю сборку в очередь
        /// </summary>
        /// <param name="project">Имя проекта</param>
        /// <param name="defName"Имя определения сборки></param>
        /// <param name="properties">Переменные сборки</param>
        /// <param name="forced">Поставить сборку сразу же</param>
        Task<Build> Schedule(string project, string defName, IDictionary<string, BuildDefinitionVariable> properties, bool forced);
    }
}