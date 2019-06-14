using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.RulesNew
{
    public interface IRuleBuilder
    {
        IRule BuildPresets(StaticRules rule, params object[] parameters);
        Dictionary<IRule, IList<WorkItem>> CheckInconsistant(IEnumerable<IRule> rules);
    }
}