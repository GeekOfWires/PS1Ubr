using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using PS1Ubr.UberClasses;

namespace PS1Ubr
{
    public class CPortalSystem
    {
        public CVec3f BBMin;
        public CVec3f BBMax;

        public CPortalMesh PortalMesh0;
        public CPortalMesh PortalMesh1;
        public CPortalMesh PortalMesh2;
        public CPortalMesh PortalMesh3;
        public CPortalMesh PortalMesh4;

        public List<CPortalSystemPortal> ExteriorPortals;
        public List<CPortalRegion> Regions;
        public List<string> Strings;
        public List<CMeshItem> MeshItems;
        //These come from <assetname>.lst
        public List<CMeshItem> AdditionalMeshItems;
        public List<uint> U32s;
        public List<CPortalRegionSubStructure0> SubStructs1; //Never used
        public bool Loaded;

        public CAABV AABV;

        public CMeshSystem MeshSystem = null;

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            Console.WriteLine($"Loading {GetType().Name}");
            uint n = 0, i = 0;
            string s;

            Clear();

            if (!r.Get(ref n)) return false;
            if (n == 0) return true;

            if (!r.Get(ref BBMin)) return false;
            if (!r.Get(ref BBMax)) return false;

            if (!PortalMesh0.Load(ref r, ref data)) return false;
            if (!PortalMesh1.Load(ref r, ref data)) return false;
            if (!PortalMesh2.Load(ref r, ref data)) return false;
            if (!PortalMesh3.Load(ref r, ref data)) return false;
            if (!PortalMesh4.Load(ref r, ref data)) return false;

            if (!Loaders.LoadArray(ref ExteriorPortals, ref r, ref data)) return false;

            if (!Loaders.LoadArray(ref Regions, ref r, ref data)) return false;
            if(!Loaders.LoadStringArray(ref Strings, ref r)) return false;

            if (!r.Get(ref n)) return false;

            var aabv = new CAABV();
            if (n != 0)
            {
                MeshItems.Capacity = (int) n;
                for (i = 0; i < n; i++)
                {
                    if (!MeshItems[(int) i].Load(ref r, ref data)) return false;
                }

                if (!r.Get(ref n)) return false;
                if (n != 0)
                {
                    if (!AABV.Load(ref r, ref data)) return false;
                }
                for(i = 0; i < Regions.Count; i++)
                {
                    uint PortalID = 0;
                    uint HasAABV = 0;
                    if (!r.Get(ref PortalID)) return false;
                    if (!r.Get(ref HasAABV)) return false;
                    if (HasAABV != 0)
                    {
                        if (!aabv.Load(ref r, ref data)) return false;
                    }
                }
            }

            if (!Loaders.LoadPrimitiveArray(ref U32s, ref r)) return false;

            if (U32s.Count != 0)
            {
                if (!r.Get(ref n)) return false;
                if (n != 0)
                {
                    if (!aabv.Load(ref r, ref data)) return false;
                }
            }

            if(!Loaders.LoadArray(ref SubStructs1, ref r, ref data))

            Loaded = true;

            return true;
        }

        public class CAABV
        {
            public List<CMeshItemAABV> AABVs;
            public CMeshItemAABV RootAABV;

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                uint n = 0;

                AABVs.Clear();
                RootAABV.LeafIDs.Clear();
                RootAABV.pLeftNode = null;
                RootAABV.pRightNode = null;

                if (!r.Get(ref n)) return false;

                if (n != 0)
                {
                    AABVs.Capacity = (int) n;
                    uint index = 0;
                    return RootAABV.Load(ref r, ref data, ref AABVs, ref index);
                }

                return true;
            }

            public class CMeshItemAABV
            {
                public byte Flags;
                public CVec3f BBMin;
                public CVec3f BBMax;
                public List<UInt16> LeafIDs;
                public CMeshItemAABV pLeftNode = null;
                public CMeshItemAABV pRightNode = null;

