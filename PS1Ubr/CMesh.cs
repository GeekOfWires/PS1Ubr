﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CMesh
    {
        public string Name;
        public string ModelName;
        public UInt32 LOD;
        public CVec3f BBMin;
        public CVec3f BBMax;
        public UInt32 MaxSectionCount2; // Always equal to mesh section count
        public UInt32 ID;   // CMeshSection::MeshID
        public UInt32 MeshSectionCount;
        public List<object> MeshSections;

        /*
         *  Just a basic note about the list of MeshSections, this was a complete
         *  1:1 move from the C++ header to this file. The List can probably be
         *  adjusted from a List<object> to List<CMeshSections>.
         */
    }
}