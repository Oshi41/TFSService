using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TfsAPI.Attributes;
using TfsAPI.Comarers;
using TfsAPI.Constants;
using TfsAPI.Interfaces;
using TfsAPI.Rules;

namespace TfsAPI.RulesNew
{
    public enum StaticRules
    {
        /// <summary>
        /// Все мои таски на эту итерацию
        /// </summary>
        [LocalizedDescription(nameof(Properties.Resource.AS_CurrentIteration))]
        AllTasksIsCurrentIteration,

        /// <summary>
        /// Проверяет мои таски на правильную область
        /// </summary>
        [LocalizedDescription(nameof(Properties.Resource.AS_SpecifiedArea))]
        CheckTasksAreapath,
    }

    public enum RuleOperation
    {
        /// <summary>
        /// Оба запроса возвращают одно кол-во элементов
        /// </summary>
        SameCount,

        /// <summary>
        /// Проверяющий запрос не возвращает вариантов
        /// </summary>
        ZeroCount,
    }

    public class RuleBuilder : IRuleBuilder
    {
        private readonly ITfsApi _api;

        #region Presets
        public IRule BuildPresets(StaticRules rule, params object[] parameters)
        {
            switch (rule)
            {
                case StaticRules.AllTasksIsCurrentIteration:
                    return AllTasksIsCurrentIteration();

                case StaticRules.CheckTasksAreapath:
                    return CheckTasksAreapath(parameters[0] as string);


                default:
                    throw new Exception($"{nameof(RuleBuilder)}.{nameof(BuildPresets)}: Unknown type");
            }
        }

        private IRule AllTasksIsCurrentIteration()
        {
            var builder = new WiqlBuilder()
                .AssignedTo()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .WithStates("and", "<>", "and", WorkItemStates.Closed, WorkItemStates.Removed);

            var result = new Rule
            {
                Title = Properties.Resource.AS_Rule_CurrentIteration_Title,
                Operation = RuleOperation.SameCount,
                Source = builder.ToString(),
                Condition = builder.CurrentIteration().ToString()
            };

            return result;
        }

        private IRule CheckTasksAreapath(string name)
        {
            var builder = new WiqlBuilder()
                .AssignedTo()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .WithStates("and", "<>", "and", WorkItemStates.Closed, WorkItemStates.Removed);

            var result = new Rule
            {
                Title= Properties.Resource.AS_Rule_AreaCondition_Title,
                Operation = RuleOperation.SameCount,
                Source = builder.ToString(),
                Condition = builder.WithAreaPath("and", name).ToString()
            };

            return result;
        }

        #endregion

        public RuleBuilder(ITfsApi api)
        {
            this._api = api;
        }

        /// <summary>
        /// Проверяет правило и возвращает неподхдодящие рабочие элементы
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public Dictionary<IRule, IList<WorkItem>> CheckInconsistant(IEnumerable<IRule> rules)
        {
            var toReturn = new Dictionary<IRule, IList<WorkItem>>();

            foreach (var rule in rules)
            {
                var result = ExecuteRule(rule);
                if (result.Any())
                {
                    toReturn[rule] = result;
                }
            }

            return toReturn;
        }

        private IList<WorkItem> ExecuteRule(IRule rule)
        {
            var result = new List<WorkItem>();

            try
            {
                var source = _api.QueryItems(rule.Source);
                var conditional = _api.QueryItems(rule.Condition);

                switch (rule.Operation)
                {
                    // Кол-во должно быть одинаковым
                    case RuleOperation.SameCount:
                        var except = source.Except(conditional, new WorkItemComparer());
                        result.AddRange(except);
                        break;

                    // Кол-во запроса должно быть нулевым
                    case RuleOperation.ZeroCount:
                        // Пока проверяется только первое условие
                        result.AddRange(source);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return result;
        }
    }
}
