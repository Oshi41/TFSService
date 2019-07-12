using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Gui.ViewModels.Rules;
using Mvvm;
using Mvvm.Commands;
using TfsAPI.RulesNew;

namespace Gui.ViewModels
{
    /// <summary>
    ///     Отображение списка моих правил рабочих элементов
    /// </summary>
    public class RuleEditorViewModel : BindableBase
    {
        private ObservableCollection<IRule> _rules;
        private IRule _selected;

        public RuleEditorViewModel(IEnumerable<IRule> rules)
        {
            _rules = new ObservableCollection<IRule>(rules);

            AddRule = new DelegateCommand(OnAddRule);
            DeleteRule = new DelegateCommand<IRule>(OnDeleteRule, OnCanDeleteRule);
        }

        public ObservableCollection<IRule> Rules
        {
            get => _rules;
            set => SetProperty(ref _rules, value);
        }

        public IRule Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public ICommand AddRule { get; }
        public ICommand DeleteRule { get; }
        public ICommand EditRule { get; }

        public bool IsChanged { get; private set; }

        #region Command handlers

        private void OnAddRule()
        {
            var vm = new AddRuleViewModel();

            if (WindowManager.ShowDialog(vm, Resources.AS_AddRule_Master_Title, 400, 300) == true)
            {
                var builder = new RuleBuilder();

                // Если выбрали стандартные пресеты
                if (vm.UsePresets)
                {
                    var rule = builder.BuildPresets(vm.Preset, vm.GetParameter());

                    if (Rules.Contains(rule))
                    {
                        if (WindowManager.ShowConfirm(Resources.AS_ReplaceRule_Asking, Resources.AS_Replacing) == true)
                            Rules.Remove(rule);
                        else
                            return;
                    }

                    Rules.Add(rule);
                    IsChanged = true;
                }
            }
        }

        private void OnDeleteRule(IRule rule)
        {
            Rules.Remove(rule);
            IsChanged = true;
        }

        private bool OnCanDeleteRule(IRule rule)
        {
            return rule != null && Rules.Contains(rule);
        }

        #endregion
    }
}