using System;
using System.Collections.Generic;
using System.Globalization;
using ZelluSim.Misc;
using ZelluSimConsolaz.AsciiArtZoom;
using ZelluSimConsolaz.MapperFunction;

namespace ZelluSimConsolaz.ConsoleCLI
{
    //TODO: Make class Serializable and do test-save and test-load into a .zsimc file.

    /// <summary>
    /// The configuration of the GUI is defined in this class. 
    /// Any changes to this configuration will either result in a 'RebuildGui' or 'ReformatGui' call on the Form1 form. 
    /// 'RebuildGui' will be necessary when the fundamental GUI elements have changed in size, position or quantity. 
    /// 'ReformatGui' will be enough in situations where only colors or texts have changed.
    /// </summary>
    public class CliConfig : IHasItems
    {
        //state:

        protected bool reformatNeeded = false;
        protected bool rebuildNeeded = false;
        protected bool suppress = false;

        protected ConsoleApp app = null;

        protected int topLeftX = 3;
        protected int topLeftY = 2;

        protected string alifeText = "><"; //best with 2 characters (to minimize vertical stretch)
        protected string deadText = ".."; //best with 2 characters (to minimize vertical stretch)
        protected string halfAlifeText = "~-"; //best with 2 characters (to minimize vertical stretch)
        protected ConsoleColor alifeColor = ConsoleColor.Green;
        protected ConsoleColor deadColor = ConsoleColor.Red;
        protected ConsoleColor halfAlifeColor = ConsoleColor.Yellow;
        //protected bool interpolateColors = true; //can't be done with Console colors

        protected string runningText = "RUNNING...";
        protected string stoppedText = "STOPPED.";
        protected ConsoleColor runningColor = ConsoleColor.DarkGreen;
        protected ConsoleColor stoppedColor = ConsoleColor.DarkRed;
        protected int delayMilliSeconds = 50;
        protected ConsoleColor feedbackColorOkay = ConsoleColor.Green;
        protected ConsoleColor feedbackColorError = ConsoleColor.Red;

        //protected string generationText = "generation: {0,0.0}";
        protected string generationText = "generation: {0:#,0}";
        protected CultureInfo generationTextCulture = CultureInfo.InvariantCulture;
        protected ConsoleColor generationTextColor = ConsoleColor.Magenta;
        protected string averageBarText = "avg = {0:0.000}";
        protected ConsoleColor averageBarColor = ConsoleColor.Yellow;

        protected string promptText = "> ";
        protected ConsoleColor promptColor = ConsoleColor.White;
        protected ConsoleColor userColor = ConsoleColor.Yellow;

        protected ConsoleColor helpColor = ConsoleColor.Yellow;
        protected ConsoleColor infoColor = ConsoleColor.Gray;

        protected ConsoleColor backColor = ConsoleColor.Black;
        //protected ConsoleColor helpBackColor = ConsoleColor.DarkBlue; //was an idea: help is yellow on dark blue

        protected TestEnum flagsEnum = TestEnum.FirstBlood | TestEnum.FifthElement;

        protected NamedObjects<IDecimalMapper> mappers;
        protected NamedObjects<IAsciiArtScale> scales;

        protected List<Item> items = new List<Item>();


        //c'tors:

