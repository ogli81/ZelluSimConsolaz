using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using ZelluSim.Misc;

namespace ZelluSimConsolaz.ConsoleCLI
{
    /// <summary>
    /// Allows you to change attributes of a something called 'target' (generic type T). 
    /// Supports our CLI and uses the <see cref="IHasItems"/> interface.
    /// <br></br>
    /// NOTE: This class makes use of a reflection mechanism and there are still some 
    /// open questions regarding the reflection mechanism (see our TODO in this class).
    /// </summary>
    /// <typeparam name="T">The generic type of our 'target' object</typeparam>
    public class ItemsChanger<T> where T : IHasItems
    {
        protected T target;
        protected CliConfig conf;
        protected int item = 0;
        protected bool edit = false;

        /// <summary>
        /// C'tor for our changer. No heavy lifting - can be used many times without problems.
        /// </summary>
        /// <param name="target">the thing that we want to change via CLI</param>
        /// <param name="conf">the format informations (colors etc.) of our CLI</param>
        public ItemsChanger(T target, CliConfig conf)
        {
            this.target = target;
            this.conf = conf;
        }

        /// <summary>
        /// Use this method to start the rendering process. It will terminate (eventually). 
        /// </summary>
        public void MainLoop()
        {
            Console.SetWindowSize(60, 30);
            ConsoleKeyInfo key;
            do
            {
                Thread.Sleep(conf.DelayMilliSeconds);
                RenderList();
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.PageUp || key.Key == ConsoleKey.LeftArrow)
                    item = item == 0 ? GetNumItems() - 1 : item - 1;
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    item = (item + 1) % GetNumItems();
                else
                if (key.Key == ConsoleKey.Enter)
                    ConfigureItem(item, target);
            }
            while (key.Key != ConsoleKey.Escape);
        }

        protected virtual Item GetItem(int index) => target.GetItem(index);

        protected virtual int GetNumItems() => target.NumItems;

        protected void RenderList()
        {
            Console.BackgroundColor = conf.BackColor;
            Console.ForegroundColor = conf.HelpColor;
            Console.Clear();
            for (int i = 0; i < target.NumItems; ++i)
                RenderWord(target.GetItem(i), i);
        }

        protected void RenderWord(Item it, int index)
        {
            string word = it.Name;
            if(index == item)
            {
                Console.ForegroundColor = conf.PromptColor;
                Console.Write("[");
                Console.ForegroundColor = conf.HelpColor;
                Console.Write(word);
                Console.ForegroundColor = conf.PromptColor;
                Console.Write("]");
            }
            else
            {
                Console.ForegroundColor = conf.HelpColor;
                Console.Write(" " + word + " ");
            }
            Console.ForegroundColor = conf.InfoColor;
            Console.WriteLine(" " + it.Info);
        }

        protected void RenderList2(int item)
        {
            Console.BackgroundColor = conf.BackColor;
            Console.Clear();
            int longest = 0;
            for (int i = 0; i < ColorInfo.NumColors; ++i)
                longest = Math.Max(longest, ColorInfo.GetColorName(i).Length);
            for (int i = 0; i < ColorInfo.NumColors; ++i)
                RenderWord2(i, item, longest);
        }

        protected void RenderWord2(int index, int item, int longestName)
        {
            Console.BackgroundColor = conf.BackColor;
            string word = ColorInfo.GetColorName(index);
            if(index == item)
            {
                Console.ForegroundColor = conf.PromptColor;
                Console.Write("[");
                Console.ForegroundColor = conf.HelpColor;
                Console.Write(word);
                Console.ForegroundColor = conf.PromptColor;
                Console.Write("]");
            }
            else
            {
                Console.ForegroundColor = conf.HelpColor;
                Console.Write($" {word} ");
            }
            string arrow = "";
            arrow = arrow.PadRight(longestName - word.Length, '-');
            arrow += "->";
            Console.Write(arrow);
            Console.BackgroundColor = ColorInfo.GetColor(index);
            Console.ForegroundColor = ColorInfo.IsDarkColor(index) ? ConsoleColor.White : ConsoleColor.Black;
            Console.WriteLine("  (color looks like this)  ");
        }

