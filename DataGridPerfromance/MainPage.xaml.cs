using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataGridPerfromance
{
    public partial class MainPage : UserControl
    {
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        private ObservableCollection<Item> _items2 = new ObservableCollection<Item>();
        private CollectionViewSource _collectionViewSource;
        private ICollectionView _collectionView => _collectionViewSource.View;

        public MainPage()
        {
            InitializeComponent();

            MyDataGrid.ItemsSource = _items;
            _collectionViewSource = new CollectionViewSource { Source = _items2 };
            // _collectionView.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Ascending));
            CollectionViewDataGrid.ItemsSource = _collectionView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshCollection(_items, ListTime);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RefreshCollection(_items2, CollectionViewTime);

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
