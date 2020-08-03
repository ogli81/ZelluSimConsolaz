using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected string alifeText = "X";
        protected string deadText = "-";
        protected string halfAlifeText = "~"; //also cool: "+"
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
        protected ConsoleColor generationTextColor = ConsoleColor.Blue;

        protected string promptText = "> ";
        protected ConsoleColor promptColor = ConsoleColor.White;
        protected ConsoleColor userColor = ConsoleColor.Yellow;

        protected ConsoleColor helpColor = ConsoleColor.Yellow;
        protected ConsoleColor infoColor = ConsoleColor.Gray;

        protected ConsoleColor backColor = ConsoleColor.Black;
        //protected ConsoleColor helpBackColor = ConsoleColor.DarkBlue; //was an idea: help is yellow on dark blue

        protected List<Item> items = new List<Item>();


        //c'tors:

        /// <summary>
        /// The GuiConfig is not connected (the App is null). As soon as you set the 
        /// app (<see cref="CliConfig.App"/>) the GuiConfig will try to connect with 
        /// the app.
        /// </summary>
        public CliConfig()
        {
            items.Add(new Item("topLeftX", "top left corner, x-coordinate"));
            items.Add(new Item("topLeftY", "top left corner, y-coordinate"));
            items.Add(new Item("alifeText", "typically only 1 character like 'X'"));
            items.Add(new Item("deadText", "typically only 1 character like '-'"));
            items.Add(new Item("halfAlifeText", "typically only 1 character like '~'"));
            items.Add(new Item("alifeColor", "color for the character 'X'"));
            items.Add(new Item("deadColor", "color for the character '-'"));
            items.Add(new Item("halfAlifeColor", "color for the character '~'"));
            items.Add(new Item("runningText", "typically a text like 'RUNNING...'"));
            items.Add(new Item("stoppedText", "typically a text like 'STOPPED.'"));
            items.Add(new Item("runningColor", "color for the text 'RUNNING...'"));
            items.Add(new Item("stoppedColor", "color for the text 'STOPPED.'"));
            items.Add(new Item("delayMilliSeconds", "number (1 ms = 0.001 seconds)"));
            items.Add(new Item("feedbackColorOkay", "color for successful commands"));
            items.Add(new Item("feedbackColorError", "color for failed commands"));
            items.Add(new Item("generationText", "a code like: 'gen: {0:#,0}'"));
            items.Add(new Item("generationTextCulture", "how are numbers written?"));
            items.Add(new Item("generationTextColor", "color for 'generationText'"));
            items.Add(new Item("promptText", "a prompt for user input like '> '"));
            items.Add(new Item("promptColor", "color of that prompt '> '"));
            items.Add(new Item("helpColor", "color help text (with 'help' or '?')"));
            items.Add(new Item("backColor", "general background color for console"));
        }


        //helper methods:

        protected void TryRerender()
        {
            reformatNeeded = true;
            if (app == null)
                return;
            if (suppress)
                return;
            app.Rerender(); //TODO: use proper C# event 'RequestReformat'
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
        //TODO: color for command (that the user is currently typing in after the prompt)
    }
}
