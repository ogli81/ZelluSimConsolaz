using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.ConsoleCLI
{
    /// <summary>
    /// This dummy class is only there to test flags-enums (bitfield-enums).
    /// </summary>
    [Flags]
    public enum TestEnum : ushort //ushort gives us 16 bits (16 possible flags)
    {
        None = 0,
        FirstBlood = 1,
        SecondMinute = 2,
        ThirdSomething = 4,
        FourthAwakens = 8,
        FifthElement = 16,
        SixthSensor = 32,
        SeventhOfNine = 64,
        EightsAfterlife = 128,
        NinthAndLastLifeOfThisCat = 256
    }
}
