using System;
using System.Runtime.InteropServices;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CVec4f
    {
        public CVec4f(float w, float x, float y, float z)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// W Component
        /// </summary>
        public float W { get; set; }

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
        public CVec4f Normalize => new CVec4f(W / Magnitude, X / Magnitude, Y / Magnitude, Z / Magnitude);
    }
}
