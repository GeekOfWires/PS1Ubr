using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PS1Ubr
{
    public class CAAB : ILoadable
    {
        public uint FaceCount;
        public uint NodeCount;
        public uint FaceIndexMapCount;
        public List<SAABFace> Faces = new List<SAABFace>();
        public List<SAABBNode> Nodes = new List<SAABBNode>();
        public List<int> FaceIndexMap = new List<int>();
        public SAABBNode RootNode;

        public void Clear()
        {
            FaceCount = NodeCount = FaceIndexMapCount = 0;
            Faces.Clear();
            Nodes.Clear();
        }

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            byte[] d;
            if (!r.Get(ref FaceCount)) return false;
            if (FaceCount != 0)
            {
                d = data.ReadMeshData(FaceCount * (uint) Marshal.SizeOf(typeof(SAABFace)));
                if (d == null || d.Length == 0) return false;
                Faces.Capacity = (int) FaceCount;
                Loaders.LoadBytesIntoObjectList(ref Faces, ref d);
            }

            if (!r.Get(ref NodeCount)) return false;

            d = data.ReadMeshData(0x28); // 40 bytes
            if (d == null || d.Length == 0) return false;
            Loaders.LoadBytesIntoObject(ref RootNode, ref d);

            if (NodeCount != 0)
            {
                d = data.ReadMeshData(NodeCount * (uint) Marshal.SizeOf(typeof(SAABBNode)));
                if (d == null || d.Length == 0) return false;
                Nodes.Capacity = (int) NodeCount;
                Loaders.LoadBytesIntoObjectList(ref Nodes, ref d);
            }

            if (!r.Get(ref FaceIndexMapCount)) return false;
            if (FaceIndexMapCount != 0)
            {
                d = data.ReadMeshData(FaceIndexMapCount * sizeof(uint));
                if (d == null || d.Length == 0) return false;
                FaceIndexMap.Capacity = (int) FaceIndexMapCount;
                Loaders.LoadBytesIntoObjectList(ref FaceIndexMap, ref d);
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
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

        [StructLayout(LayoutKind.Sequential, Pack=1)]
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
