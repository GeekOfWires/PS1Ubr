using System;
using System.Collections.Generic;
using System.Security.Policy;

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

        public bool Load()
        {
            throw new NotImplementedException();
        }

        public class CAABV
        {
            public List<CMeshItemAABV> AABVs;
            public CMeshItemAABV RootAABV;

            public bool Load()
            {
                throw new NotImplementedException();
            }

            public class CMeshItemAABV
            {
                public byte Flags;
                public CVec3f BBMin;
                public CVec3f BBMax;
                public List<UInt16> LeafIDs;
                public CMeshItemAABV pLeftNode = null;
                public CMeshItemAABV pRightNode = null;

                public bool Load()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class CMeshItem
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
            // todo: float Transform[4 * 4]; // ??
            public bool Load()
            {
                throw new NotImplementedException();
            }
        }

        public class CPortalRegion
        {
            public string Name;
            public uint ID;
            public CVec3f BBMin;
            public CVec3f BBMax;
            public List<CPortalSystemPortal> Portals;
            public List<CConvexHull> ConvexHulls;
            public List<uint> U32Array;
            public List<CPortalRegionSubStructure0> RegionSubStructs0;

            public bool Load()
            {
                throw new NotImplementedException();
            }

            public class CConvexHull
            {
                public uint A; //TODO
                public CVec3f BBMin;
                public CVec3f BBMax;
                public List<CPortalSystemPlane> Planes;
                public List<CVec3f> Verts;

                public bool Load()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class CPortalRegionSubStructure0
        {
            public float A; // todo:
            //todo: public CVec3f B[4];
            public bool Load()
            {
                throw new NotImplementedException();
            }
        }

        public class CPortalSystemPortal
        {
            public CPortalSystemPlane Plane;
            public uint A; // TODO
            // IDs of the connected regions
            public uint RegionA;
            public uint RegionB;
            //todo: public CVec3f Points[4];
            public int MeshItemID; //ID of the attached mesh ID, e. g. a door

            public bool Load()
            {
                throw new NotImplementedException();
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

            public bool Load()
            {
                throw new NotImplementedException();
            }
        }
    }
}