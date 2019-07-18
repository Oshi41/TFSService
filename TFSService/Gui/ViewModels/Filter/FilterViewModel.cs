using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Mvvm;
using Newtonsoft.Json;
using TfsAPI.Constants;

namespace Gui.ViewModels
{
    public class FilterViewModel : BindableBase
    {
        [JsonConstructor]
        public FilterViewModel(params ItemTypeMark[] marks)
        {
            Marks = new ObservableCollection<ItemTypeMark>(marks ?? new ItemTypeMark[]
            {
                WorkItemTypes.Task,
                WorkItemTypes.Pbi,
                WorkItemTypes.Bug,
                WorkItemTypes.Improvement,
                WorkItemTypes.Incident,
                WorkItemTypes.Feature,
                WorkItemTypes.CodeReview,
                WorkItemTypes.ReviewResponse,
            });

            Marks.CollectionChanged += NotifyChanges;

            // Подписываемся на события
            NotifyChanges(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Marks));
        }

        // Конструктор копирования
        public FilterViewModel(FilterViewModel source)
            : this(source?.Marks?.ToArray())
        {
            
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
}