        /// <summary>
        /// The GuiConfig is not connected (the App is null). As soon as you set the 
        /// app (<see cref="CliConfig.App"/>) the GuiConfig will try to connect with 
        /// the app.
        /// </summary>
        public CliConfig()
        {
            //mappers
            mappers = new NamedObjects<IDecimalMapper>(new string[] { "linear", "lin"}, new LinearMapper());
            mappers.Register(new string[] { "logarithmic", "log" }, new LogarithmicMapper());
            mappers.Register(new string[] { "sqrt", "root" }, new SqrtMapper());
            mappers.Register(new string[] { "custom", "points" }, new CustomMapper(new decimal[] {  0m, 0.1m, 0.25m, 0.45m, 0.75m, 1m }));

            //scales
            ThresholdScale m1 = new ThresholdScale(
                new byte[] { 230, 200, 180, 160, 130, 100, 70, 50 },
                new char[] { ' ', '.', '*', ':', 'o', '&', '8', '#', '@' },
                new Uri("https://www.codeproject.com/Articles/20435/Using-C-To-Generate-ASCII-Art-From-An-Image")
            );
            ThresholdScale m2 = new ThresholdScale(
                new decimal[] { 0.90197m, 0.8m, 0.70197m, 0.6m, 0.50197m, 0.4m, 0.30197m, 0.2m, 0.10197m },
                new char[] { ' ', '.', '-', ':', '*', '+', '=', '%', '@', '#' },
                new Uri("https://www.c-sharpcorner.com/article/generating-ascii-art-from-an-image-using-C-Sharp/")
            );
            AutomaticScale m3 = new AutomaticScale(
                "#@%=+*:-. ",
                new Uri("https://www.c-sharpcorner.com/article/generating-ascii-art-from-an-image-using-C-Sharp/")
            );
            scales = new NamedObjects<IAsciiArtScale>(new string[] { "codeproject", "default", "standard" }, m1);
            scales.Register(new string[] { "c-sharpcorner", "alternative" }, m2);
            scales.Register(new string[] { "auto-example", "auto", "automatic" }, m3);

            //items
            items.Add(new Item("TopLeftX", "top left corner, x-coordinate"));
            items.Add(new Item("TopLeftY", "top left corner, y-coordinate"));
            items.Add(new Item("AlifeText", "typically only 1 character like 'X'"));
            items.Add(new Item("DeadText", "typically only 1 character like '-'"));
            items.Add(new Item("HalfAlifeText", "typically only 1 character like '~'"));
            items.Add(new Item("AlifeColor", "color for the character 'X'"));
            items.Add(new Item("DeadColor", "color for the character '-'"));
            items.Add(new Item("HalfAlifeColor", "color for the character '~'"));
            items.Add(new Item("RunningText", "typically a text like 'RUNNING...'"));
            items.Add(new Item("StoppedText", "typically a text like 'STOPPED.'"));
            items.Add(new Item("RunningColor", "color for the text 'RUNNING...'"));
            items.Add(new Item("StoppedColor", "color for the text 'STOPPED.'"));
            items.Add(new Item("DelayMilliSeconds", "number (1 ms = 0.001 seconds)"));
            items.Add(new Item("FeedbackColorOkay", "color for successful commands"));
            items.Add(new Item("FeedbackColorError", "color for failed commands"));
            items.Add(new Item("GenerationText", "a code like: 'gen: {0:#,0}'"));
            items.Add(new Item("GenerationTextCulture", "how are numbers written?"));
            items.Add(new Item("GenerationTextColor", "color for 'GenerationText'"));
            items.Add(new Item("AverageBarText", "a code like 'avg = {0:0.000}'"));
            items.Add(new Item("AverageBarColor", "color for 'AverageBarText'"));
            items.Add(new Item("PromptText", "a prompt for user input like '> '"));
            items.Add(new Item("PromptColor", "color of that prompt '> '"));
            items.Add(new Item("HelpColor", "color help text (with 'help' or '?')"));
            items.Add(new Item("BackColor", "general background color for console"));
                items.Add(new Item("FlagsEnum", "")); //maybe we should move this to a test package
            //items.Add(new Item("Mappers", "mapper-functions (numbers more visible)"));
            //items.Add(new Item("Scales", "number-to-ascii scales (for zoomed view)"));
        }


        //helper methods:

        protected void TryRerender()
        {
            reformatNeeded = true;
            if (app == null)
                return;
            if (suppress)
                return;
            app.Rerender(); //TODO: use proper C# event 'FormatChanged'
            reformatNeeded = false;
        }


        //public methods:

        public int NumItems => items.Count;

        public Item GetItem(int index) => items[index];

