namespace TfsAPI
{
    public static class WorkItems
    {
        public static class Fields
        {
            /// <summary>
            /// Поле рабочего элемента с оставшимся временем
            /// </summary>
            public const string Remaining = "Remaining Work";

            /// <summary>
            /// Поле рабочего элемента со списанным временем
            /// </summary>
            public const string Complited = "Completed Work";
        }
    }

    /// <summary>
    /// Типы элементов
    /// </summary>
    public static class WorkItemTypes
    {
        public const string Task = nameof(Task);
        public const string Bug = nameof(Bug);
        public const string ChangeSet = nameof(ChangeSet);
        public const string Pbi = "Product Backlog Item";
        public const string Incident = nameof(Incident);
        public const string Improvement = nameof(Improvement);
    }

    /// <summary>
    /// Статус рабочего элемента
    /// </summary>
    public static class WorkItemStates
    {
        public const string Active = nameof(Active);
        public const string Closed = nameof(Closed);
        public const string New = nameof(New);
    }

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
