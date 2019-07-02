using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;

namespace TfsAPI.TFS
{
    public interface ICapacitySearcher
    {
        /// <summary>
        /// Возвращает список команд, к которым я имею доступ
        /// </summary>
        /// <returns></returns>
        IList<TeamFoundationTeam> GetAllMyTeams();

        /// <summary>
        /// Поиск списка трудозатрат моих команд за указанный промежуток времени
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<TeamCapacity> SearchCapacities(string name, DateTime start, DateTime end);
    }
}