        /// <summary>
        /// Allows to temporarily suppress any GUI rebuilds and GUI reformats.<br></br>
        /// If you want to mass-change properties of the GuiConfig do as follows:<br></br>
        /// <br></br>
        /// <list type="number">
        /// <item>call 'SuppressRebuildReformat=true'</item>
        /// <item>change as many properties as you like</item>
        /// <item>call 'SuppressRebuildReformat=false'</item>
        /// </list>
        /// <br></br>
        /// Alternatively you can follow this procedure:
        /// <list type="number">
        /// <item>call 'Form=null'</item>
        /// <item>change as many properties as you like</item>
        /// <item>call 'Form=yourForm'</item>
        /// </list>
        /// </summary>
        public bool SuppressRebuildReformat
        {
            get => suppress;
            set
            {
                suppress = value;
                if (suppress == false)
                {
                    TryRerender();
                }
            }
        }

        /// <summary>
        /// The form on which we will call 'RebuildGui' or 'ReformatGui'.
        /// </summary>
        public ConsoleApp App
        {
            get => app;
            set { app = value; TryRerender(); }
        }

        /// <summary>
        /// The x coordinate of the top-left position of our cells field.
        /// </summary>
        public int TopLeftX
        {
            get => topLeftX;
            set { topLeftX = value; TryRerender(); }
        }

