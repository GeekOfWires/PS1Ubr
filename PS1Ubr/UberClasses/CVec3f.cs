using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CVec3f
    {
        public CVec3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
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
        /// Z Component
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Length of CVec3f
        /// </summary>
        public float Magnitude => ((float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z)));

        /// <summary>
        /// Normalize
        /// </summary>
        public CVec3f Normalize => new CVec3f(X / Magnitude, Y / Magnitude, Z / Magnitude);
    }
}
