using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public struct CVec2f
    {
        private float x;
        private float y;

        public CVec2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /*
         * Properties in case I miss something
         */

        /// <summary>
        /// X Component
        /// </summary>
        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        /// Y Component
        /// </summary>
        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
    }
}
