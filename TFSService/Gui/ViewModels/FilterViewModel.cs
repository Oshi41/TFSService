using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Mvvm;

namespace Gui.ViewModels
{
    public class FilterViewModel : BindableBase
    {
        public FilterViewModel(params ItemTypeMark[] types)
        {
            Marks = new ObservableCollection<ItemTypeMark>(types ?? new ItemTypeMark[] { });

            Marks.CollectionChanged += NotifyChanges;

            // Подписываемся на события
            NotifyChanges(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Marks));
        }

        public ObservableCollection<ItemTypeMark> Marks { get; }

        public event EventHandler FilterChanged;

        public string[] GetSelectedTypes()
        {
            return Marks
                .Where(x => x.IsChecked)
                .Select(x => x.WorkType)
                .ToArray();
        }

        private void NotifyChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            var old = e?.OldItems?.OfType<INotifyPropertyChanged>().ToList();
            var added = e?.NewItems?.OfType<INotifyPropertyChanged>().ToList();

            if (!old.IsNullOrEmpty()) old.ForEach(x => x.PropertyChanged -= MarkDirty);

            if (!added.IsNullOrEmpty()) added.ForEach(x => x.PropertyChanged += MarkDirty);
        }

        private void MarkDirty(object sender, PropertyChangedEventArgs e)
        {
            if (Marks.All(x => !x.IsChecked) && sender is ItemTypeMark mark)
                // Насильно включаем последний элемент
                mark.IsChecked = true;

            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ItemTypeMark : BindableBase
    {
        private bool _isChecked;
        private bool _isEnabled;
        private string _workType;

        public ItemTypeMark(string workType, bool isChecked = true, bool enable = true)
        {
            WorkType = workType;
            IsEnabled = enable;
            IsChecked = isChecked;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string WorkType
        {
            get => _workType;
            set => SetProperty(ref _workType, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public static implicit operator ItemTypeMark(string name)
        {
            return new ItemTypeMark(name);
        }
    }
}