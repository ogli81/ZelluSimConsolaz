using System;
using ZelluSim.SimulationTypes;
using ZelluSimConsolaz.ConsoleCLI;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using ZelluSim.Misc;

namespace ZelluSimConsolaz
{
    public enum FeedbackType
    {
        OKAY,
        ERROR
    }

    public class ConsoleApp : IInterruptRequester
    {
        //state:

        protected ICellSimulation sim;
        protected CliConfig conf;
        protected string command;
        protected string feedback = "";
        protected FeedbackType feedbackType = FeedbackType.OKAY;
        protected Random rand = new Random();
        protected bool running = false;
        protected char[] sep = { ' ', '\t', ',' };


        //c'tors:

        //-


        //public methods:

        public void MainLoop(string[] args)
        {
            do
            {
                if (conf == null && sim == null)
                {
                    conf = CreateCliConfig();
                    sim = CreateCellSimulation();
                    FillWithRandoms();
                    conf.App = this;
                    SetWindowSize();
                }

                if (conf == null)
                {
                    conf = CreateCliConfig();
                    conf.App = this;
                    SetWindowSize();
                }

                if (sim == null)
                {
                    sim = CreateCellSimulation();
                    FillWithRandoms();
                    SetWindowSize();
                }

                Console.WriteLine();
                Console.ForegroundColor = conf.PromptColor;
                Console.Write(conf.PromptText);
                command = Console.ReadLine();

                if (command.StartsWith("conf"))
                {
                    ConfigChanger changer = new ConfigChanger(conf);
                    changer.MainLoop();
                    SetWindowSize();
                }
                else
                if (command.StartsWith("settings"))
                {
                    //TODO
                    //change settings of the simulation (e.g. memory slots or size of the cell field)
                }
                else
                if (command.StartsWith("sim"))
                {
                    //TODO
                    //display a list with all our simulation types
                }
                else
                if (command.StartsWith("save"))
                {
                    //TODO
                    //idea: let user select: ui-settings/sim-settings/sim(with sim-settings, optionally with ui-settings too)
                }
                else
                if (command.StartsWith("load"))
                {
                    //TODO
                    //idea: let user select: ui-settings/sim-settings/sim(with sim-settings and ui-settings, if available)
                }
                else
                if (command.StartsWith("random"))
                {
                    FillWithRandoms();
                    feedback = "Filled cells with random numbers.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.StartsWith("run"))
                {
                    Rerender();
                    feedback = "Started automatic run of simulations.";
                    feedbackType = FeedbackType.OKAY;
                    AutoSimLoop();
                    feedback = "Completed automatic run of simulations.";
                    feedbackType = FeedbackType.OKAY;
                }
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
                            feedback = "Set x and y to desired values.";
                            feedbackType = FeedbackType.OKAY;
                        }
                        else
                        {
                            feedback = "Could not set to new size.";
                            feedbackType = FeedbackType.ERROR;
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
                        if (success && BoundsCheck(x, y))
                        {
                            sim.SetCellValue(x, y, val);
                            feedback = "Set value for cell.";
                            feedbackType = FeedbackType.OKAY;
                        }
                        else
                        {
                            feedback = "Could not set to new value.";
                            feedbackType = FeedbackType.ERROR;
                        }
                    }
                }
                else
                if (command.Equals("fill"))
                {
                    FillWith(1m);
                    feedback = "Set every cell to 1 (100%).";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("clear"))
                {
                    FillWith(0m);
                    feedback = "Set every cell to 0 (0%).";
                    feedbackType = FeedbackType.OKAY;
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
                        {
                            sim.SetCellValue(x, y, 0m);
                            feedback = "Set cell value to 0 (0%).";
                            feedbackType = FeedbackType.OKAY;
                        }
                        else
                        {
                            feedback = "Could not set cell value to 0 (0%).";
                            feedbackType = FeedbackType.ERROR;
                        }
                    }
                }
                else
                if (command.Equals("next"))
                {
                    sim.CalculateNextGen();
                    feedback = "Calculated next generation.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("current?"))
                {
                    feedback = "Current generation is " + sim.CurrentGen + ".";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("oldest?"))
                {
                    feedback = "Oldest generation is " + sim.OldestGen + ".";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("num?"))
                {
                    feedback = "Number of generations: " + sim.NumGens + ".";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("oldest"))
                {
                    sim.GoToOldestGen();
                    feedback = "Went to oldest generation.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("back"))
                {
                    sim.GoBackOneGen();
                    feedback = "Went back one generation.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("zero"))
                {
                    sim.RelabelCurrentAsZero();
                    feedback = "Relabled current generation as zero, deleted others.";
                    feedbackType = FeedbackType.OKAY;
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
                            bool okay = sim.GoToGen(n, this);
                            if (okay)
                            {
                                feedback = "Went to generation " + n + ".";
                                feedbackType = FeedbackType.OKAY;
                            }
                            else
                            {
                                feedback = "Tried to go to generation " + n + " but failed.";
                                feedbackType = FeedbackType.ERROR;
                            }
                        }
                        else
                        {
                            feedback = "Could not go to that generation.";
                            feedbackType = FeedbackType.ERROR;
                        }
                    }
                }
                else
                if (command.Equals("help") || command.Equals("?"))
                {
                    ShowHelp();
                }
                else
                {
                    feedback = "Unknown command: \"" + command + "\"";
                    feedbackType = FeedbackType.ERROR;
                }

                Rerender();
            }
            while (!command.Equals("exit"));
        }

