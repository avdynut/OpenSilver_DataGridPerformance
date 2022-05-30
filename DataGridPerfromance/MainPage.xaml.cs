using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Collections;
#if OPENSILVER
using System.Diagnostics;
#endif

namespace DataGridPerfromance
{
    public partial class MainPage : UserControl
    {
        private List<Item> _items = new List<Item>();
        private ObservableCollection<Item> _items2 = new ObservableCollection<Item>();
        private ObservableCollection<Item> _items3 = new ObservableCollection<Item>();
        private ICollectionView _collectionView;
        private ICollectionView _collectionView2;

        public MainPage()
        {
            InitializeComponent();

            

            _collectionView = new CollectionViewSource { Source = _items2 }.View;
            // _collectionView.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));

            _collectionView2 = new CollectionViewSource { Source = new CollectionWrapper<Item>(_items3) }.View;
            CollectionViewListDataGrid.ItemsSource = _collectionView2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshCollection(_items, ListTime);
            MyDataGrid.ItemsSource = new CollectionViewSource { Source = _items }.View;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CollectionViewDataGrid.ItemsSource = null;
            RefreshCollection(_items2, CollectionViewTime, () => CollectionViewDataGrid.ItemsSource = _collectionView);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RefreshCollection(_items3, CollectionViewListTime, _collectionView2.Refresh);
        }

        private void RefreshCollection(IList<Item> list, TextBlock textBlock, Action action = null)
        {
            textBlock.Text = " ";
            list.Clear();

            int count;
            if (int.TryParse(CountTextBox.Text, out count))
            {
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    list.Add(new Item { Number = i, Name = "Name " + i });
                }
                action?.Invoke();
                stopwatch.Stop();
                textBlock.Text = $"{stopwatch.ElapsedMilliseconds} ms";
            }
        }
    }

    public class CollectionWrapper<T> : ICollection<T>
    {
        private readonly ICollection<T> _items;

        public CollectionWrapper(ICollection<T> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public int Count => _items.Count;
        public bool IsReadOnly => _items.IsReadOnly;
        public void Add(T item) => _items.Add(item);
        public bool Remove(T item) => _items.Remove(item);
        public bool Contains(T item) => _items.Contains(item);
        public void Clear() => _items.Clear();
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}
