using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.MapperFunction
{
    /// <summary>
    /// Will map the value using a logarithmic scale. The 
    /// default behavior is as follows:
    /// <list type="bullet">
    /// <item>
    /// the base of the logarithm is '2' (binary logarithm)
    /// </item>
    /// <item>
    /// exponents greater than 0 will result in 1 (2^0 = 1.0000000000)
    /// </item>
    /// <item>
    /// exponents lower than -10 will result in 0 (2^-10 = 0.0009765625)
    /// </item>
    /// <item>
    /// all other values will be mapped accordingly (between 0 and 1)
    /// </item>
    /// </list>
    /// </summary>
    public class LogarithmicMapper : IDecimalMapper
    {
        readonly double logBase;
        readonly decimal max;
        readonly decimal eps;
        readonly decimal range;
        readonly decimal rangeInv;

        public LogarithmicMapper(double logBase = 2, double epsExp = -10)
        {
            this.logBase = logBase;
            eps = (decimal)(Math.Pow(logBase, epsExp));
            max = 1m;
            range = (decimal)Math.Abs(0d - epsExp);
            rangeInv = 1m / range;
        }

        public decimal GetValue(decimal from0To1)
        {
            if (from0To1 >= max)
                return max;
            if (from0To1 < eps)
                return 0m;
            decimal exp = (decimal)Math.Log((double)from0To1, logBase);
            return Math.Abs(exp * rangeInv);
        }
    }
}
