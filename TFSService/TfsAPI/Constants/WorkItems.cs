using System.Collections.Specialized;

namespace TfsAPI.Constants
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

            /// <summary>
            /// Поле рабочего элемента с планируемыми трудозатратами
            /// </summary>
            public const string Planning = "Original Estimate";

            /// <summary>
            /// Поле рабочего элемента и ревизии. Кем было изменено
            /// </summary>
            public const string ChangedBy = "Changed By";
        }

        public static class LinkTypes
        {
            /// <summary>
            /// Тип связи Parent -> Child
            /// </summary>
            public const string ParentLink = "Syste.LinkTypes.Hierarhy";
        }
    }
}
