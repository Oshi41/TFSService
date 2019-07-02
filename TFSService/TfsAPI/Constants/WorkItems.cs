namespace TfsAPI.Constants
{
    public static class WorkItems
    {
        public static class Fields
        {
            /// <summary>
            ///     Поле рабочего элемента с оставшимся временем
            /// </summary>
            public const string Remaining = "Remaining Work";

            /// <summary>
            ///     Поле рабочего элемента со списанным временем
            /// </summary>
            public const string Complited = "Completed Work";

            /// <summary>
            ///     Поле рабочего элемента с планируемыми трудозатратами
            /// </summary>
            public const string Planning = "Original Estimate";

            /// <summary>
            ///     Поле рабочего элемента и ревизии. Кем было изменено
            /// </summary>
            public const string ChangedBy = "Changed By";

            /// <summary>
            ///     Причина закрытия. Для Code Response см
            /// </summary>
            public const string ClosedStatus = "Closed Status";
        }

        public static class LinkTypes
        {
            /// <summary>
            ///     Тип связи Parent -> Child
            /// </summary>
            public const string ParentLink = "System.LinkTypes.Hierarchy";
        }

        public static class ClosedStatus
        {
            /// <summary>
            ///     Все ок
            /// </summary>
            public static string LooksGood = "Looks Good";

            /// <summary>
            ///     В ходе проверки найдено несколько замечаний
            /// </summary>
            public static string WithComments = "With Comments";

            /// <summary>
            ///     Требуется доработка
            /// </summary>
            public static string NeedsWork = "Needs Work";

            /// <summary>
            ///     Отклонён
            /// </summary>
            public static string Declined = nameof(Declined);
        }
    }
}