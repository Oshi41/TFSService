using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Gui.Helper;
using Mvvm.Commands;

namespace Gui.Behaviors
{
    public class DataGridRowDetailsBehavior : Behavior<DataGrid>
    {
        public DataGridRowDetailsBehavior()
        {
            ExpandCommand = new ObservableCommand(OnExpand, o => o is IList { Count: > 0 });
        }

        private void OnExpand(object obj)
        {
            foreach (var dataItem in obj as IList)
            {
                if (AssociatedObject.ItemContainerGenerator.ContainerFromItem(dataItem) is DataGridRow row)
                {
                    Switch(row);
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDoubleClick += OnDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseDoubleClick -= OnDoubleClick;
        }

        public ICommand ExpandCommand { get; }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gridRow = WpfUtil.TryFindParent<DataGridRow>(e.OriginalSource as DependencyObject);

            if (gridRow != null)
            {
                var parent = WpfUtil.TryFindParent<DataGrid>(gridRow);

                if (Equals(parent, AssociatedObject))
                {
                    Switch(gridRow);
                }
            }
        }

        private void Switch(DataGridRow row)
        {
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}