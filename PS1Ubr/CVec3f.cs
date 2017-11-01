using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public struct CVec3f
    {
        private float x;
        private float y;
        private float z;

        public CVec3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /*
         * Properties prepped in case I miss something
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

        /// <summary>
        /// Z Component
        /// </summary>
        public float Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }
    }
}
