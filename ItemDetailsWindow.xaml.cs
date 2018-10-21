using System;
using System.Collections.Generic;
using System.IO;
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;

namespace File_Manager_WPF
{
    /// <summary>
    /// Interaction logic for ItemDetails.xaml
    /// </summary>
    public partial class ItemDetails : Window
    {
        private string itemPath;
        private FileInfo item;

        public ItemDetails()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            item = new FileInfo(itemPath);
            FileName.Content = item.Name;
            FilePath.Content = item.FullName;

            FileAttributes attr = File.GetAttributes(itemPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                FileIcon.Source = new BitmapImage(new Uri("Folder.png", UriKind.Relative));
                FileType.Content = "Folder";
                FileSize.Content = Operations.SizeConverter(Operations.GetDirectorySize(itemPath));
            }
            else
            {
                Icon fileIcon = System.Drawing.Icon.ExtractAssociatedIcon(itemPath);
                FileIcon.Source = ToImageSource(fileIcon);
                FileType.Content = item.Extension;
                FileCreationTime.Content = item.CreationTime;
                FileSize.Content = Operations.SizeConverter(item.Length);
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void SetItem(string argItem)
        {
            itemPath = argItem;
        }
        private static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

    }
}
