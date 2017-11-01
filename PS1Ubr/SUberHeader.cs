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
        internal UInt16[] FourCC;
        internal uint VersionA;
        internal uint VersionB;
        internal uint EntryCount;
        internal uint Vec3Count;
        internal uint Vec2Count;
        internal uint LookupCount;
        internal uint U32Count;
        internal uint MeshDataSize;
        internal uint Field_28;

        internal static readonly UInt16[] FourCCValue = new UInt16[4] { 'u', 'b', 'e', 'r' };
        internal static readonly uint VersionAValue = 1;
        internal static readonly uint VersionBValue = 1;
    }
}
