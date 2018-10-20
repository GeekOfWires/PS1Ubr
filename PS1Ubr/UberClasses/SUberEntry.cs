using System.Runtime.InteropServices;

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
