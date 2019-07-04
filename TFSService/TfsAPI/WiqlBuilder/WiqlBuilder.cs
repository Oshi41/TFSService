using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Common;
using TfsAPI.Constants;

namespace TfsAPI.Rules
{
    /// <summary>
    ///     Строитель WIQL запросов
    /// </summary>
    public class WiqlBuilder
    {
        private readonly bool _isEmpty;

        // Список условий
        private readonly List<string> _queries = new List<string>();

        private WiqlBuilder(bool isEmpty)
        {
            _isEmpty = isEmpty;
        }

        /// <param name="tableName">Имя таблицы с рабочими элемнетами. Не значете что делать, не трогайте</param>
        public WiqlBuilder(string tableName = Sql.Tables.WorkItems)
            : this(false)
        {
            _queries.Add($"select * from {tableName} ");
        }

        /// <summary>
        ///     Пустой Builder для создания сложных условий <see cref="WithConditions" />
        /// </summary>
        public static WiqlBuilder Empty => new WiqlBuilder(true);

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", _queries);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Назначен на
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="name">Имя пользователя. Если не указать, считается, что запрашивает залогиненый Windows пользователь</param>
        /// <returns></returns>
        public WiqlBuilder AssignedTo(string operand = "and", string name = WiqlOperators.cMacroMe)
        {
            name = WrapValue(name);

            AddCondition(operand, $"{Sql.Fields.AssignedTo} = {name}");
            return this;
        }

        /// <summary>
        ///     Кем когда либо был изменен
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="name">Имя пользователя. Если не указать, считается, что запрашивает залогиненый Windows пользователь</param>
        /// <returns></returns>
        public WiqlBuilder EverChangedBy(string operand, string name = WiqlOperators.cMacroMe)
        {
            name = WrapValue(name);

            AddCondition(operand, $"[Changed By] ever {name}");
            return this;
        }

        /// <summary>
        ///     Элементы в текущей итерации
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <returns></returns>
        public WiqlBuilder CurrentIteration(string operand = "and")
        {
            AddCondition(operand, $"{Sql.Fields.IterationPath} = {WiqlOperators.cMacroCurrentIteration}");
            return this;
        }

        /// <summary>
        ///     Проверка типов рабочих элементов
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="op">
        ///     Операнд между типами рабочих элементов.
        ///     <para>К примеру, если задать "or", то Task or Bug or PBI</para>
        /// </param>
        /// <param name="types">Типы рабочих элементов <see cref="WorkItemTypes" /></param>
        /// <returns></returns>
        public WiqlBuilder WithItemTypes(string operand, string op, params string[] types)
        {
            if (!types.IsNullOrEmpty())
            {
                var result = string.Join(" or ", types.Select(x => $"{Sql.Fields.WorkItemType} {op} '{x}'"));

                if (types.Count() > 1) result = $"( {result} )";

                AddCondition(operand, result);
            }

            return this;
        }

        /// <summary>
        ///     Делаем сложное условие (со скобочками)
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="cond">Builder с условием</param>
        /// <returns></returns>
        public WiqlBuilder WithConditions(string operand, WiqlBuilder cond)
        {
            var result = cond.ToString();
            if (cond._queries.Count > 1) result = $"( {result} )";

            AddCondition(operand, result);
            return this;
        }

        /// <summary>
        ///     Условие состояния рабочего элемента
        /// </summary>
        /// <param name="clause">Основное условие</param>
        /// <param name="operand">
        ///     Операнд в условии типа элемента
        ///     <para>К примеру, если задать "or", то Active or Closed or New</para>
        /// </param>
        /// <param name="stateClause">
        ///     Как каждое условие типа элемента соединяется с другим
        ///     <para>Если задать "=", то Task.State = Active</para>
        /// </param>
        /// <param name="states">Список состояний</param>
        /// <returns></returns>
        public WiqlBuilder WithStates(string clause, string operand, string stateClause, params string[] states)
        {
            if (!states.IsNullOrEmpty())
            {
                var result = string.Join($" {stateClause} ", states.Select(x => $"{Sql.Fields.State} {operand} '{x}'"));

                if (states.Count() > 1) result = $"( {result} )";

                AddCondition(clause, result);
            }

            return this;
        }

        /// <summary>
        ///     поиск по вхождению строки в текстовых полях рабочего элемнета
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="text">Что ищшем</param>
        /// <param name="fields">В каких полях <see cref="WorkItems.Fields" /></param>
        /// <returns></returns>
        public WiqlBuilder ContainsInFields(string operand, string text, params string[] fields)
        {
            if (!fields.IsNullOrEmpty())
            {
                var result = string.Join(" or ", fields.Select(x => $"{x} {Sql.ContainsStrOperand} '{text}'"));

                if (fields.Count() > 1) result = $"({result})";

                AddCondition(operand, result);
            }

            return this;
        }

        /// <summary>
        ///     Условие по дате изменения
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="date">Время</param>
        /// <param name="op">
        ///     Операнд сравнения. "=", "<>" и т.д
        /// </param>
        /// <returns></returns>
        public WiqlBuilder ChangedDate(string operand, DateTime date, string op)
        {
            AddCondition(operand, $"{Sql.Fields.ChangedDate} {op} '{date.ToShortDateString().Replace(".", "/")}'");
            return this;
        }

        /// <summary>
        ///     Учловие области рабочего элемента
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="area">Имя области</param>
        /// <returns></returns>
        public WiqlBuilder WithAreaPath(string operand, string area)
        {
            AddCondition(operand, $"{Sql.Fields.AreaPath} = {area}");
            return this;
        }

        /// <summary>
        ///     Поиск по ID
        /// </summary>
        /// <param name="operand">Операнд условия операции</param>
        /// <param name="number">ID элемента</param>
        /// <returns></returns>
        public WiqlBuilder WithNumber(string operand, int number)
        {
            AddCondition(operand, $"{Sql.Fields.Id} = {number}");
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

        /// <summary>
        ///     Оборачиваю значение кавычками, если это не макрос
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string WrapValue(string value)
        {
            if (Macros.Contains(value.ToLower())) return value;

            return $"'{value}'";
        }

        private static readonly List<string> Macros = new List<string>
        {
            WiqlOperators.cMacroCurrentIteration,
            WiqlOperators.cMacroMe,
            WiqlOperators.cMacroToday
        };

        #endregion
    }
}