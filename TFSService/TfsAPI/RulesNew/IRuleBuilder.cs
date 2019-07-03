using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Interfaces;

namespace TfsAPI.RulesNew
{
    /// <summary>
    /// Фабрика построения и выполнения правил
    /// </summary>
    public interface IRuleBuilder
    {
        /// <summary>
        /// Получение стандартного правила по ID и опциональными параметрами
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IRule BuildPresets(StaticRules rule, params object[] parameters);

        /// <summary>
        /// Возвращаю неподходящие по условию рабочие элементы
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        Dictionary<IRule, IList<WorkItem>> CheckInconsistant(IEnumerable<IRule> rules , ITfsApi api);
    }
}