using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CMeshSection
    {
        public String MaterialName;

        public UInt32 Flags = 0;
        public UInt32 ID = 0;
        public UInt32 MeshID = 0;
        public UInt32 LOD = 0;
        public eVertexType VertexType = eVertexType.None;
        public CVec3f BBMin;
        public CVec3f BBMax;
        public UInt32 IndexCount = 0;
        public UInt32 VertexCount = 0;
        public List<SUberVert> Vert;
        public List<UInt16> Indices;

        public bool HasNormal = false;
        public bool HasUV0 = false;
        public bool HasUV1 = false;
        public bool HasColor = false;
        public bool HasSkin = false;

        protected UInt32 LookupOffset;
        protected UInt32 U32Offset;

        protected CVec3f DecodeNormal(UInt32 n)
        {
            UInt32 x = ((UInt32)(n >> 20) & 0x3FF) - 0x200;
            UInt32 y = ((UInt32)(n >> 10) & 0x3FF) - 0x200;
            UInt32 z = ((UInt32)(n) & 0x3FF) - 0x200;

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
            UInt32 i;


        }
    }
}
