using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.AsciiArtZoom
{
    /// <summary>
    /// This class is similar to the <see cref="ThresholdScale"/>, but instead 
    /// of providing an array of thresholds, the thresholds are being computed
    /// automatically for you.
    /// <br></br>
    /// <b> WARNING: </b> There are no bounds checks performed in this implementation. 
    /// We want maximum performance and bounds checks might impede performance.
    /// </summary>
    public class AutomaticScale : IAsciiArtScale
    {
        //state:

        protected char[] chars;
        protected int lenMinus1;
        public Uri MoreInfoHere { get; }


        //c'tors:

        public AutomaticScale(string chars, Uri moreInfoHere = null) : this(chars.ToCharArray(), moreInfoHere)
        {
        }

        public AutomaticScale(char[] chars, Uri moreInfoHere = null)
        {
            HandleUnwantedInput(chars);

            //we must take into account the highest value 255 (or 1.0)
            //that's why we copy over the array and double the highest entry
            this.chars = new char[chars.Length + 1];
            Array.Copy(chars, this.chars, chars.Length);
            this.chars[this.chars.Length - 1] = chars[chars.Length - 1];

            lenMinus1 = this.chars.Length - 1;

            MoreInfoHere = moreInfoHere;
        }


        //helper methods:

        protected void HandleUnwantedInput(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("can't be null!");
            if (chars.Length == 0)
                throw new ArgumentException("can't be empty!");
        }

        ////if we wanted, we could use something like this:
        //protected char HandleResult(int index)
        //{
        //    if (index <= 0)
        //        return chars[0];
        //    if (index >= (chars.Length - 1))
        //        return chars[chars.Length - 1];
        //    return chars[index];
        //}


        //public methods:

        public char GetChar(int from0To255) => chars[from0To255 * lenMinus1 / 255];

        public char GetChar(byte from0To255) => chars[from0To255 * lenMinus1 / 255];

        public char GetChar(decimal from0To1) => chars[(int)(from0To1 * lenMinus1)];

        public char GetChar(double from0To1) => chars[(int)(from0To1 * lenMinus1)];

        public char GetChar(float from0To1) => chars[(int)(from0To1 * lenMinus1)];
    }
}
