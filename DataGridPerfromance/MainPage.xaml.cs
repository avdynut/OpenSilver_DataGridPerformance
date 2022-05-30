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

            _collectionView = new CollectionViewSource { Source = _items }.View;
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
            //_collectionView.Refresh();
        }
    }
}
