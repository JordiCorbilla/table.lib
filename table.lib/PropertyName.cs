namespace table.lib
{
    public class PropertyName
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsCollection { get; set; }

        public PropertyName(string name)
        {
            Name = name;
            Index = 0;
            IsCollection = false;
        }

        public PropertyName(string name, int index)
        {
            Name = name;
            Index = index;
            IsCollection = true;
        }
    }
}