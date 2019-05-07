using System;
using System.Collections.Generic;
using Mvvm;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Mvvm.Commands;

namespace Gui.ViewModels
{
    public class BindableExtended : BindableBase, IDataErrorInfo
    {
        /// <summary>
        /// Список ошибок всех проверенных свойств
        /// </summary>
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        /// <summary>
        /// Единоразовое выполнение
        /// </summary>
        private readonly ActionArbiter _arbiter = new ActionArbiter();

        public string this[string columnName]
        {
            get
            {
                _arbiter.Do(() =>
                {
                    _errors[columnName] = ValidateProperty(columnName);
                    OnPropertyChanged(nameof(Error));
                });

                return _errors[columnName];
            }
        }

        /// <summary>
        /// Если есть ошибочное проперти, возвращаем его
        /// </summary>
        public string Error
        {
            get
            {
                foreach (var prop in _errors)
                {
                    if (!string.IsNullOrEmpty(prop.Value))
                        return prop.Value;
                }

                return null;
            }
        }

        /// <summary>
        /// Валидация свойства модели
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        protected virtual string ValidateProperty(string prop)
        {
            return null;
        }

        /// <summary>
        /// Обработчик кнопки подтверждения. Если в кнопке явно выставили Error в true, диалог не закроется
        /// </summary>
        public ICommand SubmitCommand { get; protected set; }

    }
}
