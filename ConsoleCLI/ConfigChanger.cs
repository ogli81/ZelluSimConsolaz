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
                if(input != null)
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
            ConsoleKeyInfo key;
            String str = "";
            List<CultureInfo> cul = new List<CultureInfo>();
            CultureInfo[] all = CultureInfo.GetCultures(CultureTypes.AllCultures);
            int sel = -1;

            Console.Clear();
            Console.ForegroundColor = conf.HelpColor;
            Console.WriteLine("Chose your language/format - use these keys:");
            Console.WriteLine("[ESC] = abort anytime (doesn't change the format)");
            Console.WriteLine("[ENTER] = select this language (only if valid)");
            Console.WriteLine("[TAB] = extend to match the next fitting language");
            Console.WriteLine("[Up]/[Left]/[PageUp] = cycle to previous language");
            Console.WriteLine("[Down]/[Right]/[PageDown] = cycle to next language");
            Console.WriteLine("[BackSpace] = delete the last character");
            Console.WriteLine("[A]..[Z] (and others) = enter a new character");
            Console.WriteLine();

            do
            {
                //Console.Clear(); <--- this is problematic, because if [UpArrow] or [LeftArrow] is pressed, we see nothing...
                //---> the double-buffered console might solve this problem

                Console.ForegroundColor = conf.PromptColor;
                Console.Write(conf.PromptText);
                Console.ForegroundColor = sel < 0 ? conf.FeedbackColorError : conf.FeedbackColorOkay;
                Console.Write(str);
                Console.ForegroundColor = conf.PromptColor;
                
                cul.Clear();
                foreach (CultureInfo ci in all)
                    if (ci.DisplayName.ToLower().StartsWith(str.ToLower()))
                        cul.Add(ci);

                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Backspace)
                {
                    str = str.Length == 0 ? str : str.Substring(0, str.Length - 1);
                    sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                }
                else
                if (key.Key == ConsoleKey.Tab && cul.Count > 0)
                {
                    str = cul[0].DisplayName;
                    sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                }
                else
                if (key.Key == ConsoleKey.Enter && sel >= 0)
                {
                    return all[sel];
                }
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.PageDown)
                {
                    if (sel < 0)
                    {
                        sel = 0;
                    }
                    else
                    {
                        sel++;
                        if (sel >= all.Length)
                            sel = 0;
                        str = all[sel].DisplayName;
                    }
                }
                else
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.PageUp)
                {
                    if (sel < 0)
                    {
                        sel = all.Length - 1;
                    }
                    else
                    {
                        sel--;
                        if (sel == -1)
                            sel = all.Length - 1;
                        str = all[sel].DisplayName;
                    }
                }
                else
                if (key.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                else
                {
                    str += key.KeyChar;
                    sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                }
                Console.WriteLine();
            }
            while (true);
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
