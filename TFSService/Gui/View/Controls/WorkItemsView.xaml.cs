using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gui.ViewModels;

namespace Gui.View.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemsView.xaml
    /// </summary>
    public partial class WorkItemsView : UserControl
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IList<WorkItemVm>), typeof(WorkItemsView), new PropertyMetadata(default(IList<WorkItemVm>)));

        public IList<WorkItemVm> Items
        {
            get { return (IList<WorkItemVm>) GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
            "ViewMode", typeof(VisibleMode), typeof(WorkItemsView), new PropertyMetadata(default(VisibleMode)));

        public VisibleMode ViewMode
        {
            get { return (VisibleMode) GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        public static readonly DependencyProperty ItemMenuProperty = DependencyProperty.Register(
            "ItemMenu", typeof(ContextMenu), typeof(WorkItemsView), new PropertyMetadata(default(ContextMenu)));

        public ContextMenu ItemMenu
        {
            get { return (ContextMenu) GetValue(ItemMenuProperty); }
            set { SetValue(ItemMenuProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(
            "Selected", typeof(WorkItemVm), typeof(WorkItemsView),
            new FrameworkPropertyMetadata(default(WorkItemVm), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public WorkItemVm Selected
        {
            get { return (WorkItemVm) GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public WorkItemsView()
        {
            InitializeComponent();
        }
    }
}
