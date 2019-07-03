namespace TfsAPI.Interfaces
{
    /// <summary>
    /// Нечто, содержащее процесс, который можно стартовать и ставить на паузу
    /// </summary>
    public interface IResumable
    {
        /// <summary>
        /// Запустить выполнение
        /// </summary>
        void Start();

        /// <summary>
        /// Поставить выполнение на паузу
        /// </summary>
        void Pause();
    }
}