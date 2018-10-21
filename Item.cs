using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_WPF
{
    public class Item
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }

        public Item()
        {
        }
        public Item(string argName, string argType, string argSize)
        {
            Name = argName;
            Extension = argType;
            Size = argSize;
        }
        public Item(string argName, string argType, string argSize, string argPath)
        {
            Name = argName;
            Extension = argType;
            Size = argSize;
            Path = argPath;
        }

        public bool IsDirectory()
        {
            if (Extension == "<dir>")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
