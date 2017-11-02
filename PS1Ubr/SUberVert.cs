using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public struct SUberVert
    {
        internal CVec3f Position;
        internal CVec3f Normal;
        internal CVec2f UV0;
        internal CVec2f UV1;
        internal UInt32 Diffuse;
        internal Byte[] Bones;
        internal Single Weight;
    }
}
