using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace DataGridPerfromance.OpenSilver
{
    public class ListCollectionViewWrapperFactory<T> : ICollectionViewFactory, IEnumerable<T>
    {
        private readonly CustomSortFactory<T> _customSortFactory;
        private readonly EntitySet<T> _set;

        public ListCollectionViewWrapperFactory(EntitySet<T> set, CustomSortFactory<T> customSortFactory)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            _set = set;
            _customSortFactory = customSortFactory;
        }

        public ICollectionView CreateView()
        {
            return new ListCollectionViewWrapper(_set, _customSortFactory);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        private class ListCollectionViewWrapper : ICollectionView
        {
            private static readonly Type _listCollectionViewType;
            private static readonly PropertyInfo _customSortProperty;
            private static readonly MethodInfo _refreshOrDeferMethod;

            private readonly ICollectionView _baseView;
            private readonly CustomSort<T> _customSort;
            private SortDescriptionCollection _sort;

            static ListCollectionViewWrapper()
            {
                _listCollectionViewType = new CollectionViewSource { Source = new List<int>() }.View.GetType();
                _customSortProperty = _listCollectionViewType.GetProperty("CustomSort");
                _refreshOrDeferMethod = _listCollectionViewType.GetMethod("RefreshOrDefer", BindingFlags.Instance | BindingFlags.NonPublic);

                if (_customSortProperty == null)
                {
                    throw new InvalidOperationException("_customSortProperty is null !");
                }

                if (_refreshOrDeferMethod == null)
                {
                    throw new InvalidOperationException("_refreshOrDeferMethod is null !");
                }
            }

            public ListCollectionViewWrapper(EntitySet<T> set, CustomSortFactory<T> customSortFactory)
            {
                Debug.Assert(set != null);

                ICollectionView view = set.CreateView();

                Debug.Assert(view != null);
                Debug.Assert(_listCollectionViewType.IsAssignableFrom(view.GetType()));

                _baseView = view;
                _customSort = customSortFactory?.CreateCustomSort();

                if (_customSort != null)
                {
                    _customSortProperty.SetValue(_baseView, _customSort);
                    _customSort.SetCollectionView(this);
                }
            }

            private void SetSortDescriptions(SortDescriptionCollection descriptions)
            {
                if (_sort != null)
                {
                    ((INotifyCollectionChanged)_sort).CollectionChanged -= new NotifyCollectionChangedEventHandler(SortDescriptionsChanged);
                }

                _sort = descriptions;

                if (_sort != null)
                {
                    Debug.Assert(_sort.Count == 0, "must be empty SortDescription collection");
                    ((INotifyCollectionChanged)_sort).CollectionChanged += new NotifyCollectionChangedEventHandler(SortDescriptionsChanged);
                }
            }

            private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                IEditableCollectionView view = _baseView as IEditableCollectionView;
                if (view != null)
                {
                    if (view.IsAddingNew || view.IsEditingItem)
                        throw new InvalidOperationException(string.Format("'{0}' is not allowed during an AddNew or EditItem transaction.", "Sorting"));
                }

                _refreshOrDeferMethod.Invoke(_baseView, Array.Empty<object>());
            }

            public CultureInfo Culture { get => _baseView.Culture; set => _baseView.Culture = value; }

            public IEnumerable SourceCollection => _baseView.SourceCollection;

            public Predicate<object> Filter { get => _baseView.Filter; set => _baseView.Filter = value; }

            public bool CanFilter => _baseView.CanFilter;

            public SortDescriptionCollection SortDescriptions
            {
                get
                {
                    if (_customSort == null)
                        return _baseView.SortDescriptions;

                    if (_sort == null)
                        SetSortDescriptions(new SortDescriptionCollection());
                    return _sort;
                }
            }

            public bool CanSort => _baseView.CanSort;

            public bool CanGroup => _baseView.CanGroup;

            public ObservableCollection<GroupDescription> GroupDescriptions => _baseView.GroupDescriptions;

            public ReadOnlyObservableCollection<object> Groups => _baseView.Groups;

            public bool IsEmpty => _baseView.IsEmpty;

            public object CurrentItem => _baseView.CurrentItem;

            public int CurrentPosition => _baseView.CurrentPosition;

            public bool IsCurrentAfterLast => _baseView.IsCurrentAfterLast;

            public bool IsCurrentBeforeFirst => _baseView.IsCurrentBeforeFirst;

            public event CurrentChangingEventHandler CurrentChanging
            {
                add
                {
                    _baseView.CurrentChanging += value;
                }

                remove
                {
                    _baseView.CurrentChanging -= value;
                }
            }

            public event EventHandler CurrentChanged
            {
                add
                {
                    _baseView.CurrentChanged += value;
                }

                remove
                {
                    _baseView.CurrentChanged -= value;
                }
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add
                {
                    _baseView.CollectionChanged += value;
                }

                remove
                {
                    _baseView.CollectionChanged -= value;
                }
            }

            public bool Contains(object item)
            {
                return _baseView.Contains(item);
            }

            public IDisposable DeferRefresh()
            {
                return _baseView.DeferRefresh();
            }

            public IEnumerator GetEnumerator()
            {
                return _baseView.GetEnumerator();
            }

            public bool MoveCurrentTo(object item)
            {
                return _baseView.MoveCurrentTo(item);
            }

            public bool MoveCurrentToFirst()
            {
                return _baseView.MoveCurrentToFirst();
            }

            public bool MoveCurrentToLast()
            {
                return _baseView.MoveCurrentToLast();
            }

            public bool MoveCurrentToNext()
            {
                return _baseView.MoveCurrentToNext();
            }

            public bool MoveCurrentToPosition(int position)
            {
                return _baseView.MoveCurrentToPosition(position);
            }

            public bool MoveCurrentToPrevious()
            {
                return _baseView.MoveCurrentToPrevious();
            }

            public void Refresh()
            {
                _baseView.Refresh();
            }
        }
    }

    public abstract class CustomSortFactory<T>
    {
        public abstract CustomSort<T> CreateCustomSort();
    }

    public abstract class CustomSort<T> : IComparer
    {
        private readonly IComparer _comparer;
        private ICollectionView _view;

        public CustomSort()
        {
            _comparer = Comparer<T>.Create((x, y) =>
            {
                var result = 0;
                if (_view == null || _view.SortDescriptions.Count == 0)
                {
                    return result;
                }

                foreach (var sortDescription in _view.SortDescriptions)
                {
                    result = CompareProperty(x, y, sortDescription.PropertyName);

                    if (result != 0 && sortDescription.Direction == ListSortDirection.Descending)
                    {
                        result = -result;
                    }

                    if (result != 0)
                    {
                        break;
                    }
                }

                return result;
            });
        }

        public void SetCollectionView(ICollectionView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Cannot set view multiple times.");
            }

            _view = view;
        }

        public int Compare(object x, object y) => _comparer.Compare(x, y);

        protected abstract int CompareProperty(T x, T y, string propertyName);
    }

    public sealed class EntitySet<TEntity> : ICollectionViewFactory, IEnumerable<TEntity>
    {
        private readonly IEnumerable<TEntity> items;

        public EntitySet(IEnumerable<TEntity> items)
        {
            this.items = items;
        }

        public ICollectionView CreateView()
        {
            return new CollectionViewSource { Source = items }.View;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerable.Empty<TEntity>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}