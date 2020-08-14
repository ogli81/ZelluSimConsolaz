using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.MapperFunction
{
    /// <summary>
    /// Most basic mapper function: Simply returns the input value.
    /// </summary>
    public class LinearMapper : IDecimalMapper
    {
        public decimal GetValue(decimal from0To1)
        {
            return from0To1;
        }
    }
}
