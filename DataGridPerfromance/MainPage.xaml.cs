using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#if OPENSILVER
using System.Diagnostics;
#endif

namespace DataGridPerfromance
{
    public partial class MainPage : UserControl
    {
        private List<Item> _items = new List<Item>();
        private List<Item> _items2 = new List<Item>();
        private CollectionViewSource _collectionViewSource;
        private ICollectionView _collectionView => _collectionViewSource.View;

        public MainPage()
        {
            InitializeComponent();
            
            _collectionViewSource = new CollectionViewSource { Source = _items2 };
            // _collectionView.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Ascending));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyDataGrid.ItemsSource = null;
            RefreshCollection(_items, ListTime);
            MyDataGrid.ItemsSource = _items;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CollectionViewDataGrid.ItemsSource = null;
            RefreshCollection(_items2, CollectionViewTime);
            CollectionViewDataGrid.ItemsSource = _collectionView;
            //_collectionView.Refresh();
        }

        private void RefreshCollection(IList<Item> list, TextBlock textBlock)
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
                stopwatch.Stop();
                textBlock.Text = $"{stopwatch.ElapsedMilliseconds} ms";
            }
        }
    }
}
