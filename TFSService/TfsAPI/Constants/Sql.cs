namespace TfsAPI
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
    }
}