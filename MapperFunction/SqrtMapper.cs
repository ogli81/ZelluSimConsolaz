using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZelluSim.Misc;

namespace ZelluSimConsolaz.MapperFunction
{
    /// <summary>
    /// Will map a value to its square root. Examples: <br></br>
    /// 0.25 is mapped to 0.5 <br></br>
    /// 0.09 is mapped to 0.3 <br></br>
    /// 0.01 is mapped to 0.1 <br></br>
    /// values below an 'eps' (usually 0.00000001) will be mapped to 0
    /// </summary>
    public class SqrtMapper : IDecimalMapper
    {
        readonly decimal eps;

        public SqrtMapper(decimal eps = 0.00000001m)
        {
            this.eps = eps;
        }

        public decimal GetValue(decimal from0To1)
        {
            if (from0To1 < eps)
                return 0m;
            return DecimalMath.Sqrt(from0To1);
        }
    }
}
