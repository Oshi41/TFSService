namespace TfsAPI.Constants
{
    /// <summary>
    ///     Статус рабочего элемента
    /// </summary>
    public static class WorkItemStates
    {
        public const string Active = nameof(Active);
        public const string Closed = nameof(Closed);
        public const string New = nameof(New);
        public const string Removed = nameof(Removed);
        public const string Resolved = nameof(Resolved);
        public const string Requested = nameof(Requested);
    }
}