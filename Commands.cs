using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File_Manager_WPF
{
    class Commands
    {
        public static readonly RoutedUICommand CopyOrMove = new RoutedUICommand("CopyOrMove", "CopyOrMove", typeof(Commands));
        public static readonly RoutedUICommand Delete = new RoutedUICommand("Delete", "Delete", typeof(Commands));
        public static readonly RoutedUICommand Details = new RoutedUICommand("Details", "Details", typeof(Commands));
        public static readonly RoutedUICommand Back = new RoutedUICommand("Back", "Back", typeof(Commands));
        public static readonly RoutedUICommand CreateFileOrDirectory = new RoutedUICommand("CreateFileOrDirectory", "CreateFileOrDirectory", typeof(Commands));
    }
}
