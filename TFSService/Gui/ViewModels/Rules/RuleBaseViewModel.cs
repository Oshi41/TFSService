using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mvvm;

namespace Gui.ViewModels.Rules
{
    class RuleBaseViewModel : BindableBase
    {
        private string _field;
        private string _operand;
        private string _value;

        private List<string> _fields;
        private List<string> _operands;
        private List<string> _values;

        #region Properties

        public string Field
        {
            get => _field;
            set => SetProperty(ref _field, value);
        }

        public string Operand
        {
            get => _operand;
            set => SetProperty(ref _operand, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public List<string> Fields
        {
            get => _fields;
            set => SetProperty(ref _fields, value);
        }

        public List<string> Operands
        {
            get => _operands;
            set => SetProperty(ref _operands, value);
        }

        public List<string> Values
        {
            get => _values;
            set => SetProperty(ref _values, value);
        }

        #endregion

        public RuleBaseViewModel()
        {
            UpdateFields();
            UpdateOperands();
            UpdateMacros();
        }

        private void UpdateFields()
        {
            _fields = new List<string>
            {
                TfsAPI.Constants.Sql.Fields.AssignedTo,
                TfsAPI.Constants.Sql.Fields.ChangedDate,
                TfsAPI.Constants.Sql.Fields.Description,
                TfsAPI.Constants.Sql.Fields.History,
                TfsAPI.Constants.Sql.Fields.IterationPath,
                TfsAPI.Constants.Sql.Fields.State,
                TfsAPI.Constants.Sql.Fields.Title,
                TfsAPI.Constants.Sql.Fields.WorkItemType,
            };

            OnPropertyChanged(nameof(Fields));
        }

        private void UpdateOperands()
        {
            _operands = new List<string>
            {
                "=",
                "<",
                ">",
                "<>",
                ">=",
                "<=",
                "In",
                "Not In",
                "Was Ever",
                "Contains",
                "Does Not Contain",
                "In Group",
                "Not In Group",
                "Under",
                "Not Under",
            };

            OnPropertyChanged(nameof(Operands));
        }

        private void UpdateMacros()
        {
            _values = new List<string>
            {
                "@me",
                "@CurrentIteration",
                "@Project",
                "@StartOfDay",
                "@StartOfWeek",
                "@StartOfMonth",
                "@StartOfYear",
                "@Today",
                "[Any]",
            };

            OnPropertyChanged(nameof(Values));
        }
    }
}
