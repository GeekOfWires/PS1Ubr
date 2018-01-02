using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
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
    }
}
