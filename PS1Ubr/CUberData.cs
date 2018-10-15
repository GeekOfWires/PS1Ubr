using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using PS1Ubr.Unmanaged;

namespace PS1Ubr
{
    public class CUberData
    {
        List<SUberEntry> Entries;
        List<CVec3f> Vec3Data;
        List<CVec2f> Vec2Data;
        List<uint> LookupData;
        List<uint> U32Data;
        List<byte> MeshData;
        List<byte> ModelData;

        //todo:
        /*
         protected CDynMemoryBuffer m_ModelDataCache;
        const uint8_t*		m_pMeshDataCur = nullptr;
	    const uint8_t*		m_pMeshDataEnd = nullptr;
        */
        protected uint m_LookupOffset = 0;
        protected uint m_U32Offset = 0;

        public bool Load(MemoryStream r)
        {
            //todo: unfinished
            throw new NotImplementedException();

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

        public bool FetchMeshSystem(uint index, CMeshSystem system)
        {
            throw new NotImplementedException();
        }

        public bool FetchMeshSystem(char pName, CMeshSystem system)
        {
            throw new NotImplementedException();
        }

        //todo: convert
        /*
             const void* ReadMeshData(size_t n) {
		    if (!m_pMeshDataCur) {
			    return nullptr;
		    }
		    if (m_pMeshDataEnd - m_pMeshDataCur < n) {
			    return nullptr;
		    }
		    const void* p = m_pMeshDataCur;
		    m_pMeshDataCur += n;
		    return p;
	        }
         */

        public CVec3f GetVec3(uint index)
        {
            throw new NotImplementedException();
        }

        public uint GetLookup(uint size)
        {
            uint res = m_LookupOffset;
            m_LookupOffset += size;
            return res;
        }

        uint GetU32(uint size)
        {
            uint res = m_U32Offset;
            m_U32Offset += size;
            return res;
        }
    }
}
