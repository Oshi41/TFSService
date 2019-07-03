using Mvvm;

namespace Gui.Settings
{
    /// <summary>
    /// DTO для json, описывает трудозатраты пользователя
    /// </summary>
    public class Capacity : BindableBase
    {
        private int hours;
        private bool byUser;

        /// <summary>
        /// Кол-во часов в сутки
        /// </summary>
        public int Hours { get => hours; set => SetProperty(ref hours, value); }

        /// <summary>
        /// Задано ли кол-во часов пользователем
        /// </summary>
        public bool ByUser { get => byUser; set => SetProperty(ref byUser, value); }
    }
}
