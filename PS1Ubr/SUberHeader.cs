using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    /// <summary>
    /// Literally just rewrote the SUberHeader as close as
    /// possible
    /// </summary>
    public struct SUberHeader
    {
        internal Byte[] FourCC;
        internal UInt32 VersionA;
        internal UInt32 VersionB;
        internal UInt32 EntryCount;
        internal UInt32 Vec3Count;
        internal UInt32 Vec2Count;
        internal UInt32 LookupCount;
        internal UInt32 U32Count;
        internal UInt32 MeshDataSize;
        internal UInt32 Field_28;

        internal static readonly Byte[] FourCCValue = 
            new Byte[4] { (byte)('u'), (byte)('b'), (byte)('e'), (byte)('r') };

        internal static readonly UInt32 VersionAValue = 1;
        internal static readonly UInt32 VersionBValue = 1;
    }
}
