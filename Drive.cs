namespace File_Manager_WPF
{
    internal class Drive
    {
        public string Name { get; set; }
        public string Label { get; set; }

        public override string ToString()
        {
            return Name.Remove(Name.Length - 1) + " " + Label;
        }
    }
}