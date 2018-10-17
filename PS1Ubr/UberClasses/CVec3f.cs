using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public struct CVec3f
    {
        public CVec3f(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
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
