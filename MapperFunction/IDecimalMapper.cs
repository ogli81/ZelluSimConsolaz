using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.MapperFunction
{
    /// <summary>
    /// Will map a numeric value from the interval [0..1] to a 
    /// new numeric value from the interval [0..1]. The aim is 
    /// to make values from a certain interval more visible 
    /// because it turned out that the values that we wanted 
    /// to show (like "average cell life") are from small intervals
    /// near zero (between 0.01 and 0.2) if the simulation ran
    /// for quite a while. Also, at the start of a random
    /// cell field, we start with a value of around 0.5 and that
    /// value will sink to below 0.25 during the first generation.
    /// So our most interesting numbers are always lower than 0.25
    /// and for later generations lower than 0.1 and this was the
    /// starting point to look for a mapper function.
    /// </summary>
    public interface IDecimalMapper
    {
        decimal GetValue(decimal from0To1);
    }
}
