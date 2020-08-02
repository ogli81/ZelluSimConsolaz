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
    public partial class ConfigChanger
    {
        protected CliConfig conf;
        protected int item = 0;
        protected bool edit = false;
        protected ConsoleColor back = ConsoleColor.DarkBlue;
        protected ConsoleColor fore = ConsoleColor.Yellow;
        protected ConsoleColor info = ConsoleColor.Gray;
        protected ConsoleColor text = ConsoleColor.White;
        

        public ConfigChanger(CliConfig conf)
        {
            this.conf = conf;
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
                    item = item == 0 ? conf.NumItems - 1 : item - 1;
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    item = (item + 1) % conf.NumItems;
                else
                if (key.Key == ConsoleKey.Enter)
                    ConfigureItem(conf.GetItem(item));
            }
            while (key.Key != ConsoleKey.Escape);
        }

        protected void RenderList()
        {
            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
            Console.Clear();
            for(int i = 0; i < conf.NumItems; ++i)
                RenderWord(conf.GetItem(i).Name, i);
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
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = text;
            string input = Console.ReadLine();
            Console.ForegroundColor = was;
            return input;
        }

        protected int UserEntersInt32(Item item)
        {
            Console.WriteLine();
            Console.Write("Enter new value: ");
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = text;
            string str = Console.ReadLine();
            Console.ForegroundColor = was;
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
