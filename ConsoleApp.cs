using System;
using ZelluSim.SimulationTypes;
using ZelluSimConsolaz.ConsoleCLI;
using System.Windows.Input;
using System.Windows;

namespace ZelluSimConsolaz
{
    public class ConsoleApp
    {
        protected ICellSimulation sim;
        protected CliConfig conf;
        protected string command;
        protected Random rand = new Random();
        protected bool running = false;
        protected char[] sep = { ' ', '\t', ',' };

        public void Rerender()
        {
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
            }
        }

        public void AutoSimLoop()
        {
            running = true;
            do
            {
                sim.CalculateNextGen();
                Rerender();
                Console.ForegroundColor = conf.RunningColor;
                Console.WriteLine();
                Console.WriteLine(conf.RunningText);
            }
            while(!Keyboard.IsKeyDown(Key.Escape));
            running = false;
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

        public void MainLoop(string[] args)
        {
            do
            {
                if (sim == null)
                {
                    sim = new ClassicSimulation(new SimulationSettings());
                    FillWithRandoms();
                }

                Console.WriteLine();
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
                        if (success)
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
                    bool success = int.TryParse(split[1], out x);
                    success = success && int.TryParse(split[2], out y);
                    if (split.Length > 3)
                        success = decimal.TryParse(split[3], out val);
                    if (success)
                        sim.SetCellValue(x, y, val);
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
                    bool success = int.TryParse(split[2], out x);
                    success = success && int.TryParse(split[3], out y);
                    if (success)
                        sim.SetCellValue(x, y, 0m);
                }

                Rerender();
            }
            while (!command.Equals("exit"));
        }
    }
}
