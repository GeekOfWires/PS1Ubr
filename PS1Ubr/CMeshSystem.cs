using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CMeshSystem
    {
        string Name;
        List<CMesh> MeshArray;
        Dictionary<String, CMesh> Meshes;
        UInt32 Flags;
        CVec3f BBMin;
        CVec3f BBMax;
        UInt32 A;
        UInt32 B;

        CPortalSystem PortalSystem;
        List<List<CVec3f>> Vec3Data;
        List<Byte> UserData;
        List<CSkeleton> Skeletons;
        CAAB AAB;
        CCollisionRepresentation Collision;
        CMesh[] LODs = { null, null, null, null, null, null, null, null, null, null };
    }
}
