using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public struct SUberEntry
    {
        internal string Name;
        internal uint LookupOffset;
        internal uint U32Offset;
        internal uint MeshDataOffset;
        internal uint ModelDataOffset;
    }

    public struct SUberEntry_4
    {
        internal uint A;
    }
}
