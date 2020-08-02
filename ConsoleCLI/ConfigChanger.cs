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
            public Type Type { get; }


            //c'tors:

            public Item(string name, Type type)
            {
                Name = name;
                Type = type;
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

            items.Add(new Item("topLeftX", typeof(System.Int32)));
            items.Add(new Item("topLeftY", typeof(System.Int32)));
            items.Add(new Item("alifeText", typeof(string)));
            items.Add(new Item("deadText", typeof(string)));
            items.Add(new Item("halfAlifeText", typeof(string)));
            items.Add(new Item("alifeColor", typeof(ConsoleColor)));
            items.Add(new Item("deadColor", typeof(ConsoleColor)));
            items.Add(new Item("halfAlifeColor", typeof(ConsoleColor)));
            items.Add(new Item("runningText", typeof(string)));
            items.Add(new Item("stoppedText", typeof(string)));
            items.Add(new Item("runningColor", typeof(ConsoleColor)));
            items.Add(new Item("stoppedColor", typeof(ConsoleColor)));
            items.Add(new Item("delayMilliSeconds", typeof(System.Int32)));
            items.Add(new Item("feedbackColorOkay", typeof(ConsoleColor)));
            items.Add(new Item("feedbackColorError", typeof(ConsoleColor)));
            items.Add(new Item("generationText", typeof(string)));
            items.Add(new Item("generationTextCulture", typeof(CultureInfo)));
            items.Add(new Item("generationTextColor", typeof(ConsoleColor)));
            items.Add(new Item("promptText", typeof(string)));
            items.Add(new Item("promptColor", typeof(ConsoleColor)));
            items.Add(new Item("helpColor", typeof(ConsoleColor)));
            items.Add(new Item("backColor", typeof(ConsoleColor)));
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
                RenderWord(item.Name+","+item.Type.ToString(), i);
                i++;
            }
        }

        protected void RenderWord(string word, int index)
        {
            Console.WriteLine((index == item ? "[" : " ") + word + (index == item ? "]" : " "));
        }

        protected void ConfigureItem(Item item)
        {
            if (item.Type == typeof(System.Int32))
            {
                int input = UserEntersInt32(item);
                SetItemToConfig(item, input);
            }
            else
            if (item.Type == typeof(string))
            {
                string input = UserEntersString(item);
                SetItemToConfig(item, input);
            }
            else
            if (item.Type == typeof(ConsoleColor))
            {
                ConsoleColor input = UserEntersColor(item);
                SetItemToConfig(item, input);
            }
            else
            if (item.Type == typeof(CultureInfo))
            {
                CultureInfo input = UserEntersCulture(item);
                SetItemToConfig(item, input);
            }
            else
                throw new NotImplementedException("can't handle type: " + item.Type);
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
