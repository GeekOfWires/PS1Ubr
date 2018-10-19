using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CBoneTransform : ILoadable
    {
        public int Flags; //Flags indicate non-identity for the given component
        public CVec3f Position;
        public CVec4f RotationQuat;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 288)]
        public List<float> RotationMatrix = new List<float>(); //Scale xform? - This was float RotationMatrix[3 * 3]


        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            if (!r.Get(ref Flags)) return false;
            if (!r.Get(ref Position)) return false;
            if (!r.Get(ref RotationQuat)) return false;
            if (!r.GetRaw(ref RotationMatrix, 3 * 3)) return false;

            return true;
        }
    }
}
