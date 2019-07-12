using System.Collections.Generic;
using Gui.Helper;
using Gui.Properties;
using Gui.ViewModels.DialogViewModels;
using TfsAPI.RulesNew;
using TfsAPI.RulesNew.RuleParameter;

namespace Gui.ViewModels.Rules
{
    /// <summary>
    ///     Мастер добвления правил. НА данный момент только пресеты
    /// </summary>
    public class AddRuleViewModel : BindableExtended
    {
        private readonly Dictionary<StaticRules, string> _descr = new Dictionary<StaticRules, string>
        {
            {StaticRules.AllTasksIsCurrentIteration, Resources.AS_AllTasksInCurrentIteration_Descr},
            {StaticRules.CheckTasksAreapath, Resources.AS_CheckArea_Descr}
        };

        private StaticRules _preset;
        private string _presetDescription;
        private bool _usePresets = true;
        private string _userParameter;


        public AddRuleViewModel()
        {
            SubmitCommand = new ObservableCommand(OnSaveRule);
            Preset = default;
        }

        private void OnSaveRule()
        {
        }

        #region Properties

        /// <summary>
        ///     Используем стандартные правила
        /// </summary>
        public bool UsePresets
        {
            get => _usePresets;
            set => SetProperty(ref _usePresets, value);
        }

        /// <summary>
        ///     Какой стандартный пресе выбрали
        /// </summary>
        public StaticRules Preset
        {
            get => _preset;
            set
            {
                SetProperty(ref _preset, value);
                // Обновляем описание
                PresetDescription = _descr[value];
            }
        }

        public string PresetDescription
        {
            get => _presetDescription;
            set => SetProperty(ref _presetDescription, value);
        }

        public string UserParameter
        {
            get => _userParameter;
            set => SetProperty(ref _userParameter, value);
        }

        #endregion

        public IRuleParameter GetParameter()
        {
            switch (Preset)
            {
                case StaticRules.CheckTasksAreapath:
                    return new AreaPathParameter(UserParameter);

                default:
                    return null;
            }
        }
    }
}