        public bool RequestingInterrupt() => (Keyboard.IsKeyDown(Key.Escape));

        public void Rerender()
        {
            Console.BackgroundColor = conf.BackColor;
            Console.Clear();

            for (int i = 0; i < conf.TopLeftY; ++i)
                Console.WriteLine();

            decimal val;
            for (int y = 0; y < sim.Settings.SizeY; ++y)
            {
                //Console.BackgroundColor = conf.BackColor; (flickers too much => need double buffering / batch-flush)
                //---> https://github.com/microsoftarchive/msdn-code-gallery-community-a-c/tree/master/C%23%20Console%20Double%20Buffer

                for (int j = 0; j < conf.TopLeftX; ++j)
                    Console.Write(" ");

                //Console.BackgroundColor = ConsoleColor.Yellow; (flickers too much => need double buffering / batch-flush)
                //---> https://github.com/microsoftarchive/msdn-code-gallery-community-a-c/tree/master/C%23%20Console%20Double%20Buffer

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

            //Console.BackgroundColor = conf.BackColor; (flickers too much => need double buffering / batch-flush)
            //---> https://github.com/microsoftarchive/msdn-code-gallery-community-a-c/tree/master/C%23%20Console%20Double%20Buffer

            Console.ForegroundColor = conf.GenerationTextColor;
            //Console.WriteLine(conf.GenerationText, sim.CurrentGen.ToString(conf.GenerationTextCulture));
            Console.WriteLine(string.Format(conf.GenerationTextCulture, conf.GenerationText, sim.CurrentGen));

            switch (feedbackType)
            {
                case FeedbackType.OKAY:  Console.ForegroundColor = conf.FeedbackColorOkay; break;
                case FeedbackType.ERROR: Console.ForegroundColor = conf.FeedbackColorError; break;
            }
            Console.WriteLine(feedback);
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

        public void ShowHelp()
        {
            Console.Clear();
            Console.ForegroundColor = conf.HelpColor;
            int i = 1;
            Console.WriteLine("List of available commands:"); i++;
            Console.WriteLine(); i++;
            Console.WriteLine("'help' or '?' - show available commands."); i++;
            Console.WriteLine("'exit' - close the console, end program."); i++;
            Console.WriteLine("'go to [n]' - go back or forward, until n."); i++;
            Console.WriteLine("'zero' - label current gen as 0th gen."); i++;
            Console.WriteLine("'back' - go back one generation."); i++;
            Console.WriteLine("'oldest' - go back to the oldest gen."); i++;
            Console.WriteLine("'num?' - how many generations are there?"); i++;
            Console.WriteLine("'oldest?' - what's the oldest gen?"); i++;
            Console.WriteLine("'current?' - what's the current gen?"); i++;
            Console.WriteLine("'next' - go to the next gen."); i++;
            Console.WriteLine("'clear' - set every cell to lowest value."); i++;
            Console.WriteLine("'clear [x] [y]' - set cell to lowest value."); i++;
            Console.WriteLine("'fill' - set every cell to highest value."); i++;
            Console.WriteLine("'set size [w] [h]' - resize width/height."); i++;
            Console.WriteLine("'set [x] [y] - set cell to highest value."); i++;
            Console.WriteLine("'set [x] [y] [v] - set cell to value."); i++;
            Console.WriteLine("'run' - start auto-compute (end: [ESC])."); i++;
            Console.WriteLine("'random' - fill every cell with random."); i++;
            Console.WriteLine("'conf' - change ui settings."); i++;
            Console.WriteLine("'settings' - change simulation settings."); i++;
            Console.WriteLine("'sim' - select different type of simulation."); i++;
            Console.WriteLine("'save' - save settings and/or simulation."); i++;
            Console.WriteLine("'load' - load settings and/or simulation."); i++;
            Console.WriteLine(); i++;
            Console.WindowHeight = i + 2;
            Console.WindowWidth = 60;
            Console.Write("Press any key to continue...");
            Console.ReadKey();
            SetWindowSize();
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
            return x >= 0 && x < sim.Settings.SizeX && y >= 0 && y < sim.Settings.SizeY;
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

        public CliConfig CreateCliConfig()
        {
            CliConfig config = new CliConfig();
            config.DelayMilliSeconds = 20;
            return config;
        }

        public ICellSimulation CreateCellSimulation()
        {
            //sim = new ClassicSimulation(new SimulationSettings());

            SimulationSettings simSettings = new SimulationSettings();
            //simSettings.MemSlots = 4;
            simSettings.MemSlotsGrow = 2;
            simSettings.MemSlotsMax = int.MaxValue;
            sim = new ClassicSimulation(simSettings);

            return sim;
        }
    }
}
