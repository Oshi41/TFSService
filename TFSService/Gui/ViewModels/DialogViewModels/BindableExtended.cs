using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using Gui.Helper;
using Mvvm;
using Mvvm.Commands;

namespace Gui.ViewModels.DialogViewModels
{
    public class BindableExtended : BindableBase, IDataErrorInfo
    {
        #region Fields

        /// <summary>
        ///     Единоразовое выполнение
        /// </summary>
        private readonly ActionArbiter _arbiter = new ActionArbiter();

        /// <summary>
        ///     Список ошибок всех проверенных свойств
        /// </summary>
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        private readonly Dictionary<string, string> _optionalErrors = new Dictionary<string, string>();

        private bool _isExecuting;

        #endregion

        #region Properties

        /// <summary>
        ///     Обработчик кнопки подтверждения. Если в кнопке явно выставили Error в true, диалог не закроется
        /// </summary>
        public DelegateCommandBase SubmitCommand { get; protected set; }

        /// <summary>
        /// Специальная команда для третьей кнопки
        /// </summary>
        public DelegateCommandBase SpecialCommand { get; protected set; }

        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                string optional = null;

                _arbiter.Do(() =>
                {
                    _errors[columnName] = ValidateProperty(columnName);
                    optional = ValidateOptionalProperty(columnName);
                    OnPropertyChanged(nameof(Error));
                });

                return optional ?? _errors[columnName];
            }
        }

        /// <summary>
        ///     Если есть ошибочное проперти, возвращаем его
        /// </summary>
        public string Error
        {
            get
            {
                foreach (var prop in _errors)
                    if (!string.IsNullOrEmpty(prop.Value))
                        return prop.Value;

                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Валидация свойства модели
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        protected virtual string ValidateProperty(string prop)
        {
            return null;
        }

        /// <summary>
        ///     Валидация свойства, которое выдаёт некритичную ошибку
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        protected virtual string ValidateOptionalProperty(string prop)
        {
            return null;
        }

        /// <summary>
        ///     Выполняет действие в GUI потоке, в приоритете Loaded
        /// </summary>
        /// <param name="action"></param>
        public void SafeExecute(Action action)
        {
            Dispatcher.CurrentDispatcher.Invoke(action, DispatcherPriority.Loaded);
        }
    }
}