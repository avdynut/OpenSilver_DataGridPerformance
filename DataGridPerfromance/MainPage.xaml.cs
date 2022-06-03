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
        private ICollectionView _collectionView;

        public MainPage()
        {
            InitializeComponent();

            _collectionView = new CollectionViewSource
            {
                Source = new ListCollectionViewWrapperFactory<Item>(new EntitySet<Item>(_items), new ItemSortFactory())
            }.View;
            _collectionView.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Descending));
            MyDataGrid.ItemsSource = _collectionView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _items.Clear();
            for (int i = 0; i < 10; i++)
            {
                _items.Add(new Item { Number = i, Name = $"Name {i}" });
            }
            _collectionView.Refresh();
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

    public class ItemSortFactory : CustomSortFactory<Item>
    {
        public override CustomSort<Item> CreateCustomSort()
        {
            return new ItemSort();
        }
    }
}