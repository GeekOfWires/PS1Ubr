using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<SUberVert> Vert;
        public List<UInt16> Indices;

        public bool HasNormal = false;
        public bool HasUV0 = false;
        public bool HasUV1 = false;
        public bool HasColor = false;
        public bool HasSkin = false;

        protected uint LookupOffset;
        protected uint U32Offset;
        //todo: protected uint U32PerVert
        /*
        uint32_t U32PerVert(eVertexType type) {
		switch (type) {
		case eVertexType::Lit:
		case eVertexType::Unlit:
			return 2;
		case eVertexType::Thin:
			return 1;
		case eVertexType::Raw:
		default:
			return 0;
		case eVertexType::FatDeform:
			return 4;
		case eVertexType::Fat:
		case eVertexType::Deform:
			return 3;
		}
         */

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

        // Incomplete
        protected void BuildVerticies_Lit(CUberData data)
        {
            uint i;
            throw new NotImplementedException();
        }

        protected void BuildVertices_Unlit(CUberData data)
        {
            throw new NotImplementedException();
        }

        protected void DecodeDeform(SUberVert v, uint val)
        {
            throw new NotImplementedException();
        }

        protected void BuildVertices_Deform(CUberData data)
        {
            throw new NotImplementedException();
        }

        protected void BuildVertices_Thin(CUberData data)
        {
            throw new NotImplementedException();
        }

        protected void BuildVertices_Raw(CUberData data)
        {
            throw new NotImplementedException();
        }

        protected void BuildVertices_Fat(CUberData data)
        {
            throw new NotImplementedException();
        }

        public bool Load()
        {
            throw new NotImplementedException();
        }

        public enum eVertexType
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