        protected void RenderList3(Enum en, int current)
        {
            Console.BackgroundColor = conf.BackColor;
            Console.Clear();
            string[] names = Enum.GetNames(en.GetType());
            for (int i = 0; i < names.Length; ++i)
                RenderWord3(names[i], i, current);
        }

        protected void RenderWord3(string name, int index, int current)
        {
            Console.ForegroundColor = conf.PromptColor;
            Console.Write(index == current ? "[" : " ");
            Console.ForegroundColor = conf.HelpColor;
            Console.Write(name);
            Console.ForegroundColor = conf.PromptColor;
            Console.WriteLine(index == current ? "]" : " ");
        }

        protected void RenderList4(Enum en, long[] currentValues)
        {
            //Array vals = Enum.GetValues();
            //en.HasFlag()
        }

        protected void RenderWord4(long aValue, string aName, long[] currentValues)
        {

        }

        protected void ConfigureItem(int item, T target)
        {
            Item theItem = GetItem(item);
            Type type = GetProperty(theItem.Name).PropertyType;

            if (type == typeof(int))
            {
                int input = UserEntersInt32(theItem, target);
                SetItem(theItem, input, target);
            }
            else
            if (type == typeof(decimal))
            {
                decimal input = UserEntersDecimal(theItem, target);
                SetItem(theItem, input, target);
            }
            else
            if (type == typeof(string))
            {
                string input = UserEntersString(theItem, target);
                SetItem(theItem, input, target);
            }
            else
            if (type == typeof(ConsoleColor))
            {
                ConsoleColor input = UserEntersColor(theItem, target);
                SetItem(theItem, input, target);
            }
            else
            if (type == typeof(CultureInfo))
            {
                CultureInfo input = UserEntersCulture(theItem, target);
                if (input != null)
                    SetItem(theItem, input, target);
            }
            else
            if (type == typeof(bool))
            {
                bool? input = UserEntersBoolean(theItem, target);
                if (input != null)
                    SetItem(theItem, (bool)input, target);
            }
            else
            if (type.BaseType == typeof(Enum))
            {
                if (type.GetCustomAttributes<FlagsAttribute>().Any()) //tested this: works!
                {
                    Enum input = UserEntersFlagsEnum(theItem, target);
                    if (input != null)
                        SetItem(theItem, (Enum)input, target);
                }
                else
                {
                    Enum input = UserEntersEnum(theItem, target);
                    if (input != null)
                        SetItem(theItem, (Enum)input, target);
                }
            }
            else
            {
                ConfigureItemSub(theItem, type, target);
            }
        }

        //this method should only be used by other programmers
        //it's best to just implement as many types in this class as we can
        protected virtual void ConfigureItemSub(Item item, Type type, object target)
        {
            throw new NotImplementedException($"can't handle type: {type}");
        }

