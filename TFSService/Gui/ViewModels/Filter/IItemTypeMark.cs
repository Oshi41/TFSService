namespace Gui.ViewModels
{
    public interface IItemTypeMark
    {
        bool IsEnabled { get; set; }
        string Value { get; set; }
        bool IsChecked { get; set; }
    }
}