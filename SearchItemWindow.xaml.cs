using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace File_Manager_WPF
{
    /// <summary>
    /// Interaction logic for SearchItemWindow.xaml
    /// </summary>
    public partial class SearchItemWindow : Window
    {
        public SearchItemWindow()
        {
            InitializeComponent();
        }

        private void EscapeButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    ShowDetails();
                    break;
                case Key.Escape:
                    Close();
                    break;
            }
        }
        private void ShowDetails()
        {
            ItemDetails newDetailsWindow = new ItemDetails();
            Item selectedItem = new Item(); ;
            selectedItem = SearchResultListBox.SelectedItem as Item;
            newDetailsWindow.SetItem(selectedItem.Path);
            newDetailsWindow.Show();
        }
        private void SearchResultListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var currentLV = SearchResultListBox;
                Item selectedItem = ((Item)currentLV.SelectedItem);
                try
                { 
                    Process.Start(selectedItem.Path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        //Can Execute
        private void IsItemSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            Item SelectedItem = SearchResultListBox.SelectedItem as Item;
            if (SelectedItem != null)
            {
                e.CanExecute = true;
            }
        }
        private void Details_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowDetails();
        }
    }
}
