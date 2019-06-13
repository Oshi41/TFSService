using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Comarers;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace TfsAPI.Rules
{
    class MyTaskInCurrentIterationRule : MyItemsRule
    {
        public MyTaskInCurrentIterationRule(ITfsApi api) : base(api)
        {
        }

        #region Overrides of MyItemsRule

        protected override string GetQuery()
        {
            var builder = new WiqlBuilder()
                .AssignedToMe()
                .CurrentIteration()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .WithStates("and", "<>", WorkItemStates.Closed);

            return builder.ToString();

            return $"select * from {Constants.Sql.Tables.WorkItems} " +
                   $"and {Constants.Sql.Fields.WorkItemType} = '{WorkItemTypes.Task}' " +
                   $"and {Constants.Sql.IsCurrentIteractionCondition}" +
                   $"and {Sql.AssignedToMeCondition}";
        }

        protected override List<WorkItem> GetInconsistent(IList<WorkItem> myItems, IList<WorkItem> queried)
        {
            var except = myItems
                .Where(x => x.IsTypeOf(WorkItemTypes.Task))
                .Except(
                    queried.Where(y => y.IsTypeOf(WorkItemTypes.Task)), new WorkItemComparer());

            return except.ToList();
        }

        #endregion
    }
}
