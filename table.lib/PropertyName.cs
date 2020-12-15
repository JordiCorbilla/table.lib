namespace table.lib
{
    public class PropertyName
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsCollection { get; set; }
        public int PropertyIndex { get; set; }

        public PropertyName(string name)
        {
            Name = name;
            Index = 0;
            IsCollection = false;
        }

        public PropertyName(string name, int index, int propertyIndex)
        {
            Name = name;
            Index = index;
            PropertyIndex = propertyIndex;
            IsCollection = true;
        }
    }
}