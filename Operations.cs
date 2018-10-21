using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace File_Manager_WPF
{
    static class Operations
    {

        public static List<Item> LoadDirectory(string oldPath, string newPath)
        {
            List<Item> newDirectory = new List<Item>();
            string[] files = Directory.GetFiles(newPath);
            string[] dirs = Directory.GetDirectories(newPath);

            DirectoryInfo tmp = new DirectoryInfo(newPath);
            if (tmp.Parent != null)
            {
                newDirectory.Add(new Item("..", "<dir>", ""));
            }

            foreach (string dir in dirs)
            {
                FileInfo fileInfo = new FileInfo(dir);
                newDirectory.Add(new Item(fileInfo.Name, "<dir>", "", dir));
            }

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                newDirectory.Add(new Item(System.IO.Path.GetFileNameWithoutExtension(file), fileInfo.Extension, SizeConverter(fileInfo.Length), file));
            }
            return newDirectory;
        }

        public static void Copy(List<Item> itemsToCopy, string sourcePath, string targetPath, bool isFtp = false)
        {
            foreach (Item itemToCopy in itemsToCopy)
            {
                if (itemToCopy.IsDirectory())
                {
                    string sourceFile = Path.Combine(sourcePath, itemToCopy.Name);
                    string targetFile = Path.Combine(targetPath, itemToCopy.Name);

                    CopyDirectory(sourceFile, targetFile, true);
                }
                else
                {
                    string sourceFile = Path.Combine(sourcePath, itemToCopy.Name + itemToCopy.Extension);
                    string targetFile = Path.Combine(targetPath, itemToCopy.Name + itemToCopy.Extension);


                    if (File.Exists(targetFile))
                    {
                        if (MessageBox.Show("Перезаписать файл?", "Файл уже существует", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            File.Copy(sourceFile, targetFile, true);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        File.Copy(sourceFile, targetFile);
                    }
                }
            }
        }

        private static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Путь к каталогу не существует или не может быть найден: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void Move(List<Item> itemsToMove, string sourcePath, string targetPath, Client client = null, bool isFtp = false, bool isLeft = true)
        {
            foreach (Item itemToMove in itemsToMove)
            {
                if (itemToMove.IsDirectory())
                {
                    string sourceFile = System.IO.Path.Combine(sourcePath, itemToMove.Name);
                    string targetFile;
                    if (isFtp) targetFile = targetPath + itemToMove.Name;
                    else targetFile = System.IO.Path.Combine(targetPath, itemToMove.Name);
                    if (isFtp)
                    {
                        if(isLeft) client.Upload(targetFile, sourceFile);
                        else client.Download(targetFile, sourceFile);
                    }
                    else
                    {
                        if (Directory.Exists(targetFile))
                        {
                            if (MessageBox.Show("Перезаписать файл?", "Файл уже существует", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                Directory.Move(sourceFile, targetFile);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            Directory.Move(sourceFile, targetFile);
                        }
                    }
                }
                else
                {
                    string sourceFile = System.IO.Path.Combine(sourcePath, itemToMove.Name + itemToMove.Extension);
                    string targetFile;
                    if (isFtp) targetFile = targetPath + itemToMove.Name + itemToMove.Extension;
                    else targetFile = System.IO.Path.Combine(targetPath, itemToMove.Name + itemToMove.Extension);
                    if (isFtp)
                    {
                        if (isLeft) client.Upload(targetFile, sourceFile);
                        else client.Download(targetFile, sourceFile);
                    }
                    else
                    {
                        if (File.Exists(targetFile))
                        {
                            if (MessageBox.Show("Перезаписать файл?", "Файл уже существует", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                File.Move(sourceFile, targetFile);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            File.Move(sourceFile, targetFile);
                        }
                    }
                }
            }
        }

        public static bool Delete(List<Item> itemsToDelete)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (Item itemToDelete in itemsToDelete)
                {
                    if (itemToDelete.IsDirectory())
                    {
                        Directory.Delete(itemToDelete.Path, true);
                    }
                    else
                    {
                        File.Delete(itemToDelete.Path);
                    }
                }
                return true;
            }
            return false;
        }

        public static void Create(string targetPath, string type, Client client = null, bool isFtp = false)
        {
            NewItem newItemWindow = new NewItem();

            if (type == "File")
            {
                newItemWindow.SetItemType("файл");
                if (newItemWindow.ShowDialog() == true)
                {
                    FileStream f = File.Create(Path.Combine(targetPath, newItemWindow.ItemNameTB.Text + newItemWindow.ItemListTB.Text));
                    f.Close();
                }
            }
            else if (type == "Directory")
            {
                newItemWindow.SetItemType("каталог");
                newItemWindow.IMessageLabel.Visibility = Visibility.Hidden;
                newItemWindow.ItemListTB.IsEnabled = false;
                if (newItemWindow.ShowDialog() == true)
                {
                    if (isFtp) client.CreateDirectory(targetPath + newItemWindow.ItemNameTB.Text);
                    else Directory.CreateDirectory(Path.Combine(targetPath, newItemWindow.ItemNameTB.Text));
                }
            }
        }

        public static string Rename(Item itemToRename, string sourcePath, bool isFtp = false)
        {
            NewItem newItemWindow = new NewItem();
            newItemWindow.SetItemType("Rename");
            string targetPath = "";

            if (itemToRename.IsDirectory())
            {
                newItemWindow.SetValue(itemToRename.Name);
            }
            else
            {
                newItemWindow.SetValue(itemToRename.Name + itemToRename.Extension);
            }

            newItemWindow.SetItemType("Rename");
            if (newItemWindow.ShowDialog() == true)
            {
                if (itemToRename.IsDirectory())
                {
                    if (isFtp)
                        return newItemWindow.ItemNameTB.Text;
                    else
                    {
                        targetPath = Path.Combine(sourcePath, newItemWindow.ItemNameTB.Text);
                        sourcePath = Path.Combine(sourcePath, itemToRename.Name);
                        if (Directory.Exists(targetPath))
                        {
                            if (MessageBox.Show("Перезаписать каталог?", "Каталог уже существует",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                Directory.Move(sourcePath, targetPath);
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            Directory.Move(sourcePath, targetPath);
                        }
                    }
                }
                else
                {
                    if (isFtp) return newItemWindow.ItemNameTB.Text;
                    targetPath = Path.Combine(sourcePath, newItemWindow.ItemNameTB.Text);
                    sourcePath = Path.Combine(sourcePath, itemToRename.Name + itemToRename.Extension);
                    if (File.Exists(targetPath))
                    {
                        if (MessageBox.Show("Перезаписать файл?", "Файл уже существует", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            File.Move(sourcePath, targetPath);
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        File.Move(sourcePath, targetPath);
                    }
                }
            }
            return targetPath;
        }

        public static string SizeConverter(double size)
        {
            if (size == -1)
            {
                return "";
            }
            else if (size < 1024)
            {
                return string.Format("{0:f2} B", size);
            }
            else if (size < 1024 * 1024)
            {
                return string.Format("{0:f2} KB", size / 1024);
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return string.Format("{0:f2} MB", size / (1024 * 1024));
            }
            else
            {
                return string.Format("{0:f2} GB", size / (1024 * 1024 * 1024));
            }
        }

        public static double GetDirectorySize(string path)
        {
            double result = 0;
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    result += file.Length;
                }
                foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                {
                    result += GetDirectorySize(directory.FullName);
                }
                return result;

            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return result;
            }
        }
    }
}
