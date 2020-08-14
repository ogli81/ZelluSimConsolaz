using System;
using System.Linq;
using ZelluSim.Misc;

namespace ZelluSimConsolaz.AsciiArtZoom
{
    /// <summary>
    /// Will give you an ascii character based on a scale of values. Example:<br></br>
    /// <code>
    /// char[] chars = {'#','+',' '}; <br></br>
    /// byte[] thresholds = { 75, 170 }; <br></br>
    /// </code>
    /// Depending on the value that you provide to <see cref="ThresholdScale.GetChar(byte)"/>
    /// you get different responses. Example:<br></br>
    /// <code>
    /// char response1 = mapper.GetChar(230); //---> response1 == ' ' <br></br>
    /// char response2 = mapper.GetChar(100); //---> response2 == '+' <br></br>
    /// char response3 = mapper.GetChar(74); //---> response3 == '#' <br></br>
    /// </code>
    /// If you don't want to provide a scale of thresholds, but rather like auto-generation
    /// of thresholds, you might be interested in <see cref="AutomaticScale"/>.
    /// </summary>
    public class ThresholdScale : IAsciiArtScale
    {
        //state:

        protected int[] thresholdsInt;
        protected byte[] thresholdsByte;
        protected decimal[] thresholdsDecimal;
        protected double[] thresholdsDouble;
        protected float[] thresholdsFloat;
        protected char[] chars;
        public Uri MoreInfoHere { get; }


        //c'tors:

        public ThresholdScale(byte[] thresholds, char[] chars, Uri moreInfoHere = null)
        {
            HandleUnwantedInput(thresholds, chars);

            thresholdsByte = (byte[])thresholds.Clone();
            thresholdsInt = new int[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsInt[i] = thresholdsByte[i];
            thresholdsDecimal = new decimal[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsDecimal[i] = ((decimal)thresholdsInt[i]) / 255m;
            thresholdsDouble = new double[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsDouble[i] = (double)thresholdsDecimal[i];
            thresholdsFloat = new float[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsFloat[i] = (float)thresholdsDecimal[i];

            this.chars = (char[])chars.Clone();

            MoreInfoHere = moreInfoHere;
        }

        public ThresholdScale(decimal[] thresholds, char[] chars, Uri moreInfoHere = null)
        {
            HandleUnwantedInput(thresholds, chars);
            HandleUnwantedValues(thresholds);

            thresholdsDecimal = (decimal[])thresholds.Clone();
            thresholdsDouble = new double[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsDouble[i] = (double)thresholdsDecimal[i];
            thresholdsFloat = new float[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsFloat[i] = (float)thresholdsDecimal[i];
            thresholdsInt = new int[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsInt[i] = (int)(thresholdsDecimal[i] * 255m);
            thresholdsByte = new byte[thresholds.Length];
            for (int i = 0; i < thresholds.Length; ++i)
                thresholdsByte[i] = (byte)(thresholdsInt[i]);

            this.chars = (char[])chars.Clone();

            MoreInfoHere = moreInfoHere;
        }


        //helper methods:

        protected void HandleUnwantedInput(Array thresholds, char[] chars)
        {
            if (thresholds == null || chars == null)
                throw new ArgumentNullException("can't be null!");
            if (thresholds.Length == 0 || chars.Length == 0)
                throw new ArgumentException("can't be empty!");
            if (thresholds.Length != chars.Length - 1)
                throw new ArgumentException("thresholds should hold 1 less element than characters!");

            if (Arrays.IsSortedAscending(thresholds))
            {
                //nothing to do - everything fine
            }
            else
            if (Arrays.IsSortedDescending(thresholds))
            {
                Array.Reverse(thresholds);
                Array.Reverse(chars);
            }
            else
            {
                throw new ArgumentException("Array 'thresholds' must be sorted (either ascending or descending)!");
            }

            //double entries don't make sense
            for(int i = 1; i < thresholds.Length; ++i)
            {
                object ob1 = thresholds.GetValue(i);
                object ob2 = thresholds.GetValue(i - 1);
                if (ob1.Equals(ob2))
                    throw new Exception($"At least two values are the same: {ob1} and {ob2}");
            }
        }

        protected void HandleUnwantedValues(decimal[] thresholds)
        {
            //now we could check all the values in our array
            //but we know that the array is sorted (ascending) at this point
            decimal d0 = thresholds.First();
            if (d0 < 0m)
                throw new ArgumentException($"value is too low: {d0}");
            decimal dN = thresholds.Last();
            if (dN > 1m)
                throw new ArgumentException($"value is too high: {dN}");
        }

        protected char HandleResult(int index)
        {
            if (index >= 0) //exact hit
                return chars[index + 1];
            else
            {
                int inverted = ~index;

                //TODO: can this ever happen? => write test with value 1.0m (or 255) and set breakpoint here
                if (inverted >= chars.Length) //too high
                    return chars[chars.Length - 1];

                return chars[inverted];
            }
        }


        //public methods:

        public char GetChar(int from0To255)
        {
            //first idea:
            //int index = Array.BinarySearch(thresholdsInt, from0To255);
            //return HandleResult(index);

            //also possible:
            for (int i = thresholdsInt.Length - 1; i > -1; --i)
                if (from0To255 >= thresholdsInt[i])
                    return chars[i + 1];
            return chars[0];

            //in theory, a binary search is faster than a linear search
            //but it may be possible, that the simple linear search is faster
            //TODO: test with N=10 and N=100, stop the time after 100.000.000.000 random requests
        }

        public char GetChar(byte from0To255) => HandleResult(Array.BinarySearch(thresholdsByte, from0To255));

        public char GetChar(decimal from0To1) => HandleResult(Array.BinarySearch(thresholdsDecimal, from0To1));

        public char GetChar(double from0To1) => HandleResult(Array.BinarySearch(thresholdsDouble, from0To1));

        public char GetChar(float from0To1) => HandleResult(Array.BinarySearch(thresholdsFloat, from0To1));
    }
}
