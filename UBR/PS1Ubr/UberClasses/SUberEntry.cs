using System.Runtime.InteropServices;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SUberEntry
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;

        public uint LookupOffset;
        public uint U32Offset;
        public uint MeshDataOffset;
        public uint ModelDataOffset;
    }

    public struct SUberEntry_4
    {
        public uint A;
    }
}
