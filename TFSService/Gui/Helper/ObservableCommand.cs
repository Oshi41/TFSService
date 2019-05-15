using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mvvm.Commands;
using Mvvm.Properties;

namespace Gui.Helper
{
    public class ObservableCommand : DelegateCommandBase, ICommand
    {
        /// <summary>
        ///     Creates a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> with the <see cref="T:System.Action" /> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="T:System.Action" /> to invoke when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" /> is called.</param>
        public ObservableCommand(Action executeMethod)
          : this(executeMethod, () => true, false)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> with the <see cref="T:System.Action" /> to invoke on execution
        ///     and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="T:System.Action" /> to invoke when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" /> is called.</param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="T:System.Func`1" /> to invoke when <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)" /> is
        ///     called
        /// </param>
        /// <param name="isAutomaticRequeryDisabled"></param>
        public ObservableCommand(Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled = false)
          : base(o => executeMethod(), o => canExecuteMethod(), isAutomaticRequeryDisabled)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        private ObservableCommand(Func<Task> executeMethod)
          : this(executeMethod, () => true, false)
        {
        }

        private ObservableCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled = false)
          : base(o => executeMethod(), o => canExecuteMethod(), isAutomaticRequeryDisabled)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="T:Mvvm.Commands.DelegateCommand" /></returns>
        public static ObservableCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new ObservableCommand(executeMethod);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="T:Mvvm.Commands.DelegateCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="isAutomaticRequeryDisabled"></param>
        /// <returns>Constructed instance of <see cref="T:Mvvm.Commands.DelegateCommand" /></returns>
        public static ObservableCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled = false)
        {
            return new ObservableCommand(executeMethod, canExecuteMethod, isAutomaticRequeryDisabled);
        }

        /// <summary>
        /// После выполнения команды
        /// </summary>
        public event EventHandler Executed;

        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);

            Executed?.Invoke(this, EventArgs.Empty);
        }
    }
}
