using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public class CMesh : ILoadable
    {
        public string Name;
        public string ModelName; //?
        public uint LOD;
        public CVec3f BBMin;
        public CVec3f BBMax;
        public uint MeshSectionCount2; // Always equal to mesh section count
        public uint ID;   // CMeshSection::MeshID
        public uint MeshSectionCount;
        public List<CMeshSection> MeshSections = new List<CMeshSection>();

        public bool LoadMeshSections(ref CDynMemoryReader r, ref CUberData data)
        {
            uint i;
            MeshSections.Capacity = (int) MeshSectionCount;

            for (i = 0; i < MeshSectionCount; i++)
            {
                var section = new CMeshSection();
                if (!section.Load(ref r, ref data)) return false;
                MeshSections.Add(section);
            }

            return true;
        }

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            string s0 = r.ReadPascalStr();
            if (s0 == "") return false;
            Name = s0;

            string s1 = r.ReadPascalStr();
            if (s1 == "") return false;
            ModelName = s1;

            if (!r.Get(ref LOD)) return false;
            if (!r.Get(ref BBMin)) return false;
            if (!r.Get(ref BBMax)) return false;
            if (!r.Get(ref MeshSectionCount2)) return false;
		    //Could be quality
            if (!r.Get(ref ID)) return false;
            if (!r.Get(ref MeshSectionCount)) return false;

            return true;
        }
    }
}
