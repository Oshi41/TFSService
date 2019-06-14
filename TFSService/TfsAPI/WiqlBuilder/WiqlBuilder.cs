using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Constants;

namespace TfsAPI.Rules
{
    public class WiqlBuilder
    {
        private readonly List<string> _queries = new List<string>();
        private readonly bool _isEmpty;

        public static WiqlBuilder Empty => new WiqlBuilder(true);

        private WiqlBuilder(bool isEmpty)
        {
            _isEmpty = isEmpty;
        }

        public WiqlBuilder(string tableName = Sql.Tables.WorkItems)
            : this(false)
        {
            _queries.Add($"select * from {tableName} ");
        }

        #region Methods

        public WiqlBuilder AssignedTo(string operand = "and", string name = "@me")
        {
            name = CheckMacros(name);

            AddCondition(operand, $"{Sql.Fields.AssignedTo} = {name}");
            return this;
        }

        public WiqlBuilder EverChangedBy(string operand, string name = "@me")
        {
            name = CheckMacros(name);

            AddCondition(operand, $"[Changed By] ever {name}");
            return this;
        }

        public WiqlBuilder CurrentIteration(string operand = "and")
        {
            AddCondition(operand, Sql.IsCurrentIteractionCondition);
            return this;
        }

        public WiqlBuilder WithItemTypes(string operand, string op, params string[] types)
        {
            if (!types.IsNullOrEmpty())
            {
                var result = string.Join(" or ", types.Select(x => $"{Sql.Fields.WorkItemType} {op} '{x}'"));

                if (types.Count() > 1)
                {
                    result = $"( {result} )";
                }

                AddCondition(operand, result);
            }

            return this;
        }

        public WiqlBuilder WithConditions(string operand, WiqlBuilder cond)
        {
            var result = cond.ToString();
            if (cond._queries.Count > 1)
            {
                result = $"( {result} )";
            }

            AddCondition(operand, result);
            return this;
        }

        /// <summary>
        /// Условие типа элемента
        /// </summary>
        /// <param name="clause">Основное условие</param>
        /// <param name="operand">Операнд в условии типа элемента</param>
        /// <param name="stateClause">Как каждое условие типа элемента соединяется с другим</param>
        /// <param name="states">Список состояний</param>
        /// <returns></returns>
        public WiqlBuilder WithStates(string clause, string operand, string stateClause, params string[] states)
        {
            if (!states.IsNullOrEmpty())
            {
                var result = string.Join($" {stateClause} ", states.Select(x => $"{Sql.Fields.State} {operand} '{x}'"));

                if (states.Count() > 1)
                {
                    result = $"( {result} )";
                }

                AddCondition(clause, result);
            }

            return this;
        }

        public WiqlBuilder ContainsInFields(string operand, string text, params string[] fields)
        {
            if (!fields.IsNullOrEmpty())
            {
                var result = string.Join(" or ", fields.Select(x => $"{x} {Sql.ContainsStrOperand} '{text}'"));

                if (fields.Count() > 1)
                {
                    result = $"({result})";
                }

                AddCondition(operand, result);
            }

            return this;
        }

        public WiqlBuilder ChangedDate(string operand, DateTime date, string op)
        {
            AddCondition(operand, $"{Sql.Fields.ChangedDate} {op} '{date.ToShortDateString().Replace(".", "/")}'");
            return this;
        }

        public WiqlBuilder WithAreaPath(string operand, string area)
        {
            AddCondition(operand, $"{Sql.Fields.AreaPath} = {area}");
            return this;
        }

        #endregion

        #region Private
        private void AddCondition(string clause, string cond)
        {
            var result = string.Empty;

            switch (_queries.Count)
            {
                case 0:
                    result = cond;
                    break;

                case 1 when !_isEmpty:
                    result = $"where {cond}";
                    break;

                default:
                    result = $"{clause} {cond}";
                    break;
            }

            _queries.Add(result);
        }

        private string CheckMacros(string value)
        {
            if (_macros.Contains(value.ToLower()))
            {
                return value;
            }

            return $"'{value}'";
        }

        private static readonly List<string> _macros = new List<string>
        {
            WiqlOperators.cMacroCurrentIteration,
            WiqlOperators.cMacroMe,
            WiqlOperators.cMacroToday,
        };
        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", _queries);
        }

        #endregion
    }
}
