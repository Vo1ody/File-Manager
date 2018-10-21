using System;
using System.Collections.Generic;
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
    /// Interaction logic for NewItem.xaml
    /// </summary>
    public partial class NewItem : Window
    {
        private string[] ext = {".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls", ".zip", ".rar", ".txt"};
        private string itemType;

        public NewItem()
        {
            InitializeComponent();
        }

        #region events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (itemType == "Rename")
            {
                OkButton.Content = "Сохранить";
                Title = "Изменить имя ";
                NewItemMessageLabel.Content = "Введите новое имя ";
                ItemNameTB.Focus();
                ItemNameTB.SelectAll();
            }
            else
            {
                Title = "Новый " + itemType;
                NewItemMessageLabel.Content = "Имя нового " + itemType + "а";
                IMessageLabel.Content = "(введите имя с расширением или выберите из списка)";
                foreach (string s in ext)
                {
                    ItemListTB.Items.Add(s);
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    DialogResult = true;
                    Close();
                    break;
                case Key.Escape:
                    DialogResult = false;
                    Close();
                    break;
            }
        }
        private void OkClose(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        #endregion

        #region methods
        public void SetItemType(string argItemType)
        {
            itemType = argItemType;
        }
        public void SetValue(string value)
        {
            ItemNameTB.Text = value;
        }
        #endregion

    }
}
