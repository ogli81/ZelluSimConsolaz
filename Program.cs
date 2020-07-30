using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZelluSim;

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
