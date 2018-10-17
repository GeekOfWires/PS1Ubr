using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SUberHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal byte[] FourCC;

        internal uint VersionA;
        internal uint VersionB;
        internal uint EntryCount;
        internal uint Vec3Count;
        internal uint Vec2Count;
        internal uint LookupCount;
        internal uint U32Count;
        internal uint MeshDataSize;
        internal uint ModelDataSize;
        internal uint Field_28;

        internal static readonly byte[] FourCCValue = { (byte)'u', (byte)'b', (byte)'e', (byte)'r' };

        internal static readonly uint VersionAValue = 1;
        internal static readonly uint VersionBValue = 1;
    }
}


