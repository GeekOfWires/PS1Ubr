using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CMesh
    {
        public string Name;
        public string ModelName; //?
        public uint LOD;
        public CVec3f BBMin;
        public CVec3f BBMax;
        public uint MaxSectionCount2; // Always equal to mesh section count
        public uint ID;   // CMeshSection::MeshID
        public uint MeshSectionCount;
        public List<CMeshSection> MeshSections;

        /*
         *  Just a basic note about the list of MeshSections, this was a complete
         *  1:1 move from the C++ header to this file.
         */

        public bool Load()
        {
            throw new NotImplementedException();
        }

        public bool LoadMeshSections()
        {
            throw new NotImplementedException();
        }
    }
}
