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
}
