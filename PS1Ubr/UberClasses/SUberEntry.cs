using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SUberEntry
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
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
