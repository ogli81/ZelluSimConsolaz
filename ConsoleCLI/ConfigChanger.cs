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
                RenderWord(conf.GetItem(i), i);
        }

        protected void RenderWord(Item it, int index)
        {
            string word = it.Name;
            Console.ForegroundColor = fore;
            Console.Write((index == item ? "[" : " ") + word + (index == item ? "]" : " "));
            Console.ForegroundColor = info;
            Console.WriteLine(" " + it.Info);
        }

        protected void RenderList2(int item)
        {
            Console.BackgroundColor = back;
            Console.Clear();
            int longest = 0;
            for (int i = 0; i < ColorInfo.NumColors; ++i)
                longest = Math.Max(longest, ColorInfo.GetColorName(i).Length);
            for (int i = 0; i < ColorInfo.NumColors; ++i)
                RenderWord2(i, item, longest);
        }

        protected void RenderWord2(int index, int item, int longestName)
        {
            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
            string word = ColorInfo.GetColorName(index);
            Console.ForegroundColor = fore;
            Console.Write((index == item ? "[" : " ") + word + (index == item ? "]" : " "));
            string arrow = "";
            arrow = arrow.PadRight(longestName - word.Length, '-');
            arrow += "->";
            Console.Write(arrow);
            Console.BackgroundColor = ColorInfo.GetColor(index);
            Console.ForegroundColor = ColorInfo.IsDarkColor(index) ? ConsoleColor.White : ConsoleColor.Black;
            Console.WriteLine("  (color looks like this)  ");
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
            ConsoleColor originalColor = GetColorFromConfig(item);

            int item2 = -1;
            ConsoleColor aColor;
            do
            {
                item2++;
                aColor = ColorInfo.GetColor(item2);
            }
            while (!aColor.Equals(originalColor));

            ConsoleKeyInfo key;
            do
            {
                RenderList2(item2);
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.PageUp || key.Key == ConsoleKey.LeftArrow)
                    item2 = item2 == 0 ? ColorInfo.NumColors - 1 : item2 - 1;
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    item2 = (item2 + 1) % ColorInfo.NumColors;
                else
                if (key.Key == ConsoleKey.Enter)
                    return ColorInfo.GetColor(item2);
                else
                if (key.Key == ConsoleKey.Escape)
                    return originalColor;
            }
            while (true);
        }

        protected CultureInfo UserEntersCulture(Item item)
        {
            //ConsoleKeyInfo key;
            //String str = "";
            //do
            //{
            //    key = Console.ReadKey();
            //    if (key.Key == ConsoleKey.Backspace)
            //        str = str.Length == 0 ? str : str.Substring(0, str.Length - 1);
            //    else
            //    if (key.Key == )
            //}
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
