using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZelluSimConsolaz.ConsoleCLI
{
    public class ColorInfo
    {
        public static int NumColors => 16;
        public static ConsoleColor GetColor(int index)
        {
            switch(index)
            {
                case 0: return ConsoleColor.Black;
                case 1: return ConsoleColor.DarkBlue; 
                case 2: return ConsoleColor.DarkGreen;
                case 3: return ConsoleColor.DarkCyan;
                case 4: return ConsoleColor.DarkRed;
                case 5: return ConsoleColor.DarkMagenta;
                case 6: return ConsoleColor.DarkYellow; 
                case 7: return ConsoleColor.Gray; 
                case 8: return ConsoleColor.DarkGray;
                case 9: return ConsoleColor.Blue;
                case 10: return ConsoleColor.Green;
                case 11: return ConsoleColor.Cyan;
                case 12: return ConsoleColor.Red;
                case 13: return ConsoleColor.Magenta;
                case 14: return ConsoleColor.Yellow;
                case 15: return ConsoleColor.White;
            }
            throw new ArgumentException("Only values between 0 (inclusive) and 15 (inclusive)!");
        }

        public static string GetColorName(int index)
        {
            switch(index)
            {
                case 0: return "black";
                case 1: return "dark blue";
                case 2: return "dark green";
                case 3: return "dark cyan";
                case 4: return "dark red";
                case 5: return "dark magenta";
                case 6: return "dark yellow";
                case 7: return "gray";
                case 8: return "dark gray";
                case 9: return "blue";
                case 10: return "green";
                case 11: return "cyan";
                case 12: return "red";
                case 13: return "magenta";
                case 14: return "yellow";
                case 15: return "white";
            }
            throw new ArgumentException("Only values between 0 (inclusive) and 15 (inclusive)!");
        }

        public static bool IsDarkColor(int index)
        {
            switch (index)
            {
                case 0: return true;
                case 1: return true;
                case 2: return true;
                case 3: return true;
                case 4: return true;
                case 5: return true;
                case 6: return true;
                case 7: return false;
                case 8: return true;
                case 9: return false;
                case 10: return false;
                case 11: return false;
                case 12: return false;
                case 13: return false;
                case 14: return false;
                case 15: return false;
            }
            throw new ArgumentException("Only values between 0 (inclusive) and 15 (inclusive)!");
        }
    }
}
