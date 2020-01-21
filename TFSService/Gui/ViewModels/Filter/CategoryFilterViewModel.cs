using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Mvvm;
using Newtonsoft.Json;

namespace Gui.ViewModels.Filter
{
    public class CategoryFilterViewModel : BindableBase
    {
        
        private bool _isEnable;
        private readonly bool _shouldRestrictNotSelected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title">Название категории</param>
        /// <param name="marks">Список возможных вхождений/категорий фильтров</param>
        /// <param name="isEnable"></param>
        /// <param name="shouldRestrictNotSelected">Если true, в списке всегда будет выбран хотя бы один элемент</param>
        [JsonConstructor]
        public CategoryFilterViewModel(string title, 
            IEnumerable<ItemTypeMark> marks, 
            bool isEnable)
            : this(title, marks, isEnable, true, true)
        {
            
        }

        public CategoryFilterViewModel(string title, 
            IEnumerable<ItemTypeMark> marks, 
            bool isEnable,
            bool canDisable,
            bool shouldRestrictNotSelected)
        {
            Title = title;
            CanDisable = canDisable;
            _isEnable = isEnable;
            _shouldRestrictNotSelected = shouldRestrictNotSelected;
            Marks = new ObservableCollection<ItemTypeMark>(marks);

            Marks.CollectionChanged += NotifyChanges;

            NotifyChanges(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Marks));
        }

        public bool IsEnable
        {
            get => _isEnable;
            set
            {
                if (SetProperty(ref _isEnable, value))
                {
                    RaiseFitlerChanged();
                }
            }
        }

        public ObservableCollection<ItemTypeMark> Marks { get; }

        public string Title { get; }
        
        [JsonIgnore]
        public bool CanDisable { get; } = true;

        public event EventHandler FilterChanged;

        /// <summary>
        /// Возвращаю список текстовых значений выбранных элементов
        /// </summary>
        /// <returns></returns>
        public string[] GetSelected()
        {
            return Marks
                   .Where(x => x.IsChecked)
                   .Select(x => x.Value)
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
            if (_shouldRestrictNotSelected
                && sender is ItemTypeMark mark
                && Marks.All(x => !x.IsChecked))
            {
                mark.IsChecked = true;
            }

            RaiseFitlerChanged();
        }

        protected void RaiseFitlerChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
