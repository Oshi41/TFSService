namespace TfsAPI.Constants
{
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
        public const string CodeReview = "Code Review Request";
        public const string ReviewRequest = "Code Review Response";
    }
}