                public bool Load(ref CDynMemoryReader r, ref CUberData data, ref List<CMeshItemAABV> AABVs, ref uint index)
                {
                    UInt16 NumLeafs = 0;
                    if (!r.Get(ref Flags)) return false;
                    if (!r.Get(ref BBMin)) return false;
                    if (!r.Get(ref BBMax)) return false;

                    if ((Flags & (int) CMeshItemAABVFlags.MESHITEMAABV_FLAG_LEAF) != 0)
                    {
                        if (!r.Get(ref NumLeafs)) return false;
                        LeafIDs.Capacity = NumLeafs;
                        if (!r.GetRaw(ref LeafIDs, NumLeafs)) return false;
                    }

                    if ((Flags & (int) CMeshItemAABVFlags.MESHITEMAABV_FLAG_LEFT) != 0)
                    {
                        index++;
                        pLeftNode = AABVs[(int) index];
                        if (!pLeftNode.Load(ref r, ref data, ref AABVs, ref index)) return false;
                    }

                    if ((Flags & (int)CMeshItemAABVFlags.MESHITEMAABV_FLAG_RIGHT) != 0)
                    {
                        index++;
                        pRightNode = AABVs[(int)index];
                        if (!pRightNode.Load(ref r, ref data, ref AABVs, ref index)) return false;
                    }

                    return true;
                }
            }
        }

        public struct CMeshItem
        {
            public uint A; // TODO
            public uint Index;
            public uint ID; // TODO, may be GUID
            public uint Flags;
            public uint RegionA;
            public uint RegionB;
            public string InstanceName;
            public string AssetName;
            public List<uint> Materials;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public List<float> Transform; // This was float Transform[4 * 4]

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                string s;

                if (!r.Get(ref A)) return false;
                if (!r.Get(ref Index)) return false;
                if (!r.Get(ref ID)) return false;
                if (!r.Get(ref Flags)) return false;
                if (!r.Get(ref RegionA)) return false;
                if (!r.Get(ref RegionB)) return false;

                s = r.ReadPascalStr();
                if (s == "") return false;
                InstanceName = s;

                s = r.ReadPascalStr();
                if (s == "") return false;
                AssetName = s;

                if (!Loaders.LoadPrimitiveArray(ref Materials, ref r)) return false;

                if (!r.GetRaw(ref Transform, 4 * 4)) return false;

                return true;
            }
        }

        public class CPortalRegion : ILoadable
        {
            public string Name;
            public uint ID;
            public CVec3f BBMin;
            public CVec3f BBMax;
            public List<CPortalSystemPortal> Portals = new List<CPortalSystemPortal>();
            public List<CConvexHull> ConvexHulls = new List<CConvexHull>();
            public List<uint> U32Array = new List<uint>();
            public List<CPortalRegionSubStructure0> RegionSubStructs0 = new List<CPortalRegionSubStructure0>();

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                string s;
                s = r.ReadPascalStr();
                if (s == "") return false;
                Name = s;

                if (!r.Get(ref ID)) return false;
                if (!r.Get(ref BBMin)) return false;
                if (!r.Get(ref BBMax)) return false;
                if (!Loaders.LoadArray(ref Portals, ref r, ref data)) return false;
                if (!Loaders.LoadArray(ref ConvexHulls, ref r, ref data)) return false;
                if (!Loaders.LoadPrimitiveArray(ref U32Array, ref r)) return false;
                if (!Loaders.LoadArray(ref RegionSubStructs0, ref r, ref data)) return false;

                return true;
            }

            public class CConvexHull : ILoadable
            {
                public uint A; //TODO
                public CVec3f BBMin;
                public CVec3f BBMax;
                public List<CPortalSystemPlane> Planes = new List<CPortalSystemPlane>();
                public List<CVec3f> Verts = new List<CVec3f>();

                public bool Load(ref CDynMemoryReader r, ref CUberData data)
                {
                    if (!r.Get(ref A)) return false;
                    if (!r.Get(ref BBMin)) return false;
                    if (!r.Get(ref BBMax)) return false;
                    if (!Loaders.LoadArray(ref Planes, ref r, ref data)) return false;
                    if (!Loaders.LoadPrimitiveArray(ref Verts, ref r)) return false;

                    return true;
                }
            }
        }

        public class CPortalRegionSubStructure0 : ILoadable
        {
            public float A; //TODO

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
            public List<CVec3f> B = new List<CVec3f>(); // Was CVec3f B[4]

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                if (!r.Get(ref A)) return false;
                if (!r.GetRaw(ref B, 4)) return false;

                return true;
            }
        }

        public class CPortalSystemPortal : ILoadable
        {
            public CPortalSystemPlane Plane;
            public uint A; // TODO
            // IDs of the connected regions
            public uint RegionA;
            public uint RegionB;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
            public List<CVec3f> Points = new List<CVec3f> { Capacity =  4 };

            public int MeshItemID; //ID of the attached mesh ID, e. g. a door

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                if (!r.Get(ref A)) return false;
                if (!r.Get(ref RegionA)) return false;
                if (!r.Get(ref RegionB)) return false;
                for (int i = 0; i < 4; i++)
                {
                    var element = Points[i];
                    if (!r.Get(ref element)) return false;
                    Points[i] = element;
                }

                if (!Plane.Load(ref r, ref data)) return false;

                if (!r.Get(ref MeshItemID)) return false;

                return true;
            }
        }

        public class CPortalMesh
        {
            public string Name;
            //NOTE:These don't seem to be used, hardware breakpoints didn't fire
            public float A = 0;
            public float B = 0;

            public void Clear()
            {
                Name = "";
                A = 0;
                B = 0;
            }

            public bool Load(ref CDynMemoryReader r, ref CUberData data)
            {
                string s = r.ReadPascalStr();

                if (s == "") return false;
                Name = s;

                if (!r.Get(ref A)) return false;
                if (!r.Get(ref B)) return false;

                return true;
            }
        }
    }
}