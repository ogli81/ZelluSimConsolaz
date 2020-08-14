using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using ZelluSim.Misc;
using ZelluSimConsolaz.AsciiArtZoom;
using ZelluSimConsolaz.ConsoleCLI;
using ZelluSimConsolaz.MapperFunction;

namespace ZelluSimConsolaz
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleApp app = new ConsoleApp();
            app.MainLoop(args);
        }
    }
}
