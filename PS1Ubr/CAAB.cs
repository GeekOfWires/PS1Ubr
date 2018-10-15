using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    // Incomplete
    public class CAAB
    {
        public UInt32 FaceCount;
        public UInt32 NodeCount;
        public UInt32 FaceIndexMapCount;
        public List<SAABFace> Faces;
        public List<SAABBNode> Nodes;
        public List<int> FaceIndexMap;
        public SAABBNode RootNode;

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Load()
        {
            throw new NotImplementedException();
        }

        public struct SAABBNode
        {
            UInt16 Flags; //2 - 1 = has leaves
            UInt16 FaceCount; //4
            CVec3f BBMin; //10
            CVec3f BBMax; //1C
            uint FaceOffset; //20
            uint LeftChild; //24
            uint RightChild; //28
        }

        public struct SAABFace
        {
            UInt16 MeshID;
            UInt16 MeshSectionID;
            UInt16 V0;
            UInt16 V1;
            UInt16 V2;
        }
    }
}
