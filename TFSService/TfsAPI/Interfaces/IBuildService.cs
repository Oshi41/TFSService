using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;

namespace TfsAPI.Interfaces
{
    public interface IBuild
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
        /// Получаю все возможные определения сборок
        /// </summary>
        /// <returns>Список определений сборки</returns>
        Task<IList<BuildDefinitionReference>> GetAllDefentitions();

        /// <summary>
        /// Получаю дефолтные значения свойств для текущего определения
        /// </summary>
        /// <param name="def">Определение сборки</param>
        /// <returns></returns>
        Task<IDictionary<string, BuildDefinitionVariable>> GetDefaultProperties(BuildDefinitionReference def);

        /// <summary>
        /// Ставим сборку в очередь
        /// </summary>
        /// <param name="build">Какую сборку ставим</param>
        /// <returns></returns>
        Task<Build> Queue(Build build);

        /// <summary>
        /// Выполняем цикл проверки и постановки сборки
        /// </summary>
        Task Tick();

        /// <summary>
        /// Ставалю сборку в очередь
        /// </summary>
        /// <param name="project">Имя проекта</param>
        /// <param name="defName"Имя определения сборки></param>
        /// <param name="properties">Переменные сборки</param>
        Task<Build> Schedule(string project, string defName, IDictionary<string, BuildDefinitionVariable> properties);
    }
}