using System;
using ZelluSim.SimulationTypes;
using ZelluSimConsolaz.ConsoleCLI;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using ZelluSim.Misc;

namespace ZelluSimConsolaz
{
    /// <summary>
    /// The main class of the standard CLI for ZelluSim. Since we want to display 
    /// 2D information (the cell field) a CLI is probably not the best option 
    /// to work with ZelluSim. However, we use a simple clear-and-rerender-paradigm
    /// for many parts of this implementation and simple text input for some
    /// parts.
    /// <br></br>
    /// This works quite well, except for some "flickering" issues. There are some
    /// awesome attempts by others to add double-buffering to a C# console. When 
    /// we have some time, we may experiment with their code.
    /// </summary>
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
        protected static int paddingX = 1;
        protected static int paddingY = 4;


        //c'tors:

        //-


        //public methods:

        public void MainLoop(string[] args)
        {
            Console.SetBufferSize(2*Console.LargestWindowWidth, 10*Console.LargestWindowHeight);
            //not really sure about this line (we may still need double buffering and/or custom-flush)

            do
            {
                if (conf == null && sim == null)
                {
                    conf = CreateCliConfig();
                    sim = CreateCellSimulation();
                    FillWithRandoms(rand);
                    conf.App = this;
                    SetWindowSize();
                    Rerender();
                }

                if (conf == null)
                {
                    conf = CreateCliConfig();
                    conf.App = this;
                    SetWindowSize();
                    Rerender();
                }

                if (sim == null)
                {
                    sim = CreateCellSimulation();
                    FillWithRandoms(rand);
                    SetWindowSize();
                    Rerender();
                }

                //Console.WriteLine();
                Console.ForegroundColor = conf.PromptColor;
                Console.Write(conf.PromptText);
                Console.ForegroundColor = conf.UserColor;
                Console.CursorVisible = true;
                command = Console.ReadLine();
                Console.CursorVisible = false;

                if (command.Equals("default"))
                {
                    conf = null;
                    sim = null;
                }
                else
                if (command.Equals("conf"))
                {
                    conf.SuppressRebuildReformat = true; //we may have several changes, will react later
                    ItemsChanger<CliConfig> changer = new ItemsChanger<CliConfig>(conf, conf);
                    changer.MainLoop();
                    SetWindowSize();
                    confStr1 = null; //AlifeText may have changed
                    confStr2 = null; //HalfAlifeText may have changed
                    confStr3 = null; //DeadText may have changed
                    conf.SuppressRebuildReformat = false; //now let the console app react to all changes
                    feedback = "Left configuration management.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("settings"))
                {
                    sim.Settings.SuppressUpdates = true; //we may have several changes, will react later
                    ItemsChanger<SimulationSettings> changer = new ItemsChanger<SimulationSettings>(sim.Settings, conf);
                    changer.MainLoop();
                    SetWindowSize();
                    sim.Settings.SuppressUpdates = false; //now let the sim react to all our changes
                    feedback = "Left settings management.";
                    feedbackType = FeedbackType.OKAY;
                }
                else
                if (command.Equals("sim"))
                {
                    //TODO
                    //display a list with all our simulation types
                    //let user start a new simulation of any of those types
                }
                else
                if (command.StartsWith("save conf"))
                {
                    //TODO
                }
                else
                if (command.StartsWith("save sim"))
                {
                    //TODO
                }
                else
                if (command.StartsWith("load conf"))
                {
                    //TODO
                }
                else
                if (command.StartsWith("load sim"))
                {
                    //TODO
                }
                else
                if (command.StartsWith("random"))
                {
                    string[] split = command.Split(sep);
                    Random r;
                    bool success;
                    int seed;
                    if (split.Length < 2)
                    {
                        r = rand;
                        success = true;
                    }
                    else
                    {
                        success = int.TryParse(split[1], out seed);
                        r = new Random(seed);
                    }
                    if (success)
                    {
                        FillWithRandoms(r);
                        feedback = "Filled cells with random numbers.";
                        feedbackType = FeedbackType.OKAY;
                    }
                    else
                    {
                        feedback = "Could not parse seed value: " + split[1];
                        feedbackType = FeedbackType.ERROR;
                    }
                }
                else
                if (command.Equals("run"))
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
                    if (split.Length > 2)
                    {
                        int newX, newY = -1;
                        bool success = int.TryParse(split[2], out newX);
                        if(split.Length > 3)
                            success = success && int.TryParse(split[3], out newY);
                        if (success && newX > 0 && split.Length < 4 || newY > 0)
                        {
                            sim.Settings.SuppressUpdates = true;
                            sim.Settings.SizeX = newX;
                            sim.Settings.SizeY = split.Length > 3 ? newY : newX;
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
                if (command.StartsWith("set param"))
                {
                    string[] split = command.Split(sep);
                    if(split.Length > 3)
                    {
                        int n = -1;
                        decimal v = -1m;
                        bool success = int.TryParse(split[2], out n);
                        success = success && decimal.TryParse(split[3], out v);
                        if(success)
                        {
                            SimulationParameter param = null;

                            if (n == 1) param = sim.Param1;
                            if (n == 2) param = sim.Param2;
                            //TODO: param = sim.GetParam(n);
                            //TODO: int maxN = sim.GetNumParams();

                            if(param != null)
                            {
                                if (v >= param.Min && v <= param.Max)
                                {
                                    param.Current = v;
                                    feedback = $"Set param {n} to value {v}.";
                                    feedbackType = FeedbackType.OKAY;
                                }
                                else
                                {
                                    feedback = $"Value {v} not in allowed interval [{param.Min}..{param.Max}]!";
                                    feedbackType = FeedbackType.ERROR;
                                }
                            }
                            else
                            {
                                feedback = $"Unknown param [{n}] (try a different index!).";
                                feedbackType = FeedbackType.ERROR;
                            }
                        }
                        else
                        {
                            feedback = "Could not set param (incorrect number?).";
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
                if (command.Equals("info"))
                {
                    ShowSimInfo();
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
            if (sim == null || conf == null)
                return;

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
                        Console.Write(DeadText);
                    }
                    else
                    if (val >= 1m)
                    {
                        Console.ForegroundColor = conf.AlifeColor;
                        Console.Write(AlifeText);
                    }
                    else
                    {
                        Console.ForegroundColor = conf.HalfAlifeColor;
                        Console.Write(HalfAlifeText);
                    }
                }
                Console.WriteLine();
            }

            //Console.BackgroundColor = conf.BackColor; (flickers too much => need double buffering / batch-flush)
            //---> https://github.com/microsoftarchive/msdn-code-gallery-community-a-c/tree/master/C%23%20Console%20Double%20Buffer

            Console.ForegroundColor = conf.GenerationTextColor;
            //Console.WriteLine(conf.GenerationText, sim.CurrentGen.ToString(conf.GenerationTextCulture));
            Console.WriteLine(string.Format(conf.GenerationTextCulture, conf.GenerationText, sim.CurrentGen));

            //render a line and a number like this:
            //[avg = 0.872] (0|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||      1)
            //TODO: let user edit the character - ItemsChanger.UserEntersCharacter
            string str1 = "[avg = {0:0.000}] (0";
            string str3 = "1)";
            string str2 = "";
            decimal avg = sim.GetAverageCellValue();
            str1 = string.Format(conf.GenerationTextCulture, str1, avg);
            int len = Console.WindowWidth - str1.Length - str3.Length - 1;
            int num = (int)(avg * ((decimal)len));
            str2 = str2.PadRight(num, '|');
            str2 = str2.PadRight(len, ' ');
            Console.ForegroundColor = conf.InfoColor;
            Console.WriteLine(str1 + str2 + str3);

            switch (feedbackType)
            {
                case FeedbackType.OKAY:  Console.ForegroundColor = conf.FeedbackColorOkay; break;
                case FeedbackType.ERROR: Console.ForegroundColor = conf.FeedbackColorError; break;
            }
            Console.WriteLine(feedback);
        }


        //helper methods:

        protected void AutoSimLoop()
        {
            running = true;
            bool vis = Console.CursorVisible;
            Console.CursorVisible = false;
            do
            {
                sim.CalculateNextGen();
                Rerender();
                Console.ForegroundColor = conf.RunningColor;
                //Console.WriteLine();
                Console.Write(conf.RunningText);

                if(conf.DelayMilliSeconds > 0)
                    Thread.Sleep(conf.DelayMilliSeconds);
            }
            while(!Keyboard.IsKeyDown(Key.Escape));
            running = false;
            Console.CursorVisible = vis;
        }

        protected void ShowHelp()
        {
            Console.Clear();
            Console.ForegroundColor = conf.HelpColor;

            int i = 1;
            Console.WriteLine("List of available commands:"); i++;
            Console.WriteLine(); i++;
            Console.WriteLine("'help' or '?' - show available commands."); i++;
            Console.WriteLine("'info - get information page for simulation."); i++;//TODO
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
            Console.WriteLine("'random [s]' - fill with random (s = seed)."); i++;
            Console.WriteLine("'conf' - change ui settings."); i++;
            Console.WriteLine("'settings' - change simulation settings."); i++;
            Console.WriteLine("'sim' - select/configure type of simulation."); i++;//TODO
            Console.WriteLine("'save conf [filename]' - save ui settings."); i++;//TODO
            Console.WriteLine("'save sim [filename]' - save sim+settings."); i++;//TODO
            Console.WriteLine("'load conf [filename]' - load ui settings."); i++;//TODO
            Console.WriteLine("'load sim [filename]' - load sim+settings."); i++;//TODO
            Console.WriteLine("'default' - default settings and simulation."); i++;
            Console.WriteLine("'set param [n] [v] - set nth param to value."); i++;//TODO
            Console.WriteLine(); i++;
            Console.WindowHeight = i + 2;
            Console.WindowWidth = 60;
            Console.Write("Press any key to continue...");
            Console.ReadKey();
            SetWindowSize();
        }

        protected void ShowSimInfo()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.WindowHeight);
            Console.Clear();
            Console.ForegroundColor = conf.HelpColor;
            
            Console.WriteLine($"Information about the simulation of type {sim.GetType()}:");
            
            Console.WriteLine("Info:");
            Console.WriteLine(sim.Info);

            Console.WriteLine("Param1:");
            if (sim.Param1 == null)
                Console.WriteLine("(N/A)");
            else
            {
                Console.WriteLine(sim.Param1.Name);
                Console.WriteLine(sim.Param1.Info);
                Console.WriteLine($"[{sim.Param1.Min}..{sim.Param1.Max}] (currently at {sim.Param1.Current})");
            }

            Console.WriteLine("Param2:");
            if (sim.Param1 == null)
                Console.WriteLine("(N/A)");
            else
            {
                Console.WriteLine(sim.Param2.Name);
                Console.WriteLine(sim.Param2.Info);
                Console.WriteLine($"[{sim.Param2.Min}..{sim.Param2.Max}] (currently at {sim.Param2.Current})");
            }

            Console.WriteLine("Generations:");
            Console.WriteLine($"Number of generations in memory: {sim.NumGens}");
            Console.WriteLine($"Number-label of the current gen: {sim.CurrentGen}");
            Console.WriteLine($"Number-label of the oldest gen:  {sim.OldestGen}");

            Console.WriteLine("Geometry:");
            Console.WriteLine($"{sim.Settings.SizeX}x{sim.Settings.SizeY} cells");
            Console.WriteLine("The world "+(sim.Settings.IsWrap ? "does wrap" : "doesn't wrap")+" at the outer borders.");

            Console.WriteLine("Life sum:");
            Console.WriteLine("sum = " + sim.GetSumOfCellValues());
            Console.WriteLine("average = " + sim.GetAverageCellValue());
            Console.WriteLine("median = " + sim.GetMedianCellValue());

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
            SetWindowSize();
        }

        protected void FillWith(decimal val)
        {
            for (int x = 0; x < sim.Settings.SizeX; ++x)
                for (int y = 0; y < sim.Settings.SizeY; ++y)
                    sim.SetCellValue(x, y, val);
        }

        protected void FillWithRandoms(Random r)
        {
            for (int x = 0; x < sim.Settings.SizeX; ++x)
                for (int y = 0; y < sim.Settings.SizeY; ++y)
                    sim.SetCellValue(x, y, r.Next(2) == 1 ? 1m : 0m);
        }

        protected bool BoundsCheck(int x, int y)
        {
            return x >= 0 && x < sim.Settings.SizeX && y >= 0 && y < sim.Settings.SizeY;
        }

        protected void SetWindowSize()
        {
            //TODO:
            // we have a problem here - the console window can only support 240 x 63 characters (on this machine)
            // if we set higher values, e.g. for the height of the window, an exception will be thrown :-(
            // there are ways to make the font smaller with 'unsafe' code:
            //  https://stackoverflow.com/questions/47014258/c-sharp-modify-console-font-font-size-at-runtime
            // but maybe we should just prompt the user that a certain maximum can't be exceeded:
            //  - when editing sim.Settings via the ItemChanger (add parameter 'maxSizeXY', a (int,int) tupel
            //  - when trying to load a .zsim file (write error-feedback like 'max window size exceeded')
            // our newest idea to circumvent this problem:
            //  - render a "zoomed view", using ASCII graphics and a mapping function

            int width = conf.TopLeftX + sim.Settings.SizeX + paddingX;
            int height = conf.TopLeftY + sim.Settings.SizeY + paddingY;

            //adjust width so that most of our feedback messages fit well into the window:
            width = Math.Max(width, 64);

            Console.SetWindowSize(width, height);
        }

        protected int CellTextLength
        {
            get
            {
                int len = 0;
                len = Math.Max(conf.AlifeText.Length, len);
                len = Math.Max(conf.DeadText.Length, len);
                len = Math.Max(conf.HalfAlifeText.Length, len);
                len = Math.Max(len, 1);
                len = Math.Min(len, 8);
                return len;
            }
        }

        protected string confStr1 = null;
        protected string confStr2 = null;
        protected string confStr3 = null;

        protected string adjuStr1 = null;
        protected string adjuStr2 = null;
        protected string adjuStr3 = null;

        protected string EnsureString(in string what, ref string confStr, ref string adjuStr)
        {
            if (what.Equals(confStr))
                return adjuStr;
            int len = CellTextLength;
            confStr = what;
            if (confStr.Length < len)
                adjuStr = confStr.PadRight(len, ' ');
            else
            if (what.Length >= len) //may have more than 8 characters
                adjuStr = confStr.Substring(0, len);
            return adjuStr;
        }

        protected string AlifeText => EnsureString(conf.AlifeText, ref confStr1, ref adjuStr1);
        protected string HalfAlifeText => EnsureString(conf.HalfAlifeText, ref confStr2, ref adjuStr2);
        protected string DeadText => EnsureString(conf.DeadText, ref confStr3, ref adjuStr3);

        //protected string AlifeText
        //{
        //    get
        //    {
        //        if (conf.AlifeText.Equals(confStr1))
        //            return adjuStr1;
        //        int len = CellTextLength;
        //        if (conf.AlifeText.Length < len)
        //            adjuStr1 = conf.AlifeText.PadRight(len - conf.AlifeText.Length, ' ');
        //        else
        //        if (conf.AlifeText.Length >= len) //may have more than 8 characters
        //            adjuStr1 = conf.AlifeText.Substring(0, len);
        //        confStr1 = conf.AlifeText;
        //        return adjuStr1;
        //    }
        //}

        //TODO: render a zoomed version if simulation is bigger
        //TODO: how can we do this???
        //idea: https://www.google.com/search?q=C%23+convert+image+to+ascii+art
        //      - https://www.codeproject.com/Articles/20435/Using-C-To-Generate-ASCII-Art-From-An-Image
        //      - https://www.c-sharpcorner.com/article/generating-ascii-art-from-an-image-using-C-Sharp/
        protected (int x, int y) GetMaxSimSize()
        {
            (int x, int y) max = ( Console.LargestWindowWidth, Console.LargestWindowHeight );
            max = ( max.x - paddingX - conf.TopLeftX, max.y - paddingY - conf.TopLeftY );
            max.x /= CellTextLength;
            return max;
        }
        //this doesn't make much sense:
        //protected (int x, int y) GetMaxConfSize()
        //{
        //    (int x, int y) max = ( Console.LargestWindowWidth, Console.LargestWindowHeight );
        //    max = ( max.x - paddingX - sim.Settings.SizeX, max.y - paddingY - sim.Settings.SizeY );
        //    return max;
        //}

        protected CliConfig CreateCliConfig()
        {
            CliConfig config = new CliConfig();
            config.DelayMilliSeconds = 20;
            return config;
        }

        protected ICellSimulation CreateCellSimulation()
        {
            //sim = new ClassicSimulation(new SimulationSettings());

            SimulationSettings simSettings = new SimulationSettings();
            //simSettings.MemSlots = 4;
            simSettings.MemSlotsGrow = 2;
            //simSettings.MemSlotsMax = int.MaxValue; //crazy setting! :-)
            simSettings.MemSlotsMax = 10000;
            sim = new ClassicSimulation(simSettings);

            return sim;
        }
    }
}
