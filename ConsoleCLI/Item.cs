namespace ZelluSimConsolaz.ConsoleCLI
{
    //minor helper struct
    public struct Item
    {
        public string Name { get; }
        public string Info { get; }
        public Item(string name, string info)
        {
            Name = name;
            Info = info;
        }
    }

    //minor helper interface
    public interface IHasItems
    {
        int NumItems { get; }

        Item GetItem(int index);
    }
}
