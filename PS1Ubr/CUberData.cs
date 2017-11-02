using System;
using System.Collections.Generic;
using System.IO;
using PS1Ubr.Unmanaged;

namespace PS1Ubr
{
    public class CUberData
    {
        List<SUberEntry> Entries;
        List<CVec3f> Vec3Data;
        List<CVec2f> Vec2Data;
        List<UInt32> LookupData;
        List<UInt32> U32Data;
        List<Byte> MeshData;
        List<Byte> ModelData;

        public bool Load(MemoryStream r)
        {
            uint i;
            SUberHeader hdr = new SUberHeader();    // C# Possibly Unassigned Value

            if (false)  // Gotta find CDynMemoryReader for Get(hdr)
            {
                return false;
            }
            if (Memory.ArraysCompare(hdr.FourCC, SUberHeader.FourCCValue))
            {
                return false;
            }
            if (hdr.VersionA != SUberHeader.VersionAValue)
            {
                return false;
            }
            if (hdr.VersionB != SUberHeader.VersionBValue)
            {
                return false;
            }

            return true;
        }

        uint NumEntries()
        {
            return (uint)(Entries.Count > 0 ? Entries.Count - 1 : 0);
        }
    }
}
