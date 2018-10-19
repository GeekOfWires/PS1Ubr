using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CPortalSystemPlane : ILoadable
    {
        public CVec3f Normal;
        public float Distance;
        public float E; //TODO

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            Console.WriteLine($"Loading {GetType().Name}");
            float temp = 0;
            if (!r.Get(ref temp)) return false;
            Normal.X = temp;

            if (!r.Get(ref temp)) return false;
            Normal.Y = temp;

            if (!r.Get(ref temp)) return false;
            Normal.Z = temp;

            if (!r.Get(ref Distance)) return false;
            if (!r.Get(ref E)) return false;

            return true;
        }
    }
}
