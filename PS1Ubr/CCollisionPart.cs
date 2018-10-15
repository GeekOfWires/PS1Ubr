using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CCollisionPart : IDisposable
    {

        // Since c# doesn't support unions this is an approximation
        [StructLayout(LayoutKind.Explicit)]
        private struct CCollisionPartType
        {
            [FieldOffset(0)] private CCollisionPartSphere pSphere;
            [FieldOffset(1)] private CCollisionPartBox pBox;
            [FieldOffset(2)] private CCollisionPartMesh pMesh;
            [FieldOffset(3)] private CCollisionPartCylinder pCylinder;
            [FieldOffset(4)] private CCollisionPartOOBB pOOBB;
        }

        String Name;
        CVec3f BBoxMin;
        CVec3f BBoxMax;

        //todo: CCollisionPart operator functions
        //CCollisionPart &operator=(const CCollisionPart &b) = delete;
        //CCollisionPart(const CCollisionPart &b) = delete;
        //CCollisionPart(CCollisionPart &&b)
        //CCollisionPart &operator=(CCollisionPart &&b)

        public bool Load()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Clear();
        }

        private void Clear()
        {
            throw new NotImplementedException();
        }
    }

    internal class CCollisionPartOOBB
    {
        //todo
    }

    internal class CCollisionPartCylinder
    {
        //todo
    }

    internal class CCollisionPartMesh
    {
        //todo
    }

    internal class CCollisionPartBox
    {
        //todo
    }

    internal class CCollisionPartSphere
    {
        //todo
    }

    enum eCollisionType : int
    {
        None = -1,
        Sphere = 0,
        Box = 1,
        OOBB = 6,
        Cylinder = 2, // ?
        Mesh = 3,
        Undefined = 4
    }
}
