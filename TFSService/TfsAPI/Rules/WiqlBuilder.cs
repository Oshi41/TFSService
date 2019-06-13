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
        private string s;

        public WiqlBuilder(string tableName = Sql.Tables.WorkItems)
        {
            s = $"select * from {tableName} ";
        }

        #region Methods

        public WiqlBuilder AssignedToMe(string operand = "and")
        {
            s = string.Join($" {operand}", s, Sql.AssignedToMeCondition);
            return this;
        }

        public WiqlBuilder CurrentIteration(string operand = "and")
        {
            s = string.Join($" {operand}", s, Sql.IsCurrentIteractionCondition);
            return this;
        }

        public WiqlBuilder WithItemTypes(string operand, string op, params string[] types)
        {
            s += " " + operand;

            var t = string.Join(" or", types.Select(x => $"{Sql.Fields.WorkItemType} {op} {x}"));

            s += $" ({t})";

            return this;
        }

        public WiqlBuilder WithConditions(string operand, WiqlBuilder cond)
        {
            s = string.Join($" {operand}", s, $"({cond})");
            return this;
        }

        public WiqlBuilder WithStates(string operand, string op, params string[] states)
        {
            s += " " + operand;

            var stats = string.Join(" or", states.Select(x => $"{Sql.Fields.State} {op} {x}"));

            s += $" ({stats})";

            return this;
        }


        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return s;
        }

        #endregion
    }
}
