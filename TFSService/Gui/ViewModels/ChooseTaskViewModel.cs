using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mvvm.Commands;
using TfsAPI;

namespace Gui.ViewModels
{
    class ChooseTaskViewModel : BindableExtended
    {
        private readonly ITfs _tfs;
        private WorkItemSearcher _searcher;

        public ChooseTaskViewModel(ITfs tfs)
        {
            _tfs = tfs;

            Searcher = new WorkItemSearcher(tfs, WorkItemTypes.Task, WorkItemTypes.Pbi)
            {
                Help = "Выберите рабочий элемент:"
            };

            SpecialCommand = new DelegateCommand(OnCreate);

        }

        private void OnCreate()
        {
            
        }

        public WorkItemSearcher Searcher
        {
            get => _searcher;
            set => SetProperty(ref _searcher, value);
        }
    }
}
