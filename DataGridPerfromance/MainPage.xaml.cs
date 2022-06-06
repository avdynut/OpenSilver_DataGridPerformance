using DataGridPerfromance.OpenSilver;
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
        private ICollectionView _collectionView;
        private ICollectionView _collectionView2;

        public MainPage()
        {
            InitializeComponent();

            _collectionView = new CollectionViewSource { Source = _items }.View;
            _collectionView.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            MyDataGrid.ItemsSource = _collectionView;

            _collectionView2 = new CollectionViewSource
            {
                Source = new ListCollectionViewWrapperFactory<Item>(_items2, new ItemSort())
            }.View;
            _collectionView2.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            CollectionViewDataGrid.ItemsSource = _collectionView2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CountTextBox.Text, out int count))
            {
                _items.Clear();
                for (int i = 0; i < count; i++)
                {
                    _items.Add(new Item { Number = i, Name = $"Name {i}" });
                }
                _collectionView.Refresh();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CountTextBox.Text, out int count))
            {
                _items2.Clear();
                for (int i = 0; i < count; i++)
                {
                    _items2.Add(new Item { Number = i, Name = $"Name {i}" });
                }
                _collectionView2.Refresh();
            }
        }
    }

    public class ItemSort : CustomSort<Item>
    {
        protected override int CompareProperty(Item x, Item y, string propertyName)
        {
            switch (propertyName)
            {
                case "Number":
                    return x.Number.CompareTo(y.Number);
                case "Name":
                    return x.Name.CompareTo(y.Name);
                default:
                    return 0;
            }
        }
    }
}