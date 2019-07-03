using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Microsoft.VisualStudio.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Interfaces;
using TfsAPI.RulesNew;

namespace Gui.ViewModels.Rules
{
    /// <summary>
    /// Мастер добвления правил. НА данный момент только пресеты
    /// </summary>
    public class AddRuleViewModel : BindableExtended
    {
        private bool usePresets = true;
        private StaticRules preset;
        private string presetDescription;
        private string userParameter;

        private readonly Dictionary<StaticRules, string> _descr = new Dictionary<StaticRules, string>
        {
            {StaticRules.AllTasksIsCurrentIteration, Properties.Resources.AS_AllTasksInCurrentIteration_Descr },
            {StaticRules.CheckTasksAreapath, Properties.Resources.AS_CheckArea_Descr },
        };


        public AddRuleViewModel()
        {
            SubmitCommand = new Helper.ObservableCommand(OnSaveRule);
            Preset = default(StaticRules);
        }

        private void OnSaveRule()
        {

        }

        #region Properties

        /// <summary>
        /// Используем стандартные правила
        /// </summary>
        public bool UsePresets { get => usePresets; set => SetProperty(ref usePresets, value); }

        /// <summary>
        /// Какой стандартный пресе выбрали
        /// </summary>
        public StaticRules Preset
        {
            get => preset;
            set
            {
                SetProperty(ref preset, value);
                // Обновляем описание
                PresetDescription = _descr[value];
            }
        }

        public string PresetDescription { get => presetDescription; set => SetProperty(ref presetDescription, value); }

        public string UserParameter { get => userParameter; set => SetProperty(ref userParameter, value); }

        #endregion
    }
}
