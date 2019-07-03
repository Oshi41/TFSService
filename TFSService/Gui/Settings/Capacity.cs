using Mvvm;

namespace Gui.Settings
{
    /// <summary>
    ///     DTO для json, описывает трудозатраты пользователя
    /// </summary>
    public class Capacity : BindableBase
    {
        private bool _byUser;
        private int _hours;

        /// <summary>
        ///     Кол-во часов в сутки
        /// </summary>
        public int Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        /// <summary>
        ///     Задано ли кол-во часов пользователем
        /// </summary>
        public bool ByUser
        {
            get => _byUser;
            set => SetProperty(ref _byUser, value);
        }
    }
}