using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
#if OPENSILVER
using System.Diagnostics;
#endif

namespace DataGridPerfromance
{
    public partial class MainPage : UserControl
    {
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        private ObservableCollection<Item> _items2 = new ObservableCollection<Item>();
        private ObservableCollection<Item> _items3 = new ObservableCollection<Item>();
        private ICollectionView _collectionView;
        private ICollectionView _collectionView2;

        public MainPage()
        {
            InitializeComponent();

            MyDataGrid.ItemsSource = _items;

            _collectionView = new CollectionViewSource { Source = _items2 }.View;
            // _collectionView.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));

            _collectionView2 = new CollectionViewSource { Source = new Collection<Item>(_items3) }.View;
            CollectionViewListDataGrid.ItemsSource = _collectionView2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshCollection(_items, ListTime);
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
}
