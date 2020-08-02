using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZelluSimConsolaz.ConsoleCLI
{
    public class ConfigChanger
    {
        public struct Item
        {
            //state:

            public string Name { get; }


            //c'tors:

            public Item(string name)
            {
                Name = name;
            }
        }

        protected CliConfig conf;
        protected int item = 0;
        protected bool edit = false;
        protected ConsoleColor back = ConsoleColor.DarkBlue;
        protected ConsoleColor fore = ConsoleColor.Yellow;
        protected ConsoleColor text = ConsoleColor.White;
        protected List<Item> items = new List<Item>();

        public ConfigChanger(CliConfig conf)
        {
            this.conf = conf;

            items.Add(new Item("topLeftX"));
            items.Add(new Item("topLeftY"));
            items.Add(new Item("alifeText"));
            items.Add(new Item("deadText"));
            items.Add(new Item("halfAlifeText"));
            items.Add(new Item("alifeColor"));
            items.Add(new Item("deadColor"));
            items.Add(new Item("halfAlifeColor"));
            items.Add(new Item("runningText"));
            items.Add(new Item("stoppedText"));
            items.Add(new Item("runningColor"));
            items.Add(new Item("stoppedColor"));
            items.Add(new Item("delayMilliSeconds"));
            items.Add(new Item("feedbackColorOkay"));
            items.Add(new Item("feedbackColorError"));
            items.Add(new Item("generationText"));
            items.Add(new Item("generationTextCulture"));
            items.Add(new Item("generationTextColor"));
            items.Add(new Item("promptText"));
            items.Add(new Item("promptColor"));
            items.Add(new Item("helpColor"));
            items.Add(new Item("backColor"));
        }

        public void MainLoop()
        {
            Console.SetWindowSize(60, 30);
            ConsoleKeyInfo key;
            do
            {
                RenderList();
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.PageUp || key.Key == ConsoleKey.LeftArrow)
                    item = item == 0 ? items.Count - 1 : item - 1;
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    item = (item + 1) % items.Count;
                else
                if (key.Key == ConsoleKey.Enter)
                    ConfigureItem(items[item]);
            }
            while (key.Key != ConsoleKey.Escape);
        }

        protected void RenderList()
        {
            int i = 0;
            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
            Console.Clear();

            foreach(Item item in items)
            {
                RenderWord(item.Name, i);
                i++;
            }
        }

        protected void RenderWord(string word, int index)
        {
            Console.WriteLine((index == item ? "[" : " ") + word + (index == item ? "]" : " "));
        }

        protected void ConfigureItem(Item item)
        {
            Type type = GetField(item.Name).FieldType;
            if (type == typeof(System.Int32))
            {
                int input = UserEntersInt32(item);
                SetItemToConfig(item, input);
            }
            else
            if (type == typeof(string))
            {
                string input = UserEntersString(item);
                SetItemToConfig(item, input);
            }
            else
            if (type == typeof(ConsoleColor))
            {
                ConsoleColor input = UserEntersColor(item);
                SetItemToConfig(item, input);
            }
            else
            if (type == typeof(CultureInfo))
            {
                CultureInfo input = UserEntersCulture(item);
                SetItemToConfig(item, input);
            }
            else
                throw new NotImplementedException("can't handle type (yet): " + type);
        }

        protected string UserEntersString(Item item)
        {
            Console.WriteLine();
            Console.Write("Enter new value: ");
            return Console.ReadLine();
        }

        protected int UserEntersInt32(Item item)
        {
            Console.WriteLine();
            Console.Write("Enter new value: ");
            string str = Console.ReadLine();
            if (int.TryParse(str, out int result))
                return result;
            return GetInt32FromConfig(item);
        }

        protected ConsoleColor UserEntersColor(Item item)
        {
            //TODO
            return ConsoleColor.White;
        }

        protected CultureInfo UserEntersCulture(Item item)
        {
            //TODO
            return CultureInfo.InvariantCulture;
        }


        protected FieldInfo GetField(string name) => typeof(CliConfig).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

        protected void SetItemToConfig(Item item, string val) => GetField(item.Name).SetValue(conf, val);
        protected void SetItemToConfig(Item item, int val) => GetField(item.Name).SetValue(conf, val);
        protected void SetItemToConfig(Item item, ConsoleColor val) => GetField(item.Name).SetValue(conf, val);
        protected void SetItemToConfig(Item item, CultureInfo val) => GetField(item.Name).SetValue(conf, val);

        protected string GetStringFromConfig(Item item) => (string)GetField(item.Name).GetValue(conf);
        protected int GetInt32FromConfig(Item item) => (int)GetField(item.Name).GetValue(conf);
        protected ConsoleColor GetColorFromConfig(Item item) => (ConsoleColor)GetField(item.Name).GetValue(conf);
        protected CultureInfo GetCultureFromConfig(Item item) => (CultureInfo)GetField(item.Name).GetValue(conf);
    }
}
