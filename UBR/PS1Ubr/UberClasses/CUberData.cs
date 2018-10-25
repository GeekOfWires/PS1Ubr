using System;
using System.Collections.Generic;
using System.Linq;
using PS1Ubr.UberClasses;

namespace PS1Ubr
{
    public class CUberData
    {
        public List<SUberEntry> Entries = new List<SUberEntry>();
        public List<CVec3f> Vec3Data = new List<CVec3f>();
        public List<CVec2f> Vec2Data = new List<CVec2f>();
        public List<uint> LookupData = new List<uint>();
        public List<uint> U32Data = new List<uint>();
        public List<byte> MeshData = new List<byte>();
        private byte[] MeshDataBytes = null; // Populated after CUberData is loaded to avoid excessive .ToArray() calls on MeshData List
        public List<byte> ModelData = new List<byte>();

        protected CDynMemoryBuffer m_ModelDataCache = new CDynMemoryBuffer();
        protected uint m_pMeshDataCur;
        protected uint m_pMeshDataEnd;
        protected uint m_LookupOffset = 0;
        protected uint m_U32Offset = 0;

        public bool Load(ref CDynMemoryReader r)
        {
            Console.WriteLine($"Loading {GetType().Name}");
            SUberHeader hdr = new SUberHeader();    // C# Possibly Unassigned Value

            if (!r.Get(ref hdr))
            {
                return false;
            }
            if (!hdr.FourCC.SequenceEqual(SUberHeader.FourCCValue))
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

            Console.WriteLine($"Loading {hdr.EntryCount} Entries");
            Entries.Capacity = (int) hdr.EntryCount;
            if (!r.GetRaw(ref Entries, hdr.EntryCount)) return false;

            Console.WriteLine($"Loading {hdr.Vec3Count} Vec3Data");
            Vec3Data.Capacity = (int) hdr.Vec3Count;
            if (!r.GetRaw(ref Vec3Data, hdr.Vec3Count)) return false;

            Console.WriteLine($"Loading {hdr.Vec2Count} Vec2Data");
            Vec2Data.Capacity = (int) hdr.Vec2Count;
            if (!r.GetRaw(ref Vec2Data, hdr.Vec2Count)) return false;

            Console.WriteLine($"Loading {hdr.LookupCount} LookupData");
            LookupData.Capacity = (int) hdr.LookupCount;
            if (!r.GetRaw(ref LookupData, hdr.LookupCount)) return false;

            Console.WriteLine($"Loading {hdr.MeshDataSize} MeshDataSize");
            MeshData.Capacity = (int) hdr.MeshDataSize;
            if (!r.GetRaw(ref MeshData, hdr.MeshDataSize)) return false;
            MeshDataBytes = MeshData.ToArray();

            Console.WriteLine($"Loading {hdr.U32Count} U32Count");
            U32Data.Capacity = (int) hdr.U32Count;
            if (!r.GetRaw(ref U32Data, hdr.U32Count)) return false;

            Console.WriteLine($"Loading {hdr.ModelDataSize} ModelDataSize");
            ModelData.Capacity = (int) hdr.ModelDataSize;
            if (!r.GetRaw(ref ModelData, hdr.ModelDataSize)) return false;

            Entries.Add(new SUberEntry
            {
                Name = ".",
                LookupOffset = hdr.LookupCount,
                MeshDataOffset = hdr.U32Count,
                ModelDataOffset = hdr.MeshDataSize,
                U32Offset = hdr.ModelDataSize
            });

            return true;
        }

        private uint NumEntries()
        {
            return (uint)(Entries.Count > 0 ? Entries.Count - 1 : 0);
        }

        public bool FetchMeshSystem(uint index, CMeshSystem system)
        {
            if (index >= NumEntries()) return false;
            SUberEntry entry = Entries[(int) index];
            SUberEntry nextEntry = Entries[(int) index + 1];

            var bytes = ModelData.Skip((int) entry.ModelDataOffset).Take((int) nextEntry.ModelDataOffset - (int) entry.ModelDataOffset).ToArray();
            m_ModelDataCache.m_pBuffer = bytes;
            var r = new CDynMemoryReader(ref m_ModelDataCache);

            m_pMeshDataCur = entry.MeshDataOffset;
            m_pMeshDataEnd = m_pMeshDataCur + (nextEntry.MeshDataOffset - entry.MeshDataOffset);

            m_LookupOffset = entry.LookupOffset;
            m_U32Offset = entry.U32Offset;

            if (!system.Load(ref r, this)) return false;

            system.Name = entry.Name;
            if(entry.Name.StartsWith("map") && char.IsNumber(entry.Name[3]) && char.IsNumber(entry.Name[4])) system.Flags |= (int) MeshFlags.MESHSYS_FLAG_MAP_GEOMETRY;
            if(entry.Name.StartsWith("ugd") && char.IsNumber(entry.Name[3]) && char.IsNumber(entry.Name[4])) system.Flags |= (int) MeshFlags.MESHSYS_FLAG_MAP_GEOMETRY;
            if(entry.Name.StartsWith("lava") && char.IsNumber(entry.Name[4]) && char.IsNumber(entry.Name[5])) system.Flags |= (int) MeshFlags.MESHSYS_FLAG_MAP_GEOMETRY;

            return true;
        }

        public CMeshSystem FetchMeshSystem(char[] pName, CMeshSystem system)
        {
            var entry = Entries.FindIndex(x => x.Name == new string(pName));
            var fetched = FetchMeshSystem((uint)entry, system);
            if(!fetched) { throw new Exception($"Failed to fetch mesh system for object {pName}");}

            return system;
        }

        public byte[] ReadMeshData(uint length)
        {
            if (m_pMeshDataEnd - m_pMeshDataCur < length) throw new IndexOutOfRangeException();
            var bytes = new byte[length];
            Array.Copy(MeshDataBytes, (int) m_pMeshDataCur, bytes, 0, length);
            m_pMeshDataCur += length;
            return bytes;
        }

        public CVec3f GetVec3(uint index)
        {
            return Vec3Data[(int) LookupData[(int) index]];
        }

        public uint GetLookup(uint size)
        {
            uint res = m_LookupOffset;
            m_LookupOffset += size;
            return res;
        }

        public uint GetU32(uint size)
        {
            uint res = m_U32Offset;
            m_U32Offset += size;
            return res;
        }
    }
}
