using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf.Transitions;
using Mvvm.Commands;

namespace Gui.View.Rule_Wizard
{
    /// <summary>
    ///     Interaction logic for WizardCommands.xaml
    /// </summary>
    public partial class WizardCommands : UserControl
    {
        public WizardCommands()
        {
            InitializeComponent();

            Loaded += SubscribeOnce;
        }

        private void SubscribeOnce(object sender, RoutedEventArgs e)
        {
            Loaded += SubscribeOnce;

            if (PrevCommand == null)
                PrevCommand = new DelegateCommand(() => Navigate(false),
                    () => CanExecuteByCondition(CanPrev));

            if (NextCommand == null)
                NextCommand = new DelegateCommand(() => Navigate(true),
                    () => CanExecuteByCondition(CanNext));

            ChangeButtonsView();
        }

        private void ChangeButtonsView(object sender = null, EventArgs e = null)
        {
            if (Transitioner == null)
            {
                ShowOnly();
                return;
            }

            var index = Transitioner.SelectedIndex;
            var max = Transitioner.Items.Count;

            // Стоим в конце
            if (index == max - 1)
            {
                ShowOnly(Last, Prev);
            }
            else
            {
                // стоим в начале
                if (index == 0)
                    ShowOnly(First);
                else
                    ShowOnly(Prev, Next);
            }
        }

        #region Dependency Properties

        public Transitioner Transitioner
        {
            get => (Transitioner) GetValue(TransitionerProperty);
            set => SetValue(TransitionerProperty, value);
        }

        // Using a DependencyProperty as the backing store for Transitioner.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransitionerProperty =
            DependencyProperty.Register("Transitioner", typeof(Transitioner), typeof(WizardCommands),
                new FrameworkPropertyMetadata(OnSubscribe));

        private static void OnSubscribe(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WizardCommands commands)
            {
                if (e.OldValue is Transitioner old) old.SelectionChanged -= commands.ChangeButtonsView;

                if (e.NewValue is Transitioner newer) newer.SelectionChanged += commands.ChangeButtonsView;
            }
        }

        public ICommand NextCommand
        {
            get => (ICommand) GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for NextCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(WizardCommands),
                new FrameworkPropertyMetadata(OnSubscribeForward));

        private static void OnSubscribeForward(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WizardCommands commands)
            {
                if (e.OldValue is ICommand old)
                {
                    var toRemove = commands.CommandBindings.OfType<CommandBinding>()
                        .FirstOrDefault(x => x.Command == old);
                    if (toRemove != null) commands.CommandBindings.Remove(toRemove);
                }

                if (e.NewValue is ICommand newer)
                    commands.CommandBindings.Add(new CommandBinding(newer, commands.OnNext, commands.OnCanNext));
            }
        }

        public ICommand PrevCommand
        {
            get => (ICommand) GetValue(PrevCommandProperty);
            set => SetValue(PrevCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for PrevCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrevCommandProperty =
            DependencyProperty.Register("PrevCommand", typeof(ICommand), typeof(WizardCommands),
                new FrameworkPropertyMetadata(OnSubscribeBack));

        private static void OnSubscribeBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WizardCommands commands)
            {
                if (e.OldValue is ICommand old)
                {
                    var toRemove = commands.CommandBindings.OfType<CommandBinding>()
                        .FirstOrDefault(x => x.Command == old);
                    if (toRemove != null) commands.CommandBindings.Remove(toRemove);
                }

                if (e.NewValue is ICommand newer)
                    commands.CommandBindings.Add(new CommandBinding(newer, commands.OnPrev, commands.OnCanPrev));
            }
        }

        #endregion

        #region Command handlers

        private void OnCanNext(object sender, CanExecuteRoutedEventArgs e)
        {
            HandleCanExecuteAction(e, CanNext);
        }

        private void OnNext(object sender, ExecutedRoutedEventArgs e)
        {
            Navigate(true);
        }

        private void OnCanPrev(object sender, CanExecuteRoutedEventArgs e)
        {
            HandleCanExecuteAction(e, CanPrev);
        }

        private void OnPrev(object sender, ExecutedRoutedEventArgs e)
        {
            Navigate(false);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            CloseWithResult(false);
        }

        #endregion

        #region Static Helping

        private static CanExecuteNavigation CanNext
        {
            get { return (index, max) => index <= max; }
        }

        private static CanExecuteNavigation CanPrev
        {
            get { return (index, max) => index > 0; }
        }

        #endregion

        #region Heping

        /// <summary>
        ///     Функция CanExecute для кнопок навигации
        /// </summary>
        /// <param name="index">На какой странице находимся (начиная с нуля)</param>
        /// <param name="max">Обзее кол-во страниц</param>
        /// <returns></returns>
        public delegate bool CanExecuteNavigation(int index, int max);

        private void HandleCanExecuteAction(CanExecuteRoutedEventArgs e, CanExecuteNavigation action)
        {
            if (!CanExecuteByCondition(action))
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private bool CanExecuteByCondition(CanExecuteNavigation action)
        {
            var index = Transitioner?.SelectedIndex;
            var max = Transitioner?.Items?.Count;

            return index.HasValue && max.HasValue && action(index.Value, max.Value);
        }

        private void Navigate(bool forward)
        {
            if (Transitioner == null)
                return;

            // Закрываем окно
            if (Transitioner.SelectedIndex == Transitioner.Items.Count - 1 && forward)
            {
                CloseWithResult(true);
                return;
            }

            Transitioner.SelectedIndex += forward ? 1 : -1;

            ChangeButtonsView();
        }

        private void CloseWithResult(bool result)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            window.DialogResult = result;
            window.Close();
        }

        private void ShowOnly(params Button[] buttons)
        {
            var list = new[] {First, Last, Next, Prev};

            foreach (var button in list.Except(buttons)) button.Visibility = Visibility.Collapsed;

            foreach (var button in buttons) button.Visibility = Visibility.Visible;
        }

        #endregion
    }
}