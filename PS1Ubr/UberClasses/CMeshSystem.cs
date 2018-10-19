using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PS1Ubr.UberClasses;

namespace PS1Ubr
{
    public class CMeshSystem : IDisposable
    {
        public string Name;
        public List<CMesh> MeshArray = new List<CMesh>();
        public Dictionary<string, CMesh> Meshes = new Dictionary<string, CMesh>();
        public uint Flags;
        public CVec3f BBMin;
        public CVec3f BBMax;
        public uint A;
        public uint B;

        public CPortalSystem PortalSystem = new CPortalSystem();
        public List<List<CVec3f>> Vec3Data = new List<List<CVec3f>>();
        public List<byte> UserData = new List<byte>();
        public List<CSkeleton> Skeletons = new List<CSkeleton>();
        public CAAB AAB = new CAAB();
        public CCollisionRepresentation Collision = new CCollisionRepresentation();
        public CMesh[] LODs = { null, null, null, null, null, null, null, null, null, null };

        public void Dispose()
        {
            Clear();
        }

        private void Clear()
        {
            Meshes.Clear();
            MeshArray.Clear();

            Vec3Data.Clear();
            UserData.Clear();
            Skeletons.Clear();
            PortalSystem = new CPortalSystem();

            AAB.Clear();

            for (var i = 0; i < 10; i++)
            {
                LODs[i] = null;
            }
        }

        public bool Load(ref CDynMemoryReader r, CUberData data)
        {
            Console.WriteLine($"Loading {GetType().Name}");
            Clear();

            uint i, n = 0;
            if (!r.Get(ref Flags)) return false;
            if (!r.Get(ref BBMin)) return false;
            if (!r.Get(ref BBMax)) return false;
            if (!r.Get(ref A)) return false;
            if (!r.Get(ref B)) return false;

            if (!Loaders.LoadArray(ref MeshArray, ref r, ref data)) return false;
            foreach (var mesh in MeshArray)
            {
                Meshes[mesh.Name] = mesh;
            }

            if((Flags & (uint) MeshFlags.MESHSYS_FLAG_PORTALSYSTEM) != 0)
            {
                if (!PortalSystem.Load(ref r, ref data)) return false;
            }

            if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_VEC3ARRAY) != 0)
            {
                if (!r.Get(ref n)) return false;
                Vec3Data.Capacity = (int) n;
                for (i = 0; i < n; i++)
                {
                    var element = Vec3Data[(int) i];
                    if (!Loaders.LoadPrimitiveArray(ref element, ref r)) return false;
                    Vec3Data[(int) i] = element;
                }
            }

            if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_USERDATA) != 0)
            {
                if (!r.Get(ref n)) return false;

                UserData.Capacity = (int) n;
                if (!r.GetRaw(ref UserData, sizeof(byte) * n)) return false;
            }

            if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_SKELETONS) != 0)
            {
                if (!Loaders.LoadArray(ref Skeletons, ref r, ref data)) return false;

                foreach (var skel in Skeletons)
                {
                    foreach (var bone in skel.Bones)
                    {
                        if (bone.Name.StartsWith("ef_"))
                        {
                            Flags |= (uint) MeshFlags.MESHSYS_FLAG_HAS_EF;
                            break;
                        }
                    }

                    if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_HAS_EF) != 0) break;
                }
            }

            foreach (var mesh in MeshArray)
            {
                if (!mesh.LoadMeshSections(ref r, ref data)) return false;
            }

            if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_NO_AAB) == 0)
            {
                if (!AAB.Load(ref r, ref data)) return false;
            }

            if ((Flags & (uint) MeshFlags.MESHSYS_FLAG_COLLISION) != 0)
            {
                if (!Collision.Load(ref r, ref data)) return false;
            }

            foreach (var mesh in MeshArray)
            {
                if (mesh.LOD == 14)
                {
                    if (LODs[0] == null) LODs[0] = mesh;
                }
                else if (mesh.LOD >= 1001 && mesh.LOD <= 1008)
                {
                    n = mesh.LOD - 1000;
                    if (LODs[n] == null) LODs[n] = mesh;
                }
            }

            return true;
        }
    }
}
