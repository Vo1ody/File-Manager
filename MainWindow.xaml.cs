using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;

namespace File_Manager_WPF
{
    public delegate void FtpDelegate(Client cl);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields
        private string currentPathLeft, currentPathRight;
        private int holdLeftIndex, holdRightIndex, activePanel;
        private bool isLeftSelected = false, isRightSelected = false, cbActive = true, isFTP = false;
        private List<Drive> drivesList;
        private List<string> leftHistory, rightHistory;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static HookProc proc;
        private static IntPtr hook = IntPtr.Zero;

        private Client client = null;
        #endregion

        #region Hook
        private static IntPtr SetHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Key)vkCode == Key.O)
                {
                    Activate();
                    FilesSearchTB.Focus();
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            leftHistory = new List<string>();
            rightHistory = new List<string>();
            //proc = HookCallback;
            hook = SetHook(proc);
            UnhookWindowsHookEx(hook);
        }

        #region Events
        // Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drivesList = new List<Drive>();
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                Drive newDrive = new Drive();
                try
                {
                    newDrive.Label = drive.VolumeLabel;
                }
                catch
                {
                    newDrive.Label = drive.DriveType.ToString();
                }
                newDrive.Name = drive.Name;
                drivesList.Add(newDrive);

            }
            leftDrive.ItemsSource = drivesList;
            leftDrive.SelectedIndex = 0;
            rightDrive.ItemsSource = drivesList;
            rightDrive.SelectedIndex = 0;

            currentPathLeft = ((Drive)leftDrive.SelectedItem).Name;
            currentPathRight = ((Drive)rightDrive.SelectedItem).Name;
            activePanel = 0;
            
            leftDirectoryPathTextBox.Visibility = Visibility.Hidden;
            leftDirectoryPathTextBox.Padding = new Thickness(4);
            rightDirectoryPathTextBox.Visibility = Visibility.Hidden;
            rightDirectoryPathTextBox.Padding = new Thickness(4);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    EnterKeyHanlder();
                    break;
                case Key.Back:
                    ChangeDirectory("..");
                    break;
                case Key.Left:
                    if (leftDirectory.SelectedItem != null)
                    {
                        ListViewItem item = leftDirectory.ItemContainerGenerator.ContainerFromIndex(leftDirectory.SelectedIndex) as ListViewItem;
                        item.Focus();
                    }
                    else
                    {
                        ListViewItem item = leftDirectory.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                        item.Focus();
                    }
                    break;
                case Key.Right:
                    if (rightDirectory.SelectedItem != null)
                    {
                        ListViewItem item = rightDirectory.ItemContainerGenerator.ContainerFromIndex(rightDirectory.SelectedIndex) as ListViewItem;
                        item.Focus();
                    }
                    else
                    {
                        ListViewItem item = rightDirectory.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                        item.Focus();
                    }
                    break;
                case Key.F1:
                    ShowHelp();
                    break;
                case Key.F2:
                    CopyItem();
                    break;
                case Key.F3:
                    MoveItem();
                    break;
                case Key.F4:
                    DeleteItem();
                    break;
                case Key.F5:
                    RefreshDirectiories();
                    break;
                case Key.F6:
                    RenameItem();
                    break;
                case Key.F7:
                    CreateFileOrDirectory("Directory");
                    break;
                case Key.F8:
                    CreateFileOrDirectory("File");
                    break;
                case Key.F9:
                    if(isFTP) return;
                    ShowDetails();
                    break;
                case Key.Escape:
                    EscapeKeyHanlder();
                    break;
                case Key.Q:
                    string panel = "Активна: ";
                    if (leftDirectory.SelectedItem != null)
                    {
                        panel += "[L]";
                    }
                    if (rightDirectory.SelectedItem != null)
                    {
                        panel += "[P]";
                    }
                    if (activePanel == -1)
                    {
                        panel += "ЛЕВАЯ";
                    }
                    if (activePanel == 1)
                    {
                        panel += "ПРАВАЯ";
                    }
                    MessageBox.Show(panel);
                    break;

            }
        }

        // ComboBox
        private void leftDriveSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbActive)
                {
                    var currentCB = sender as ComboBox;
                    string newPath = ((Drive)currentCB.SelectedItem).Name;
                    if (currentPathLeft != null)
                    {
                        leftHistory.Add(currentPathLeft);
                    }

                    List<Item> tmpLV = Operations.LoadDirectory(currentPathLeft, newPath);
                    if (tmpLV != null)
                    {
                        leftDirectory.ItemsSource = tmpLV;
                        leftDirectory.Items.Refresh();
                        currentPathLeft = newPath;

                    }
                    leftDirectoryPathLabel.Content = currentPathLeft;
                    leftDirectoryPathTextBox.Text = currentPathLeft;
                    holdLeftIndex = currentCB.SelectedIndex;
                    DeactivatePanel();
                }
                cbActive = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                leftDrive.SelectedIndex = holdLeftIndex;
            }
        }
        private void rightDriveSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbActive)
                {
                    var currentCB = sender as ComboBox;
                    string newPath = ((Drive)currentCB.SelectedItem).Name;
                    if (currentPathRight != null)
                    {
                        rightHistory.Add(currentPathRight);
                    }

                    List<Item> tmpLV;
                    if (isFTP) tmpLV = client.DirectoryListDetailed(newPath);
                    else tmpLV = Operations.LoadDirectory(currentPathRight, newPath);
                    if (tmpLV != null)
                    {
                        rightDirectory.ItemsSource = tmpLV;
                        rightDirectory.Items.Refresh();
                        currentPathRight = newPath;

                    }
                    rightDirectoryPathLabel.Content = currentPathRight;
                    rightDirectoryPathTextBox.Text = currentPathRight;
                    holdRightIndex = currentCB.SelectedIndex;
                    DeactivatePanel();
                }
                cbActive = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                rightDrive.SelectedIndex = holdRightIndex;
            }
        }

        // ListView DoubleClick
        private void leftDirectory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ChangeDirectory();
            }
        }
        private void rightDirectory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ChangeDirectory();
            }
        }

        // Label Context Menu
        private void EditLeftPathContextMenuClick(object sender, RoutedEventArgs e)
        {
            leftDirectoryPathLabel.Visibility = Visibility.Hidden;
            leftDirectoryPathTextBox.Visibility = Visibility.Visible;
            leftDirectoryPathTextBox.Text = leftDirectoryPathLabel.Content.ToString();
            leftDirectoryPathTextBox.Focus();
            leftDirectoryPathTextBox.SelectAll();
        }
        private void EditRightPathContextMenuClick(object sender, RoutedEventArgs e)
        {
            rightDirectoryPathLabel.Visibility = Visibility.Hidden;
            rightDirectoryPathTextBox.Visibility = Visibility.Visible;
            rightDirectoryPathTextBox.Text = rightDirectoryPathLabel.Content.ToString();
            rightDirectoryPathTextBox.Focus();
            rightDirectoryPathTextBox.SelectAll();
        }

        // Focus
        private void rightDirectoryPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            rightDirectoryPathTextBox.Text = currentPathRight;
            rightDirectoryPathLabel.Visibility = Visibility.Visible;
            rightDirectoryPathTextBox.Visibility = Visibility.Hidden;
        }
        private void leftDirectoryPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            leftDirectoryPathTextBox.Text = currentPathLeft;
            leftDirectoryPathLabel.Visibility = Visibility.Visible;
            leftDirectoryPathTextBox.Visibility = Visibility.Hidden;
        }
        private void leftDirectory_GotFocus(object sender, RoutedEventArgs e)
        {
            activePanel = -1;
            isLeftSelected = true;
            //leftDirectory.SetValue(BorderBrushProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom("#3361e0")));
            //rightDirectory.SetValue(BorderBrushProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom("#727272")));
        }
        private void rightDirectory_GotFocus(object sender, RoutedEventArgs e)
        {
            activePanel = 1;
            isRightSelected = true;
            //rightDirectory.SetValue(BorderBrushProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom("#3361e0")));
            //leftDirectory.SetValue(BorderBrushProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom("#727272")));
        }

        //Buttons
        private void RefreshButton(object sender, RoutedEventArgs e)
        {
            RefreshDirectiories();
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            string tmp = FilesSearchTB.Text;
            string pattern = "[\\/:*?<>|+.%!@\"]";
            Regex rgx = new Regex(pattern);
            if (tmp == "") {MessageBox.Show("Строка поиска не может быть пустой", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); return;} 
            if (rgx.IsMatch(tmp)) { MessageBox.Show("Строка поиска содержит недопустимые символы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            FilesSearchTB.Clear();
            FileSearchLabel.Content = "Поиск...";
            Thread th = new Thread(new ParameterizedThreadStart(SearchFileOrDirectories));
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start(tmp);
        }
        private void EscapeButton(object sender, RoutedEventArgs e)
        {
            CloseProgram();
        }
        private void HelpButton(object sender, RoutedEventArgs e)
        {
            ShowHelp();
        }
        private void FTPConnectButton(object sender, RoutedEventArgs e)
        {
            isFTP = true;
            FTPCon.IsEnabled = false;
            FTPDiscon.IsEnabled = true;
            rightDrive.IsEnabled = rightDirectoryPathTextBox.IsEnabled = false;
            FtpClientWindow fcw = new FtpClientWindow(new FtpDelegate(FtpClientHandler));
            fcw.Show();
        }
        private void FTPDisconnectButton(object sender, RoutedEventArgs e)
        {
            isFTP = false;
            FTPCon.IsEnabled = true;
            FTPDiscon.IsEnabled = false;
            rightDrive.IsEnabled = rightDirectoryPathTextBox.IsEnabled = true;
            currentPathRight = ((Drive)rightDrive.SelectedItem).Name;
            List<Item> tmpLV = Operations.LoadDirectory("", currentPathRight);
            if (tmpLV != null)
            {
                rightDirectory.ItemsSource = tmpLV;
                rightDirectory.Items.Refresh();

            }
            rightDirectoryPathLabel.Content = currentPathRight;
            rightDirectoryPathTextBox.Text = currentPathRight;
            DeactivatePanel();
        }
        #endregion

        #region Commands
        // Executed
        private void CreateFileOrDirectory_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CreateFileOrDirectory(e.Parameter.ToString());
        }
        private void CopyOrMove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == "Copy")
            {
                CopyItem();
            }
            else if (e.Parameter.ToString() == "Move")
            {
                MoveItem();
            }
            else if (e.Parameter.ToString() == "Rename")
            {
                RenameItem();
            }
        }
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteItem();
        }
        private void Details_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(isFTP) return;
            ShowDetails();
        }
        private void Back_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == "Left")
            {
                //string newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathLeft, ".."));
                string newPath = leftHistory[leftHistory.Count - 1];
                leftDirectory.ItemsSource = Operations.LoadDirectory(currentPathLeft, newPath);
                leftDirectory.Items.Refresh();
                currentPathLeft = newPath;
                leftDirectoryPathLabel.Content = newPath;
                leftHistory.RemoveAt(leftHistory.Count - 1);

                if (System.IO.Path.GetPathRoot(currentPathLeft) != ((Drive)leftDrive.SelectedItem).Name)
                {
                    int i = 0;
                    foreach (Drive drive in leftDrive.Items)
                    {
                        if (drive.Name == System.IO.Path.GetPathRoot(currentPathLeft))
                        {
                            cbActive = false;
                            leftDrive.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                DeactivatePanel();
            }
            else if (e.Parameter.ToString() == "Right")
            {
                //string newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathLeft, ".."));
                string newPath = rightHistory[rightHistory.Count - 1];
                if (isFTP) rightDirectory.ItemsSource = client.DirectoryListDetailed(newPath);
                else rightDirectory.ItemsSource = Operations.LoadDirectory(currentPathRight, newPath);
                rightDirectory.Items.Refresh();
                currentPathRight = newPath;
                rightDirectoryPathLabel.Content = newPath;
                rightHistory.RemoveAt(rightHistory.Count - 1);
                if(isFTP) return;
                else if (System.IO.Path.GetPathRoot(currentPathRight) != ((Drive)rightDrive.SelectedItem).Name)
                {
                    int i = 0;
                    foreach (Drive drive in rightDrive.Items)
                    {
                        if (drive.Name == System.IO.Path.GetPathRoot(currentPathRight))
                        {
                            cbActive = false;
                            rightDrive.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                DeactivatePanel();
            }
        }

        //CanExecute
        private void IsItemSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            Item leftSelectedItem = leftDirectory.SelectedItem as Item;
            Item rightSelectedItem = rightDirectory.SelectedItem as Item;
            if (leftSelectedItem != null)
            {
                if (activePanel == -1 && leftSelectedItem.Name != "..")
                {
                    e.CanExecute = true;
                }
            }
            if (rightSelectedItem != null)
            {
                if (activePanel == 1 && rightSelectedItem.Name != "..")
                {
                    e.CanExecute = true;
                }
            }
        }
        private void IsDirectorySelected(object sender, CanExecuteRoutedEventArgs e)
        {
            Item leftSelectedItem = leftDirectory.SelectedItem as Item;
            Item rightSelectedItem = rightDirectory.SelectedItem as Item;
            if (leftSelectedItem != null)
            {
                if (activePanel == -1)
                {
                    e.CanExecute = true;
                }
            }
            if (rightSelectedItem != null)
            {
                if (activePanel == 1)
                {
                    e.CanExecute = true;
                }
            }
        }
        private void IsHistoryNotEmpty(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.ToString() == "Left")
                {
                    if (leftHistory.Count > 0)
                    {
                        e.CanExecute = true;
                    }
                }
                else if (e.Parameter.ToString() == "Right")
                {
                    if (rightHistory.Count > 0)
                    {
                        e.CanExecute = true;
                    }
                }
            }
        }
        #endregion

        #region Methods

        void FtpClientHandler(Client cl)
        {
            client = cl;
            currentPathRight = "";
            List<Item> list = client.DirectoryListDetailed(currentPathRight);
            // Отобразить список в ListView
            rightDirectory.ItemsSource = list;
        }
        private void ChangeDirectory()
        {
            try
            {
                if (activePanel == -1)
                {
                    var currentLV = leftDirectory;
                    Item selectedItem = ((Item)currentLV.SelectedItem);
                    if (selectedItem.IsDirectory())
                    {
                        leftHistory.Add(currentPathLeft);
                        string newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathLeft, selectedItem.Name));

                        List<Item> tmpLV = Operations.LoadDirectory(currentPathLeft, newPath);
                        if (tmpLV != null)
                        {
                            leftDirectory.ItemsSource = tmpLV;
                            leftDirectory.Items.Refresh();
                            currentPathLeft = newPath;

                        }
                        leftDirectoryPathLabel.Content = currentPathLeft;
                        leftDirectoryPathTextBox.Text = currentPathLeft;
                        leftDirectory.SelectedIndex = 0;
                        //DeactivatePanel();
                    }
                    else
                    {
                        Process.Start(selectedItem.Path);
                    }
                }
                else if (activePanel == 1)
                {
                    var currentLV = rightDirectory;
                    Item selectedItem = ((Item)currentLV.SelectedItem);
                    if (selectedItem.IsDirectory())
                    {
                        string newPath = null;
                        List<Item> tmpLV;
                        rightHistory.Add(currentPathRight);
                        if (isFTP)
                        {
                            newPath = currentPathRight + selectedItem.Name + "/";
                            tmpLV = client.DirectoryListDetailed(newPath);
                        }
                        else
                        {
                            newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathRight, selectedItem.Name));
                            tmpLV = Operations.LoadDirectory(currentPathRight, newPath);
                        }
                        if (tmpLV != null)
                        {
                            rightDirectory.ItemsSource = tmpLV;
                            rightDirectory.Items.Refresh();
                            currentPathRight = newPath;
                        }

                        rightDirectoryPathLabel.Content = currentPathRight;
                        rightDirectoryPathTextBox.Text = currentPathRight;
                        rightDirectory.SelectedIndex = 0;
                        //DeactivatePanel();
                    }
                    else
                    {
                        Process.Start(selectedItem.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ChangeDirectory(string targetPath)
        {
            try
            {
                if (activePanel == -1)
                {
                    leftHistory.Add(currentPathLeft);
                    var currentLV = leftDirectory;
                    Item selectedItem = ((Item)currentLV.SelectedItem);
                    string newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathLeft, targetPath));

                    List<Item> tmpLV = Operations.LoadDirectory(currentPathLeft, newPath);
                    if (tmpLV != null)
                    {
                        leftDirectory.ItemsSource = tmpLV;
                        leftDirectory.Items.Refresh();
                        currentPathLeft = newPath;

                    }
                    leftDirectoryPathLabel.Content = currentPathLeft;
                    leftDirectoryPathTextBox.Text = currentPathLeft;
                    leftDirectory.SelectedIndex = 0;
                    //DeactivatePanel();
                }
                else if (activePanel == 1)
                {
                    rightHistory.Add(currentPathRight);
                    var currentLV = rightDirectory;
                    Item selectedItem = ((Item)currentLV.SelectedItem);
                    string newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPathRight, targetPath));
                    List<Item> tmpLV;
                    if (isFTP) tmpLV = client.DirectoryListDetailed(newPath);
                    else tmpLV = Operations.LoadDirectory(currentPathRight, newPath);
                    if (tmpLV != null)
                    {
                        rightDirectory.ItemsSource = tmpLV;
                        rightDirectory.Items.Refresh();
                        currentPathRight = newPath;

                    }
                    rightDirectoryPathLabel.Content = currentPathRight;
                    rightDirectoryPathTextBox.Text = currentPathRight;
                    rightDirectory.SelectedIndex = 0;
                    //DeactivatePanel();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchFileOrDirectories(object name)
        {
            string tmp = (string) name;
            List<Item> newDirectory = new List<Item>();
            List<string> files = new List<string>();
            GetDirectories("D:\\", tmp, ref files);
            GetDirectories("C:\\", tmp, ref files);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                newDirectory.Add(new Item(System.IO.Path.GetFileNameWithoutExtension(file), fileInfo.Extension, Operations.SizeConverter(fileInfo.Length), file));
            }

            if (newDirectory.Count == 0)
            {
                Dispatcher.Invoke(() => FileSearchLabel.Content = "");
                MessageBox.Show("Файл не найден", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                Dispatcher.Invoke(() => FileSearchLabel.Content = "");
                SearchItemWindow siw;
                Dispatcher.Invoke(() =>
                {
                    siw = new SearchItemWindow();
                    siw.SearchResultListBox.ItemsSource = newDirectory;
                    siw.Show();
                });
            }
        }
        
        private void GetDirectories(string path, string name, ref List<string> files)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(path))
                {
                    foreach (string f in Directory.GetFiles(d, name + '*'))
                    {
                        files.Add(f);
                    }
                    GetDirectories(d, name, ref files);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
        private void RefreshDirectiories()
        {
            try
            {
                if (isFTP)
                {
                    leftDirectory.ItemsSource = Operations.LoadDirectory(currentPathLeft, currentPathLeft);
                    leftDirectory.Items.Refresh();
                    rightDirectory.ItemsSource = client.DirectoryListDetailed(currentPathRight);
                    rightDirectory.Items.Refresh();
                }
                else
                {
                    leftDirectory.ItemsSource = Operations.LoadDirectory(currentPathLeft, currentPathLeft);
                    leftDirectory.Items.Refresh();
                    rightDirectory.ItemsSource = Operations.LoadDirectory(currentPathRight, currentPathRight);
                    rightDirectory.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CopyItem()
        {
            try
            {
                if (activePanel == -1)
                {
                    if (isFTP)
                    {
                        List<Item> tmp = GetAllSelectedItems(leftDirectory);
                        foreach (Item iCopy in tmp)
                        {
                            if(iCopy.IsDirectory())
                                client.Upload(currentPathRight + iCopy.Name, currentPathLeft);
                            else client.Upload(currentPathRight + iCopy.Name + iCopy.Extension, currentPathLeft);
                        }
                    }
                    else Operations.Copy(GetAllSelectedItems(leftDirectory), currentPathLeft, currentPathRight);
                    RefreshDirectiories();
                }
                else if (activePanel == 1)
                {
                    if (isFTP)
                    {
                        List<Item> tmp = GetAllSelectedItems(rightDirectory);
                        foreach (Item iCopy in tmp)
                        {
                            if (iCopy.IsDirectory())
                                client.Download(currentPathRight + iCopy.Name, currentPathLeft + iCopy.Name);
                            else
                                client.Download(
                                    currentPathRight + iCopy.Name + iCopy.Extension,
                                    System.IO.Path.Combine(currentPathLeft, iCopy.Name + iCopy.Extension));
                        }
                    }
                    else Operations.Copy(GetAllSelectedItems(rightDirectory), currentPathRight, currentPathLeft);
                    RefreshDirectiories();
                }
                activePanel = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void MoveItem()
        {
            try
            {
                if (activePanel == -1)
                {
                    if (isFTP)
                    {
                        List<Item> selectedItems = GetAllSelectedItems(leftDirectory);
                        Operations.Move(selectedItems, currentPathLeft, currentPathRight, client, true);
                        if (Operations.Delete(selectedItems))
                        {
                            RefreshDirectiories();
                            activePanel = 0;
                        }
                    }
                    else Operations.Move(GetAllSelectedItems(leftDirectory), currentPathLeft, currentPathRight);
                    RefreshDirectiories();
                }
                else if (activePanel == 1)
                {
                    if (isFTP)
                    {
                        List<Item> selectedItems = GetAllSelectedItems(rightDirectory);
                        Operations.Move(selectedItems, currentPathLeft, currentPathRight, client, true, false);
                        if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            foreach (Item itemToDelete in selectedItems)
                            {
                                if (itemToDelete.IsDirectory())
                                {
                                    client.Delete(currentPathRight + itemToDelete.Name);
                                }
                                else
                                {
                                    client.Delete(currentPathRight + itemToDelete.Name + itemToDelete.Extension);
                                }
                            }
                        }

                    }
                    else Operations.Move(GetAllSelectedItems(rightDirectory), currentPathRight, currentPathLeft);
                    RefreshDirectiories();
                }
                activePanel = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RenameItem()
        {
            try
            {
                if (activePanel == -1)
                {
                    Item itemToRename = leftDirectory.SelectedItem as Item;
                    string newPath = Operations.Rename(itemToRename, currentPathLeft);
                    if (newPath != "")
                    {
                        if (leftHistory.Contains(itemToRename.Path))
                        {
                            leftHistory[leftHistory.IndexOf(itemToRename.Path)] = newPath;
                        }
                        RefreshDirectiories();
                        activePanel = 0;
                    }
                }
                else if (activePanel == 1)
                {
                    Item itemToRename = rightDirectory.SelectedItem as Item;
                    if (isFTP)
                    {
                        if (itemToRename.IsDirectory())
                        {
                            string rename = Operations.Rename(itemToRename, currentPathRight, true);
                            client.Rename(currentPathRight + itemToRename.Name, rename);
                        }
                        else
                        {
                            string rename = Operations.Rename(itemToRename, currentPathRight, true);
                            client.Rename(currentPathRight + itemToRename.Name + itemToRename.Extension, rename);
                        }
                        RefreshDirectiories();
                        activePanel = 0;
                    }
                    else
                    {
                        string newPath = Operations.Rename(itemToRename, currentPathRight);
                        if (newPath != "")
                        {
                            if (rightHistory.Contains(itemToRename.Path))
                            {
                                rightHistory[rightHistory.IndexOf(itemToRename.Path)] = newPath;
                            }
                            RefreshDirectiories();
                            activePanel = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteItem()
        {
            try
            {
                if (activePanel == -1)
                {
                    if (Operations.Delete(GetAllSelectedItems(leftDirectory)))
                    {
                        RefreshDirectiories();
                        activePanel = 0;
                    }
                }
                else if (activePanel == 1)
                {
                    if (isFTP)
                    {
                        List<Item> tmp = GetAllSelectedItems(rightDirectory);
                        if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            foreach (Item itemToDelete in tmp)
                            {
                                if (itemToDelete.IsDirectory())
                                {
                                    client.Delete(currentPathRight + itemToDelete.Name);
                                }
                                else
                                {
                                    client.Delete(currentPathRight + itemToDelete.Name + itemToDelete.Extension);
                                }
                            }
                        }
                        RefreshDirectiories();
                        activePanel = 0;
                    }
                    else
                    {
                        if (Operations.Delete(GetAllSelectedItems(rightDirectory)))
                        {
                            RefreshDirectiories();
                            activePanel = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CloseProgram()
        {
            if (MessageBox.Show("Вы действительно хотите выйти из программы?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Close();
            }
        }
        private void EnterKeyHanlder()
        {
            try
            {
                if (leftDirectoryPathTextBox.Visibility == Visibility.Visible)
                {
                    leftHistory.Add(currentPathLeft);
                    leftDirectory.ItemsSource = Operations.LoadDirectory(currentPathLeft, leftDirectoryPathTextBox.Text);
                    leftDirectory.Items.Refresh();
                    currentPathLeft = leftDirectoryPathTextBox.Text;
                    leftDirectoryPathLabel.Content = currentPathLeft;
                    leftDirectoryPathLabel.Visibility = Visibility.Visible;
                    leftDirectoryPathTextBox.Visibility = Visibility.Hidden;

                    int i = 0;
                    foreach (Drive drive in leftDrive.Items)
                    {
                        if (drive.Name == System.IO.Path.GetPathRoot(currentPathLeft))
                        {
                            cbActive = false;
                            leftDrive.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    DeactivatePanel();
                }
                else if (rightDirectoryPathTextBox.Visibility == Visibility.Visible)
                {
                    rightHistory.Add(currentPathRight);
                    rightDirectory.ItemsSource = Operations.LoadDirectory(currentPathRight, rightDirectoryPathTextBox.Text);
                    rightDirectory.Items.Refresh();
                    currentPathRight = rightDirectoryPathTextBox.Text;
                    rightDirectoryPathLabel.Content = currentPathRight;
                    rightDirectoryPathLabel.Visibility = Visibility.Visible;
                    rightDirectoryPathTextBox.Visibility = Visibility.Hidden;
                    int i = 0;
                    foreach (Drive drive in rightDrive.Items)
                    {
                        if (drive.Name == System.IO.Path.GetPathRoot(currentPathRight))
                        {
                            cbActive = false;
                            rightDrive.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    DeactivatePanel();
                }
                else
                {
                    ChangeDirectory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilesSearchTB_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                SearchButton(sender, e);
        }

        private void EscapeKeyHanlder()
        {
            try
            {
                if (leftDirectoryPathTextBox.Visibility == Visibility.Visible)
                {
                    leftDirectoryPathTextBox.Text = currentPathLeft;
                    leftDirectoryPathLabel.Visibility = Visibility.Visible;
                    leftDirectoryPathTextBox.Visibility = Visibility.Hidden;
                }
                else if (rightDirectoryPathTextBox.Visibility == Visibility.Visible)
                {
                    rightDirectoryPathTextBox.Text = currentPathRight;
                    rightDirectoryPathLabel.Visibility = Visibility.Visible;
                    rightDirectoryPathTextBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    CloseProgram();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeactivatePanel()
        {
            if (activePanel == -1)
            {
                if (isRightSelected)
                {
                    activePanel = 1;
                }
                else
                {
                    activePanel = 0;
                }
                isLeftSelected = false;
            }
            else if (activePanel == 1)
            {
                if (isLeftSelected)
                {
                    activePanel = -1;
                }
                else
                {
                    activePanel = 0;
                }
                isRightSelected = false;
            }
        }
        private void ShowDetails()
        {
            ItemDetails newDetailsWindow = new ItemDetails();
            Item selectedItem = new Item(); ;
            if (activePanel == -1)
            {
                selectedItem = leftDirectory.SelectedItem as Item;
            }
            else if (activePanel == 1)
            {
                selectedItem = rightDirectory.SelectedItem as Item;
            }
            if(isFTP) newDetailsWindow.SetItem(client.Host + "/" + selectedItem.Path);
            else newDetailsWindow.SetItem(selectedItem.Path);
            newDetailsWindow.Show();
        }
        private void CreateFileOrDirectory(string type)
        {
            if (activePanel == -1)
            {
                Operations.Create(currentPathLeft, type);
            }
            else if (activePanel == 1)
            {
                if(isFTP) Operations.Create(currentPathRight, "Directory", client, true);
                else Operations.Create(currentPathRight, type);
            }
            activePanel = 0;
            RefreshDirectiories();
        }
        private void ShowHelp()
        {
            StringBuilder help = new StringBuilder();
            help.AppendLine("Стрелки влево и вправо - переключение между окнами");
            help.AppendLine("Стрелки вверх и вниз - переключение между каталогами и файлами");
            help.AppendLine("Enter - вход в каталог или запуск файла");
            help.AppendLine("Backspace - выйти из текущего каталога на уровень выше");
            help.AppendLine("F2-F9 - операции с файлами и каталогами");
            help.AppendLine("Delete - удаление каталога или файла");
            help.AppendLine("Escape - выход из программы");
            help.AppendLine();
            help.AppendLine("Правый клик мыши на адрес текущего местоположения, чтобы открыть режим редактирования пути\nEnter - для подтверждения\nEscape - для отмены");
            help.AppendLine("Правый клик мыши на файл или каталог, чтобы открыть контекстное меню");
            help.AppendLine();
            help.AppendLine();
            MessageBox.Show(help.ToString());
        }
        private List<Item> GetAllSelectedItems(ListView source)
        {
            List<Item> selectedItems = new List<Item>();

            foreach (Item item in source.SelectedItems)
            {
                selectedItems.Add(item);
            }

            return selectedItems;
        }
        #endregion

    }


}
