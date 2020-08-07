using System;

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
