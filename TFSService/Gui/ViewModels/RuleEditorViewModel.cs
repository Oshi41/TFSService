using Gui.Helper;
using Gui.ViewModels.Rules;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TfsAPI.Interfaces;
using TfsAPI.RulesNew;

namespace Gui.ViewModels
{
    public class RuleEditorViewModel : BindableBase
    {
        private ObservableCollection<IRule> rules;
        private IRule selected;

        public ObservableCollection<IRule> Rules { get => rules; set => SetProperty(ref rules, value); }

        public IRule Selected { get => selected; set => SetProperty(ref selected, value); }

        public ICommand AddRule { get; }
        public ICommand DeleteRule { get; }
        public ICommand EditRule { get; }

        public bool IsChanged { get; private set; }

        public RuleEditorViewModel(IEnumerable<IRule> rules)
        {
            this.rules = new ObservableCollection<IRule>(rules);

            AddRule = new DelegateCommand(OnAddRule);
            DeleteRule = new DelegateCommand<IRule>(OnDeleteRule, OnCanDeleteRule);
        }

        #region Command handlers

        private void OnAddRule()
        {
            var vm = new AddRuleViewModel();

            if (WindowManager.ShowDialog(vm, Properties.Resources.AS_AddRule_Master_Title, 400, 300) == true)
            {
                var builder = new RuleBuilder();

                // Если выбрали стандартные пресеты
                if (vm.UsePresets)
                {
                    var rule = builder.BuildPresets(vm.Preset);

                    if (Rules.Contains(rule))
                    {
                        if (WindowManager.ShowConfirm(Properties.Resources.AS_ReplaceRule_Asking, Properties.Resources.AS_Replacing) == true)
                        {
                            Rules.Remove(rule);
                        }
                        else
                        {
                            return;
                        }
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
