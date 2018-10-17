using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CBoneTransform
    {
        public Int32 Flags; //Flags indicate non-identity for the given component
        public CVec3f Position;
        public CVec4f RotationQuat;
        public float[] RotationMatrix; //Scale xform?

        public bool Load(object r, CUberData data)
        {
            throw new NotImplementedException();
        }
    }
}
