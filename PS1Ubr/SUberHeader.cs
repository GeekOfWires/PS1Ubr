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
        public UInt16[] FourCC;
        public uint VersionA;
        public uint VersionB;
        public uint EntryCount;
        public uint Vec3Count;
        public uint Vec2Count;
        public uint LookupCount;
        public uint U32Count;
        public uint MeshDataSize;
        public uint Field_28;

        public static readonly UInt16[] FourCCValue = new UInt16[4] { 'u', 'b', 'e', 'r' };
        public static readonly uint VersionAValue = 1;
        public static readonly uint VersionBValue = 1;
    }
}
