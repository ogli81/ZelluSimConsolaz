using System;
using ZelluSim.SimulationTypes;
using ZelluSimConsolaz.ConsoleCLI;
using System.Windows.Input;
using System.Windows;
using System.Threading;

namespace ZelluSimConsolaz
{
    public class ConsoleApp
    {
        protected ICellSimulation sim;
        protected CliConfig conf;
        protected string command;
        protected string feedback = "";
        protected Random rand = new Random();
        protected bool running = false;
        protected char[] sep = { ' ', '\t', ',' };

        public void Rerender()
        {
            Console.BackgroundColor = conf.BackColor;
            Console.Clear();

            for (int i = 0; i < conf.TopLeftY; ++i)
                Console.WriteLine();

            decimal val;
            for (int y = 0; y < sim.Settings.SizeY; ++y)
            {
                for (int j = 0; j < conf.TopLeftX; ++j)
                    Console.Write(" ");
                for (int x = 0; x < sim.Settings.SizeX; ++x)
                {
                    val = sim.GetCellValue(x, y);
                    if (val <= 0m)
                    {
                        Console.ForegroundColor = conf.DeadColor;
                        Console.Write(conf.DeadText);
                    }
                    else
                    if (val >= 1m)
                    {
                        Console.ForegroundColor = conf.AlifeColor;
                        Console.Write(conf.AlifeText);
                    }
                    else
                    {
                        Console.ForegroundColor = conf.HalfAlifeColor;
                        Console.Write(conf.HalfAlifeText);
                    }
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = conf.GenerationTextColor;
            //Console.WriteLine(conf.GenerationText, sim.CurrentGen.ToString(conf.GenerationTextCulture));
            Console.WriteLine(String.Format(conf.GenerationTextCulture, conf.GenerationText, sim.CurrentGen));
        }

        public void AutoSimLoop()
        {
            running = true;
            bool vis = Console.CursorVisible;
            Console.CursorVisible = false;
            do
            {
                sim.CalculateNextGen();
                Rerender();
                Console.ForegroundColor = conf.RunningColor;
                Console.WriteLine();
                Console.WriteLine(conf.RunningText);

                if(conf.DelayMilliSeconds > 0)
                    Thread.Sleep(conf.DelayMilliSeconds);
            }
            while(!Keyboard.IsKeyDown(Key.Escape));
            running = false;
            Console.CursorVisible = vis;
        }

        public void FillWith(decimal val)
        {
            for (int x = 0; x < sim.Settings.SizeX; ++x)
                for (int y = 0; y < sim.Settings.SizeY; ++y)
                    sim.SetCellValue(x, y, val);
        }

        public void FillWithRandoms()
        {
            for (int x = 0; x < sim.Settings.SizeX; ++x)
                for (int y = 0; y < sim.Settings.SizeY; ++y)
                    sim.SetCellValue(x, y, rand.Next(2) == 1 ? 1m : 0m);
        }

        public bool BoundsCheck(int x, int y)
        {
            return (x < 0 || x >= sim.Settings.SizeX || y < 0 || y >= sim.Settings.SizeY) ? false : true;
        }

        public void SetWindowSize()
        {
            int paddingX = 4;
            int paddingY = 4;
            Console.SetWindowSize(
                conf.TopLeftX + sim.Settings.SizeX + paddingX, 
                conf.TopLeftY + sim.Settings.SizeY + paddingY
                );
        }

        public void MainLoop(string[] args)
        {
            do
            {
                if (conf == null && sim == null)
                {
                    conf = new CliConfig();
                    sim = new ClassicSimulation(new SimulationSettings());
                    FillWithRandoms();
                    conf.App = this;
                    SetWindowSize();
                }

                if (conf == null)
                {
                    conf = new CliConfig();
                    conf.App = this;
                    SetWindowSize();
                }

                if (sim == null)
                {
                    sim = new ClassicSimulation(new SimulationSettings());
                    FillWithRandoms();
                    SetWindowSize();
                }

                Console.WriteLine();
                Console.ForegroundColor = conf.PromptColor;
                Console.Write(conf.PromptText);
                command = Console.ReadLine();

                if (command.StartsWith("random"))
                    FillWithRandoms();
                else
                if (command.StartsWith("run"))
                    AutoSimLoop();
                else
                if (command.StartsWith("set size"))
                {
                    string[] split = command.Split(sep);
                    if (split.Length > 3)
                    {
                        int newX, newY = -1;
                        bool success = int.TryParse(split[2], out newX);
                        success = success && int.TryParse(split[3], out newY);
                        if (success && newX > 0 && newY > 0)
                        {
                            sim.Settings.SuppressUpdates = true;
                            sim.Settings.SizeX = newX;
                            sim.Settings.SizeY = newY;
                            sim.Settings.SuppressUpdates = false;
                        }
                    }
                }
                else
                if (command.StartsWith("set"))
                {
                    int x, y = -1;
                    decimal val = 1m;
                    string[] split = command.Split(sep);
                    if (split.Length > 2)
                    {
                        bool success = int.TryParse(split[1], out x);
                        success = success && int.TryParse(split[2], out y);
                        if (split.Length > 3)
                            success = decimal.TryParse(split[3], out val);
                        if (success && BoundsCheck(x,y))
                            sim.SetCellValue(x, y, val);
                    }
                }
                else
                if (command.Equals("fill"))
                {
                    FillWith(1m);
                }
                else
                if (command.Equals("clear"))
                {
                    FillWith(0m);
                }
                else
                if (command.StartsWith("clear"))
                {
                    int x, y = -1;
                    string[] split = command.Split(sep);
                    if (split.Length > 2)
                    {
                        bool success = int.TryParse(split[1], out x);
                        success = success && int.TryParse(split[2], out y);
                        if (success && BoundsCheck(x, y))
                            sim.SetCellValue(x, y, 0m);
                    }
                }
                else
                if (command.Equals("next"))
                {
                    sim.CalculateNextGen();
                }
                else
                if (command.Equals("oldest?"))
                {
                    feedback = "" + sim.OldestGen;
                }
                else
                if (command.Equals("oldest"))
                {
                    sim.GoToOldestGen();
                }
                else
                if (command.Equals("back"))
                {
                    sim.GoBackOneGen();
                }
                else
                if (command.Equals("zero"))
                {
                    sim.RelabelCurrentAsZero();
                }
                else
                if (command.StartsWith("go to"))
                {
                    string[] split = command.Split(sep);
                    if (split.Length > 2)
                    {
                        int n;
                        bool success = int.TryParse(split[2], out n);
                        if (success && n >= 0)
                        {
                            sim.GoToGen(n);
                        }
                    }
                }

                Rerender();
            }
            while (!command.Equals("exit"));
        }
    }
}
