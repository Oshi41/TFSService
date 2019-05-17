﻿namespace TfsAPI.Constants
{
    public static class Sql
    {
        /// <summary>
        /// Имена полей для SQL запросов
        /// </summary>
        public static class Fields
        {
            /// <summary>
            /// Поле для типа рабочго элемента
            /// </summary>
            public const string WorkItemType = "[Work Item Type]";

            /// <summary>
            /// Поле для статуса рабочего элемента
            /// </summary>
            public const string State = "[State]";

            /// <summary>
            /// Поле для описания рабочего элемента
            /// </summary>
            public const string Description = "[Description]";

            /// <summary>
            /// Поле для истории рабочего элемента
            /// </summary>
            public const string History = "[History]";

            /// <summary>
            /// Поле заголовка рабочего элемента
            /// </summary>
            public const string Title = "[Title]";

            /// <summary>
            /// Поле содержащее время последнего изменения рабочего элемента
            /// </summary>
            public const string ChangedDate = "[Changed Date]";
        }

        /// <summary>
        /// Имена таблиц для поиска
        /// </summary>
        public static class Tables
        {
            /// <summary>
            /// Имя БД с рабочими элементами
            /// </summary>
            public const string WorkItems = "WorkItems";
        }

        /// <summary>
        /// Условие для поиска рабочих элементов на мне
        /// </summary>
        public const string AssignedToMeCondition = "[Assigned To] = @me";

        /// <summary>
        /// Условие, что рабочий элемент когда-то был изменен мной
        /// </summary>
        public const string WasEverChangedByMeCondition = "ever [Changed By] = @me";

        /// <summary>
        /// Операнд для поиска строки
        /// </summary>
        public const string ContainsStrOperand = "Contains Words";
    }
}