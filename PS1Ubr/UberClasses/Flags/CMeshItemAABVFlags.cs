using System;

namespace PS1Ubr.UberClasses
{
    [Flags]
    public enum CMeshItemAABVFlags
    {
        MESHITEMAABV_FLAG_LEAF = 0x01,
        MESHITEMAABV_FLAG_LEFT = 0x10,
        MESHITEMAABV_FLAG_RIGHT = 0x20
    }
}
