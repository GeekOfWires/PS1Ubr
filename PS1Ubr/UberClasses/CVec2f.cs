using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CVec2f
    {
        public CVec2f(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// X Component
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y Component
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Magnitude
        /// </summary>
        public float Magnitude => ((float)Math.Sqrt((X * X) + (Y * Y)));

        /// <summary>
        /// Normalize
        /// </summary>
        public CVec2f Normalize => new CVec2f(X / Magnitude, Y / Magnitude);
    }
}