        //user selects 'Yes' or 'No' ('On' or 'Off') ('1' or '0) ('Day' or 'Night') ('A' or 'B')
        protected bool? UserEntersBoolean(Item item, T target, string YesStr = "Yes", string NoStr = "No")
        {
            //bool originalBool = GetBool(item, target);
            Console.WriteLine();
            Console.Write("Select: ");
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = conf.PromptColor;
            int[] pos = { Console.CursorLeft, Console.CursorTop }; //better do this with tupels
            ConsoleKeyInfo key;
            bool yes = GetBool(item, target);
            do
            {
                Console.CursorLeft = pos[0];
                Console.CursorTop = pos[1];
                Console.WriteLine(yes ?
                    $"> {YesStr} <   {NoStr}  " :
                    $"  {YesStr}   > {NoStr} <");

                key = Console.ReadKey();
                if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown)
                    yes = yes ? !yes : yes;
                else
                if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.PageUp)
                    yes = yes ? yes : !yes;
                else
                if (key.Key == ConsoleKey.Tab)
                    yes = !yes;
            }
            while (key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.Enter);
            Console.ForegroundColor = was;
            //return key.Key == ConsoleKey.Escape ? originalBool : yes;
            return key.Key == ConsoleKey.Escape ? (bool?) null : (bool?) yes;
        }

        //user enters a string - no way to escape the input
        protected string UserEntersString(Item item, T target)
        {
            string originalStr = GetString(item, target);
            Console.WriteLine();
            Console.WriteLine($"Original value: {originalStr}");
            Console.WriteLine("Enter new value: ");
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = conf.PromptColor;
            Console.Write(conf.PromptText);
            Console.ForegroundColor = conf.UserColor;
            Console.CursorVisible = true;
            string input = Console.ReadLine();
            Console.CursorVisible = false;
            Console.ForegroundColor = was;
            return input;
        }

        //user enters a int - escape the input with strings that can't be parsed into an int
        protected int UserEntersInt32(Item item, T target)
        {
            int originalInt = GetInt32(item, target);
            Console.WriteLine();
            Console.WriteLine($"Original value: {originalInt}");
            Console.WriteLine("Enter new value: ");
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = conf.PromptColor;
            Console.Write(conf.PromptText);
            Console.ForegroundColor = conf.UserColor;
            Console.CursorVisible = true;
            string str = Console.ReadLine();
            Console.CursorVisible = false;
            Console.ForegroundColor = was;
            if (int.TryParse(str, out int result))
                return result;
            else
                return originalInt;
        }

        //user enters a int - escape the input with strings that can't be parsed into an int
        protected decimal UserEntersDecimal(Item item, T target)
        {
            decimal originalInt = GetDecimal(item, target);
            Console.WriteLine();
            Console.WriteLine($"Original value: {originalInt}");
            Console.WriteLine("Enter new value: ");
            ConsoleColor was = Console.ForegroundColor;
            Console.ForegroundColor = conf.PromptColor;
            Console.Write(conf.PromptText);
            Console.ForegroundColor = conf.UserColor;
            Console.CursorVisible = true;
            string str = Console.ReadLine();
            Console.CursorVisible = false;
            Console.ForegroundColor = was;
            if (decimal.TryParse(str, out decimal result))
                return result;
            else
                return originalInt;
        }

        //'UserEntersEnum'   ----> list of words, user may select one of the words
        protected Enum UserEntersEnum(Item item, T target)
        {
            Enum originalEnum = GetEnum<Enum>(item, target);
            Type enumType = originalEnum.GetType();

            string currentName = originalEnum.ToString();
            string[] names = Enum.GetNames(enumType);
            int index = -1;
            foreach(string name in names)
            {
                ++index;
                if (name.Equals(currentName))
                    break;
            }

            ConsoleKeyInfo key;
            do
            {
                Thread.Sleep(conf.DelayMilliSeconds);
                RenderList3(originalEnum, index);
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.PageUp || key.Key == ConsoleKey.LeftArrow)
                    index = (index == 0) ? names.Length - 1 : index - 1;
                else
                if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    index = (index == names.Length - 1) ? 0 : index + 1;
                else
                if (key.Key == ConsoleKey.Enter)
                    return (Enum) Enum.ToObject(enumType, Enum.GetValues(enumType).GetValue(index));
                else
                if (key.Key == ConsoleKey.Escape)
                    return null;
            }
            while (true);
        }

        //'UserEntersFlagsEnum' -> [ ] [x] [x] [ ] [x] [ ] (words with checkbox infront of the word)
        protected Enum UserEntersFlagsEnum(Item item, T target)
        {
            Enum originalEnum = GetEnum<Enum>(item, target);
            Type enumType = originalEnum.GetType();

            //TODO

            return null;
        }

        //user selects one of 16 colors - escape with [ESC] key
        protected ConsoleColor UserEntersColor(Item item, T target)
        {
            ConsoleColor originalColor = GetColor(item, target);

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
                Thread.Sleep(conf.DelayMilliSeconds);
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

        //user enters a culture - escape with [ESC] key
        protected CultureInfo UserEntersCulture(Item item, T target)
        {
            ConsoleKeyInfo key;
            String str = "";
            List<CultureInfo> cul = new List<CultureInfo>();
            CultureInfo[] all1 = CultureInfo.GetCultures(CultureTypes.AllCultures);
            List<CultureInfo> all = new List<CultureInfo>(all1);
            all.Sort(delegate (CultureInfo ci1, CultureInfo ci2)
            {
                return ci1.DisplayName.CompareTo(ci2.DisplayName);
            });
            int sel = -1;

            Console.CursorVisible = true;

            do
            {
                Thread.Sleep(conf.DelayMilliSeconds);

                Console.Clear(); //<--- this is problematic, because if [UpArrow] or [LeftArrow] is pressed, we see nothing...
                                 //---> the double-buffered console might solve this problem

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

                Console.ForegroundColor = conf.PromptColor;
                Console.Write(conf.PromptText);
                Console.ForegroundColor = sel < 0 ? conf.FeedbackColorError : conf.FeedbackColorOkay;
                Console.Write(str);
                Console.ForegroundColor = conf.UserColor;

                cul.Clear();
                foreach (CultureInfo ci in all)
                    if (ci.DisplayName.ToLower().StartsWith(str.ToLower()))
                        cul.Add(ci);

                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Backspace)
                {
                    str = str.Length == 0 ? str : str.Substring(0, str.Length - 1);
                    //sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                    sel = all.FindIndex((e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                }
                else
                if (key.Key == ConsoleKey.Tab)
                {
                    if (cul.Count > 0)
                    {
                        str = cul[0].DisplayName;
                        //sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                        sel = all.FindIndex((e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                    }
                }
                else
                if (key.Key == ConsoleKey.Enter && sel >= 0)
                {
                    Console.CursorVisible = false;
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
                        //if (sel >= all.Length)
                        if (sel >= all.Count)
                            sel = 0;
                        str = all[sel].DisplayName;
                    }
                }
                else
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.PageUp)
                {
                    if (sel < 0)
                    {
                        //sel = all.Length - 1;
                        sel = all.Count - 1;
                    }
                    else
                    {
                        sel--;
                        if (sel == -1)
                            //sel = all.Length - 1;
                            sel = all.Count - 1;
                        str = all[sel].DisplayName;
                    }
                }
                else
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = false;
                    return null;
                }
                else
                {
                    str += key.KeyChar;
                    //sel = Array.FindIndex(all, (e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                    sel = all.FindIndex((e) => e.DisplayName.ToLower().Equals(str.ToLower()));
                }
                Console.WriteLine();
            }
            while (true);
        }

        //you override this method to work with other targets
        protected virtual PropertyInfo GetProperty(string name) => typeof(T).GetProperty(name);

        protected void SetItem<V>(Item item, V val, object ob) => GetProperty(item.Name).SetValue(ob, val);

        protected string GetString(Item item, object ob) => (string)GetProperty(item.Name).GetValue(ob);
        protected int GetInt32(Item item, object ob) => (int)GetProperty(item.Name).GetValue(ob);
        protected decimal GetDecimal(Item item, object ob) => (decimal)GetProperty(item.Name).GetValue(ob);
        protected ConsoleColor GetColor(Item item, object ob) => (ConsoleColor)GetProperty(item.Name).GetValue(ob);
        protected CultureInfo GetCulture(Item item, object ob) => (CultureInfo)GetProperty(item.Name).GetValue(ob);
        protected bool GetBool(Item item, object ob) => (bool)GetProperty(item.Name).GetValue(ob);
        protected E GetEnum<E>(Item item, object ob) where E : Enum
        {
            return (E)GetProperty(item.Name).GetValue(ob);
        }
    }
}
