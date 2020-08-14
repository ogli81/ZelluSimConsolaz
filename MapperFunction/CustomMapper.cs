using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using ZelluSim.CellField;
using ZelluSim.Misc;

namespace ZelluSimConsolaz.MapperFunction
{
    /// <summary>
    /// An example best explains how this works. Example: <br></br>
    /// We have 'sortedValues': { 0.0, 0.1, 0.25, 0.45, 0.75, 1.0 } <br></br>
    /// an input of 0.0 or lower will be mapped to 0.0 <br></br>
    /// an input of 0.1 will be mapped to 0.2 <br></br>
    /// an input of 0.25 will be mapped to 0.4 <br></br>
    /// an input of 0.45 will be mapped to 0.6 <br></br>
    /// an input of 0.75 will be mapped to 0.8 <br></br>
    /// an input of 1.0 or higher will be mapped to 1.0 <br></br>
    /// All values inbetween those steps will be interpolated. The current
    /// interpolation is a very simple 'linear interpolation', but better
    /// interpolations may be implemented in future versions.
    /// </summary>
    public class CustomMapper : IDecimalMapper
    {
        readonly decimal[] sortedValues; //array not really "readonly", but we make a defensive clone
        readonly decimal len;
        readonly decimal lenInv;
        readonly decimal[] valueDistances; //will have 1 less element than 'sortedValues'
        readonly decimal[] valueDistancesInv;

        /// <summary>
        /// Creates a new CustomMapper. There are restrictions for the sortedValues parameter.
        /// </summary>
        /// <param name="sortedValues">
        /// These are the rules that this array must follow: <br></br>
        /// it mustn't be null or empty <br></br>
        /// it must be sorted ascending <br></br>
        /// the first value should be 0 (zero) and the last value should be 1 (one) <br></br>
        /// there should be no duplicates (double values, e.g. a 0.5 followed by a 0.5) <br></br>
        /// </param>
        public CustomMapper(decimal[] sortedValues)
        {
            if (sortedValues == null)
                throw new ArgumentNullException("can't be null!");
            if (sortedValues.Length == 0)
                throw new ArgumentException("can't be empty!");
            if (!Arrays.IsSortedAscending(sortedValues))
                throw new ArgumentException("must be sorted ascending!");
            if (sortedValues[0] != 0m)
                throw new ArgumentException("first value should be zero!");
            if (sortedValues[1] != 1m)
                throw new ArgumentException("last value should be one!");
            for (int i = 0; i < sortedValues.Length - 1; ++i)
                if (sortedValues[i] == sortedValues[i + 1])
                    throw new ArgumentException($"duplicates detected: sortedValues[{i}] and sortedValues[{i+1}] (value is: {sortedValues[i]})");

            this.sortedValues = (decimal[])sortedValues.Clone(); //defensive clone
            len = sortedValues.Length - 1; //the '-1' is important
            lenInv = 1.0m / len;
            valueDistances = new decimal[sortedValues.Length - 1];
            valueDistancesInv = new decimal[sortedValues.Length - 1];
            for (int i = 0; i < valueDistances.Length; ++i)
            {
                valueDistances[i] = sortedValues[i + 1] - sortedValues[i];
                if (valueDistances[i] == 0)
                    throw new ArgumentException("calculated a distance of zero! do we have duplicate values? or very small differences?");
                valueDistancesInv[i] = 1.0m / valueDistances[i];
            }
        }

        //  _-''--_-'-__-'-_.-'
        public decimal GetValue(decimal from0To1)
        {
            if (from0To1 <= 0m)
                return 0m;
            if (from0To1 >= 1m)
                return 1m;
            int i1, i2;
            for(i1 = 0, i2 = 1; i2 < sortedValues.Length; ++i1, ++i2)
                if (from0To1 >= sortedValues[i1] && from0To1 <= sortedValues[i2])
                    break;
            decimal val = from0To1 - sortedValues[i1];
            val *= valueDistancesInv[i1]; //val should now be in [0..1]
            val = (((decimal)i1) + val) * lenInv; //example: (4 + 0.5) * 0.2 = 0.9  (it is half-way between 0.8 and 1.0)
            return val;
        }
    }
}
