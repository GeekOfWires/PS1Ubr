using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CMeshSection
    {
        public string MaterialName;

        public uint Flags = 0; //2 = tristrip
        public uint ID = 0;
        public uint MeshID = 0;
        public uint LOD = 0;

        public eVertexType VertexType = eVertexType.None;

        public CVec3f BBMin;
        public CVec3f BBMax;
        public uint IndexCount = 0;
        public uint VertexCount = 0;
        public List<SUberVert> Verts = new List<SUberVert>();
        public List<UInt16> Indices = new List<UInt16>();

        public bool HasNormal = false;
        public bool HasUV0 = false;
        public bool HasUV1 = false;
        public bool HasColor = false;
        public bool HasSkin = false;

        protected uint LookupOffset;
        protected uint U32Offset;

        protected CVec3f DecodeNormal(uint n)
        {
            uint x = ((uint)(n >> 20) & 0x3FF) - 0x200;
            uint y = ((uint)(n >> 10) & 0x3FF) - 0x200;
            uint z = ((uint)(n) & 0x3FF) - 0x200;

            return new CVec3f
            {
                X = (float)x * 1.0f / 512.0f,
                Y = (float)y * 1.0f / 512.0f,
                Z = (float)z * 1.0f / 512.0f
            };
        }

        protected void BuildVerticies_Lit(ref CUberData data)
        {
            uint i;
            Verts.Capacity = (int)VertexCount;
            HasColor = HasUV0 = true;

            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int)i];
                v.Position = data.GetVec3(LookupOffset + i);
                v.Diffuse = data.U32Data[(int) u32o++];
                v.UV0 = data.Vec2Data[(int)data.U32Data[(int)u32o++]];
                Verts[(int)i] = v;
            }
        }

        protected void BuildVertices_Unlit(ref CUberData data)
        {
            uint i;
            Verts.Capacity = (int)VertexCount;
            HasNormal = HasUV0 = true;

            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int)i];
                v.Position = data.GetVec3(LookupOffset + i);
                v.Normal = DecodeNormal(data.U32Data[(int) u32o++]);
                v.UV0 = data.Vec2Data[(int)data.U32Data[(int)u32o++]];
                Verts[(int)i] = v;
            }
        }

        protected void DecodeDeform(ref SUberVert v, uint val)
        {
            //todo:
            //v.Bones[0] = (val >> 8) & 0xFF;
            //v.Bones[1] = (val) & 0xFF;
            Int16 w = (Int16)(val >> 16);
            v.Weight = (float)w / 16384.0f;
        }

        protected void BuildVertices_Deform(ref CUberData data)
        {
            uint i;
            Verts.Capacity = (int)VertexCount;
            HasNormal = HasUV0 = HasSkin = true;

            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int)i];
                v.Position = data.GetVec3(LookupOffset + i);
                v.Normal = DecodeNormal(data.U32Data[(int) u32o++]);
                v.UV0 = data.Vec2Data[(int)data.U32Data[(int)u32o++]];
                DecodeDeform(ref v, data.U32Data[(int) u32o++]);
                Verts[(int)i] = v;
            }
        }

        protected void BuildVertices_Thin(ref CUberData data)
        {
            uint i;
            Verts.Capacity = (int) VertexCount;
            HasUV0 = true;

            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int)i];
                v.Position = data.GetVec3(LookupOffset + i);
                v.UV0 = data.Vec2Data[(int)data.U32Data[(int)u32o++]];
                Verts[(int)i] = v;
            }
        }

        protected void BuildVertices_Raw(ref CUberData data)
        {
            uint i = 0;
            Verts.Capacity = (int) VertexCount;
            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int) i];
                v.Position = data.GetVec3(LookupOffset + i);
                Verts[(int) i] = v;
            }
        }

        protected void BuildVertices_Fat(ref CUberData data)
        {
            uint i = 0;
            Verts.Capacity = (int) VertexCount;
            HasNormal = HasUV0 = HasUV1 = true;
            uint u32o = U32Offset;
            for (i = 0; i < VertexCount; i++)
            {
                if(Verts.Count <= i) { Verts.Add(new SUberVert());}
                var v = Verts[(int) i];
                v.Position = data.GetVec3(LookupOffset + i);
                v.Normal = DecodeNormal(data.U32Data[(int) u32o++]);
                v.UV0 = data.Vec2Data[(int) data.U32Data[(int) u32o++]];
                v.UV1 = data.Vec2Data[(int) data.U32Data[(int) u32o++]];
                Verts[(int) i] = v;
            }
        }

        protected uint U32PerVert(eVertexType type)
        {
            switch (type)
            {
                case eVertexType.Thin:
                    return 1;
                case eVertexType.Lit:
                case eVertexType.Unlit:
                    return 2;
                case eVertexType.Fat:
                case eVertexType.Deform:
                    return 3;
                case eVertexType.FatDeform:
                    return 4;
                case eVertexType.Raw:
                default:
                    return 0;
            }
        }

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            Console.WriteLine($"Loading {GetType().Name}");
            string s0 = r.ReadPascalStr();
            if (s0 == "") return false;
            MaterialName = s0;

            uint n = 0;
            if (!r.Get(ref Flags)) return false;
            if (!r.Get(ref ID)) return false;
            if (!r.Get(ref MeshID)) return false;
            if (!r.Get(ref LOD)) return false;

            // This is a workaround for Marshal.SizeOf not working with enums. Assign to a temporary variable of the correct byte length first.
            uint vertexTypeTemp = 0;
            if (!r.Get(ref vertexTypeTemp)) return false;
            VertexType = (eVertexType) vertexTypeTemp;

            if (!r.Get(ref BBMin)) return false;
            if (!r.Get(ref BBMax)) return false;
            if (!r.Get(ref IndexCount)) return false;

            var pIndexData = data.ReadMeshData(sizeof(UInt16) * IndexCount);
            Indices.Capacity = (int) IndexCount;
            // Convert from byte to short
            UInt16[] indices = new ushort[pIndexData.Length / 2];
            Buffer.BlockCopy(pIndexData, 0, indices, 0, pIndexData.Length);
            Indices = indices.ToList();

            uint d = 0;
            switch (VertexType)
            {
                case eVertexType.Fat:
                case eVertexType.Lit:
                case eVertexType.Unlit:
                case eVertexType.Thin:
                case eVertexType.Raw:
                case eVertexType.FatDeform:
                case eVertexType.Deform:
                    if (!r.Get(ref VertexCount)) return false;
                    LookupOffset = data.GetLookup(VertexCount);
                    n = U32PerVert(VertexType);
                    U32Offset = n != 0 ? data.GetU32(VertexCount * n) : 0;
                    break;
                default:
                    VertexCount = 0;
                    break;
            }

            switch (VertexType)
            {
                case eVertexType.Fat:
                    BuildVertices_Fat(ref data);
                    break;
                case eVertexType.Lit:
                    BuildVerticies_Lit(ref data);
                    break;
                case eVertexType.Unlit:
                    BuildVertices_Unlit(ref data);
                    break;
                case eVertexType.Thin:
                    BuildVertices_Thin(ref data);
                    break;
                case eVertexType.Raw:
                    BuildVertices_Raw(ref data);
                    break;
                case eVertexType.Deform:
                    BuildVertices_Deform(ref data);
                    break;
                case eVertexType.FatDeform:
                    // Do nothing?
                    break;
            }

            return true;
        }
        
        public enum eVertexType : uint
        {
            None = 0,
            Lit = 1,
            Unlit = 2,
            Thin = 3,
            Raw = 4,
            FatDeform = 5,
            Fat = 6,
            Deform = 14
        }

        public struct SUberVert
        {
            internal CVec3f Position;
            internal CVec3f Normal;
            internal CVec2f UV0;
            internal CVec2f UV1;
            internal UInt32 Diffuse;
            internal Byte[] Bones;
            internal Single Weight;
        }
    }

}
