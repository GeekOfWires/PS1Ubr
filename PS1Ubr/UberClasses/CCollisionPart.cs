using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CCollisionPart : IDisposable, ILoadable
    {

        // Since c# doesn't support unions this is an approximation
        //[StructLayout(LayoutKind.Explicit)]
        //private struct CCollisionPartType
        //{
        //    [FieldOffset(0)] private CCollisionPartSphere pSphere;
        //    [FieldOffset(1)] private CCollisionPartBox pBox;
        //    [FieldOffset(2)] private CCollisionPartMesh pMesh;
        //    [FieldOffset(3)] private CCollisionPartCylinder pCylinder;
        //    [FieldOffset(4)] private CCollisionPartOOBB pOOBB;
        //}

        private CCollisionPartSphere pSphere;
        private CCollisionPartBox pBox;
        private CCollisionPartMesh pMesh;
        private CCollisionPartCylinder pCylinder;
        private CCollisionPartOOBB pOOBB;

        public string Name;
        public CVec3f BBoxMin;
        public CVec3f BBoxMax;
        public eCollisionType Type = eCollisionType.None;

        //todo: CCollisionPart operator functions
        //CCollisionPart &operator=(const CCollisionPart &b) = delete;
        //CCollisionPart(const CCollisionPart &b) = delete;
        //CCollisionPart(CCollisionPart &&b)
        //CCollisionPart &operator=(CCollisionPart &&b)

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            Clear();
            string s;
            s = r.ReadPascalStr();
            if (s == "") return false;
            
            // This is a workaround for Marshal.SizeOf not working with enums. Assign to a temporary variable of the correct byte length first.
            uint typeTemp = 0;
            if (!r.Get(ref typeTemp)) return false;
            Type = (eCollisionType) typeTemp;

            if (!r.Get(ref BBoxMin)) return false;
            if (!r.Get(ref BBoxMax)) return false;

            switch (Type)
            {
                case eCollisionType.Sphere:
                    pSphere = new CCollisionPartSphere();
                    if (!pSphere.Load(ref r, ref data)) return false;
                    break;
                case eCollisionType.Box:
                    pBox = new CCollisionPartBox();
                    if (!pBox.Load(ref r, ref data)) return false;
                    break;
                case eCollisionType.Mesh:
                    pMesh = new CCollisionPartMesh();
                    if (!pMesh.Load(ref r, ref data)) return false;
                    break;
                case eCollisionType.Cylinder:
                    pCylinder = new CCollisionPartCylinder();
                    if (!pCylinder.Load(ref r, ref data)) return false;
                    break;
                case eCollisionType.OOBB:
                    pOOBB = new CCollisionPartOOBB();
                    if (!pOOBB.Load(ref r, ref data)) return false;
                    break;
                case eCollisionType.Undefined:
                    break;
                default:
                    //Should never happen.
                    break;
            }

            return true;
        }

        public void Dispose()
        {
            Clear();
        }

        private void Clear()
        {
            switch (Type)
            {
                case eCollisionType.Sphere:
                    pSphere = null;
                    break;
                case eCollisionType.Box:
                    pBox = null;
                    break;
                case eCollisionType.Mesh:
                    pMesh = null;
                    break;
                case eCollisionType.Cylinder:
                    pCylinder = null;
                    break;
                case eCollisionType.OOBB:
                    pOOBB = null;
                    break;
                case eCollisionType.Undefined:
                    break;
            };
            Type = eCollisionType.None;
        }
    }

    internal class CCollisionPartOOBB : ILoadable
    {
        public List<float> Matrix = new List<float>(); // This was float Matrix[4 * 4]
        public CVec3f ExtendMin;
        public CVec3f ExtendMax;
        public float A; //TODO
        public float B; //TODO

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            if (!r.GetRaw(ref Matrix, 4 * 4)) return false;
            if (!r.Get(ref ExtendMin)) return false;
            if (!r.Get(ref ExtendMax)) return false;
            if (!r.Get(ref A)) return false;
            if (!r.Get(ref B)) return false;
            return true;
        }
    }

    internal class CCollisionPartCylinder : ILoadable
    {
        public CVec3f Center;
        public float Length;
        private float Radius;

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            if (!r.Get(ref Center)) return false;
            if (!r.Get(ref Length)) return false;
            if (!r.Get(ref Radius)) return false;
            return true;
        }
    }

    internal class CCollisionPartMesh : ILoadable
    {
        public struct STri
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 288)]
            private List<CVec3f> Verts; // This was Cvec3f Verts[3]
        }

        public CVec3f BBoxMin;
        public CVec3f BBoxMax;
        public List<STri> Tris = new List<STri>();
        public List<CVec3f> Normals = new List<CVec3f>();

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            uint n = 0;
            if (!r.Get(ref n)) return false;
            if (!r.Get(ref BBoxMin)) return false;
            if (!r.Get(ref BBoxMax)) return false;

            Tris.Capacity = (int) n;
            Normals.Capacity = (int) n;
            if (!r.GetRaw(ref Tris, (uint) Marshal.SizeOf(typeof(STri)) * n)) return false;
            if (!r.GetRaw(ref Normals, (uint)Marshal.SizeOf(typeof(CVec3f)) * n)) return false;

            return true;
        }
    }

    internal class CCollisionPartBox : ILoadable
    {
        public CVec3f Min;
        public CVec3f Max;

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            if (!r.Get(ref Min)) return false;
            if (!r.Get(ref Max)) return false;
            return true;
        }
    }

    internal class CCollisionPartSphere : ILoadable
    {
        public CVec3f Center;
        private float Radius;

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            if (!r.Get(ref Center)) return false;
            if (!r.Get(ref Radius)) return false;
            return true;
        }
    }

    public enum eCollisionType : int
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