        /// <summary>
        /// The y coordinate of the top-left position of our cells field.
        /// </summary>
        public int TopLeftY
        {
            get => topLeftY;
            set { topLeftY = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text, when cell is alife (more than 0% life).
        /// </summary>
        public string AlifeText
        {
            get => alifeText;
            set { alifeText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text, when cell is dead (at 0% life).
        /// </summary>
        public string DeadText
        {
            get => deadText;
            set { deadText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text, when cell is 50% alife / 50% dead.
        /// </summary>
        public string HalfAlifeText
        {
            get => halfAlifeText;
            set { halfAlifeText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when cell is (100%) alife.
        /// </summary>
        public ConsoleColor AlifeColor
        {
            get => alifeColor;
            set { alifeColor = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when cell is (100%) dead.
        /// </summary>
        public ConsoleColor DeadColor
        {
            get => deadColor;
            set { deadColor = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when cell is 50% alife / 50% dead.
        /// </summary>
        public ConsoleColor HalfAlifeColor
        {
            get => halfAlifeColor;
            set { halfAlifeColor = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text, when the simulation is running.
        /// </summary>
        public string RunningText
        {
            get => runningText;
            set { runningText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text, when the simulation is stopped.
        /// </summary>
        public string StoppedText
        {
            get => stoppedText;
            set { stoppedText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when the simulation is running.
        /// </summary>
        public ConsoleColor RunningColor
        {
            get => runningColor;
            set { runningColor = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when the simulation is stopped.
        /// </summary>
        public ConsoleColor StoppedColor
        {
            get => stoppedColor;
            set { stoppedColor = value; TryRerender(); }
        }

        /// <summary>
        /// The delay between frames of the automatic calculation. 
        /// Can be zero.
        /// </summary>
        public int DelayMilliSeconds
        {
            get => delayMilliSeconds;
            set { delayMilliSeconds = value < 0 ? 0 : value; TryRerender(); }
        }

        /// <summary>
        /// This formatting string will be used to display the information "which generation do we currently have?". 
        /// The string will be used with 'String.Format()'. It is expected to receive one value of type integer.
        /// </summary>
        public string GenerationText
        {
            get => generationText;
            set { generationText = value; TryRerender(); }
        }

        /// <summary>
        /// More information about the formatting string. In some cultures the decimal separator is a point, not a 
        /// comma (as in US English). So, in an English UI the number 12334521.7 will be shown as "12,334,521.7", but 
        /// in a German UI the number will be shown as "12.334.521,7".
        /// </summary>
        public CultureInfo GenerationTextCulture
        {
            get => generationTextCulture;
            set { generationTextCulture = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when showing the current generation number.
        /// </summary>
        public ConsoleColor GenerationTextColor
        {
            get => generationTextColor;
            set { generationTextColor = value; TryRerender(); }
        }
        
        /// <summary>
        /// This formatting string will be used to display the information "what's the overall average cell value?". 
        /// The string will be used with 'String.Format()'. It is expected to receive one value of type decimal.
        /// That value is supposed to be from the interval [0.000 .. 1.000] ("between zero and one").
        /// </summary>
        public string AverageBarText
        {
            get => averageBarText;
            set { averageBarText = value; TryRerender(); }
        }

        /// <summary>
        /// We display this color, when showing the current overall average cell value.
        /// </summary>
        public ConsoleColor AverageBarColor
        {
            get => averageBarColor;
            set { averageBarColor = value; TryRerender(); }
        }

        /// <summary>
        /// We display this text at the start of a line.
        /// </summary>
        public string PromptText
        {
            get => promptText;
            set { promptText = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color for the prompt.
        /// </summary>
        public ConsoleColor PromptColor
        {
            get => promptColor;
            set { promptColor = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color for the command-line input of the user.
        /// </summary>
        public ConsoleColor UserColor
        {
            get => userColor;
            set { userColor = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color for feedback messages when everything is fine.
        /// </summary>
        public ConsoleColor FeedbackColorOkay
        {
            get => feedbackColorOkay;
            set { feedbackColorOkay = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color for feedback messages when we have an error / wrong command.
        /// </summary>
        public ConsoleColor FeedbackColorError
        {
            get => feedbackColorError;
            set { feedbackColorError = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color to display our help (command 'help' or '?').
        /// </summary>
        public ConsoleColor HelpColor
        {
            get => helpColor;
            set { helpColor = value; TryRerender(); }
        }

        /// <summary>
        /// We use this color to display secondary information (usually in gray). 
        /// This is additional information that should not be so bright. So, the 
        /// more important color is bright white or yellow.
        /// </summary>
        public ConsoleColor InfoColor
        {
            get => infoColor;
            set { infoColor = value; TryRerender(); }
        }

        /// <summary>
        /// This is the background color of our console.
        /// </summary>
        public ConsoleColor BackColor
        {
            get => backColor;
            set { backColor = value; TryRerender(); }
        }

        //TODO: background-color for everything

        /// <summary>
        /// When observing the overall average life of a cell field we discovered
        /// that values start with 0.5 and quickly drop below 0.1 and some time
        /// later even further down. Since the "interesting numbers" are all in 
        /// that lower region, we introduced a "mapper function" mechanism. The 
        /// idea is simple: Push small numbers up (similar to a "gamma correction"
        /// that many gamers know).<br></br>
        /// Should always have a "current object" and should "not be empty". 
        /// See <see cref="ObjectsWithIDs{I, O}.CurrentObject"/> and 
        /// <see cref="ObjectsWithIDs{I, O}.CanBeEmpty"/>. You can assign this
        /// property to itself (which will trigger a rerendering of the UI.
        /// </summary>
        public NamedObjects<IDecimalMapper> Mappers
        {
            get => mappers;
            set { mappers = value; TryRerender(); }
        }

        /// <summary>
        /// When rendering a zoomed view, we use an "ASCII art" mechanism: 
        /// The average life value of multiple cells will be computed and then
        /// we render that as ASCII art.<br></br>
        /// Should always have a "current object" and should "not be empty".
        /// See <see cref="ObjectsWithIDs{I, O}.CurrentObject"/> and 
        /// <see cref="ObjectsWithIDs{I, O}.CanBeEmpty"/>. You can assign this
        /// property to itself (which will trigger a rerendering of the UI.
        /// </summary>
        public NamedObjects<IAsciiArtScale> Scales
        {
            get => scales;
            set { scales = value; TryRerender(); }
        }

        /// <summary>
        /// This is just for testing purposes (it's a pure dummy something).
        /// </summary>
        public TestEnum FlagsEnum
        {
            get => flagsEnum;
            set { flagsEnum = value; TryRerender(); }
        }
    }
}
