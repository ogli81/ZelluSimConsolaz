using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelluSimConsolaz.AsciiArtZoom
{
    /// <summary>
    /// Will give you an ASCII character (for rendering ascii art) 
    /// based on a numeric value. The numeric value is either an
    /// integral number from the interval [0..255] or a floating
    /// point number from the interval [0..1].
    /// </summary>
    public interface IAsciiArtScale
    {
        char GetChar(int from0To255);
        char GetChar(byte from0To255);
        char GetChar(decimal from0To1);
        char GetChar(double from0To1);
        char GetChar(float from0To1);
    }
}
