using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Mvvm.Commands;

namespace Gui.Helper
{
    /// <summary>
    ///     Команда наблюдает за выполнением своей функции. Если функция выполняется, команда недоступна
    /// </summary>
    public class ObservableCommand : DelegateCommandBase, ICommand, INotifyPropertyChanged
    {
        private bool _isExecuting;

        /// <summary>
        ///     Макс. кол-во выполнения функции. Бесконечность при отриц значениях
        /// </summary>
        private int _maxExecutionCount = -1;

        /// <summary>
        ///     Выполняется ли команда в данный момент
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                if (value == _isExecuting)
                    return;

                _isExecuting = value;
                OnPropertyChanged(nameof(IsExecuting));
            }
        }

        /// <summary>
        ///     После выполнения команды
        /// </summary>
        public event EventHandler Executed;

        #region Constructors

        /// <summary>
        ///     Creates a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> with the <see cref="T:System.Action" /> to
        ///     invoke on execution.
        /// </summary>
        /// <param name="executeMethod">
        ///     The <see cref="T:System.Action" /> to invoke when
        ///     <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" /> is called.
        /// </param>
        public ObservableCommand(Action executeMethod)
            : this(executeMethod, () => true, false)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> with the <see cref="T:System.Action" /> to
        ///     invoke on execution
        ///     and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">
        ///     The <see cref="T:System.Action" /> to invoke when
        ///     <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" /> is called.
        /// </param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="T:System.Func`1" /> to invoke when
        ///     <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)" /> is
        ///     called
        /// </param>
        /// <param name="isAutomaticRequeryDisabled"></param>
        public ObservableCommand(Action executeMethod, Func<bool> canExecuteMethod,
            bool isAutomaticRequeryDisabled = false)
            : base(o => executeMethod(), o => canExecuteMethod(), isAutomaticRequeryDisabled)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        private ObservableCommand(Func<Task> executeMethod)
            : this(executeMethod, () => true, false)
        {
        }

        private ObservableCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod,
            bool isAutomaticRequeryDisabled = false)
            : base(o => executeMethod(), o => canExecuteMethod(), isAutomaticRequeryDisabled)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        /// <summary>
        ///     Команда выполнится единократно
        /// </summary>
        /// <returns></returns>
        public ObservableCommand ExecuteOnce()
        {
            _maxExecutionCount = 1;
            return this;
        }

        #endregion

        #region Static

        /// <summary>
        ///     Factory method to create a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> from an awaitable handler
        ///     method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="T:Mvvm.Commands.DelegateCommand" /></returns>
        public static ObservableCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new ObservableCommand(executeMethod);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> from an awaitable handler
        ///     method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="isAutomaticRequeryDisabled"></param>
        /// <returns>Constructed instance of <see cref="T:Mvvm.Commands.DelegateCommand" /></returns>
        public static ObservableCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod,
            bool isAutomaticRequeryDisabled = false)
        {
            return new ObservableCommand(executeMethod, canExecuteMethod, isAutomaticRequeryDisabled);
        }

        #endregion

        #region ICommand

        async void ICommand.Execute(object parameter)
        {
            try
            {
                IsExecuting = true;

                await Execute(parameter);
            }
            finally
            {
                // Вычтем кол-во разрешённых кликов
                if (_maxExecutionCount > 0) _maxExecutionCount--;

                IsExecuting = false;
                Executed?.Invoke(this, EventArgs.Empty);

                // Иначе залагивает
                RaiseCanExecuteChanged();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !IsExecuting
                   && _maxExecutionCount != 0
                   && CanExecute(parameter);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}