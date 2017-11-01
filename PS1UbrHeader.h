#ifndef PS1UBR_H
#define PS1UBR_H

#include <stdint.h>
#include "../Shared/Memory.h"
#include "../Shared/FileHelpers.h"
#include "../Shared/Buffer.h"
#include "../Shared/StringBuffer.h"
#include "../Shared/Array.h"
#include "../Shared/Helpers.h"
#include "../Shared/StringUtils.h"

#include "../Shared/VectorMath.h"
#include "../Shared/HashMap.h"

struct SUberHeader {
	uint8_t		FourCC[4]; //'u', 'b', 'e', 'r' //@0
	uint32_t	VersionA; //1 //@4
	uint32_t	VersionB; //1 //@8
	uint32_t	EntryCount; //Named entries //@C
	uint32_t	Vec3Count; //@10
	uint32_t	Vec2Count; //@14
	uint32_t	LookupCount; //@18
	uint32_t	U32Count; //@1C
	uint32_t	MeshDataSize; //@20 Contains indices
	uint32_t	ModelDataSize;
	uint32_t	Field_28;
	const static uint8_t FourCCValue[4];
	const static uint32_t	VersionAValue;
	const static uint32_t	VersionBValue;
};

const uint8_t	SUberHeader::FourCCValue[4] = { 'u', 'b', 'e', 'r' };
const uint32_t	SUberHeader::VersionAValue = 1;
const uint32_t	SUberHeader::VersionBValue = 1;

struct SUberEntry {
	char		Name[0x40];
	uint32_t	LookupOffset;
	uint32_t	U32Offset;
	uint32_t	MeshDataOffset;
	uint32_t	ModelDataOffset;
};
typedef CVec3f SUberVec3;
typedef CVec2f SUberVec2;

struct SUberEntry_4 {
	uint32_t	A;
};

static_assert(sizeof(SUberHeader) == 0x2C, "Uber header size");
static_assert(sizeof(SUberEntry) == 0x50, "Uber entry size");

static_assert(sizeof(SUberVec3) == 0xC, "Uber vec3");
static_assert(sizeof(SUberVec2) == 0x8, "Uber vec2");


/*
Header
Entries
Vec3s
Vec2s
Packed uint32_t (Field_18 * uint32_t)
Indices
Optionally loaded uint32_t array (NumEntries_0x4_Bytes)
Sub mesh data
*/

class CMeshSystem;

struct SUberVert {
	CVec3f		Position;
	CVec3f		Normal;
	CVec2f		UV0;
	CVec2f		UV1;
	uint32_t	Diffuse;
	uint8_t		Bones[2];
	float		Weight;
};

struct SUberTri {
	uint16_t	V[3];
};

class CUberData {
public:
	TDynArray<SUberEntry>	Entries;
	TDynArray<CVec3f>		Vec3Data;
	TDynArray<CVec2f>		Vec2Data;
	TDynArray<uint32_t>		LookupData;
	TDynArray<uint32_t>		U32Data;
	TDynArray<uint8_t>		MeshData;
	TDynArray<uint8_t>		ModelData;
	bool Load(CDynMemoryReader &r) {
		uint32_t i;
		SUberHeader hdr;
		if (!r.Get(hdr)) {
			return false;
		}
		if (memcmp(hdr.FourCC, SUberHeader::FourCCValue, sizeof(SUberHeader::FourCCValue)) != 0) {
			return false;
		}
		if (hdr.VersionA != SUberHeader::VersionAValue) {
			return false;
		}
		if (hdr.VersionB != SUberHeader::VersionBValue) {
			return false;
		}
		Entries.Resize(hdr.EntryCount);
		if (!r.GetRaw(Entries.begin(), sizeof(SUberEntry) * hdr.EntryCount)) {
			return false;
		}
		Vec3Data.Resize(hdr.Vec3Count);
		if (!r.GetRaw(Vec3Data.begin(), sizeof(CVec3f) * hdr.Vec3Count)) {
			return false;
		}
		Vec2Data.Resize(hdr.Vec2Count);
		if (!r.GetRaw(Vec2Data.begin(), sizeof(CVec2f) * hdr.Vec2Count)) {
			return false;
		}
		LookupData.Resize(hdr.LookupCount);
		if (!r.GetRaw(LookupData.begin(), sizeof(uint32_t) * hdr.LookupCount)) {
			return false;
		}
		MeshData.Resize(hdr.MeshDataSize);
		if (!r.GetRaw(MeshData.begin(), sizeof(uint8_t) * hdr.MeshDataSize)) {
			return false;
		}
		U32Data.Resize(hdr.U32Count);
		if (!r.GetRaw(U32Data.begin(), sizeof(uint32_t) * hdr.U32Count)) {
			return false;
		}
		ModelData.Resize(hdr.ModelDataSize);
		if (!r.GetRaw(ModelData.begin(), sizeof(uint8_t) * hdr.ModelDataSize)) {
			return false;
		}
		SUberEntry entry = { ".", hdr.LookupCount, hdr.U32Count, hdr.MeshDataSize, hdr.ModelDataSize };
		Entries.PushBack(entry);
		return true;
	}
	inline uint32_t NumEntries() {
		return Entries.Size() > 0 ? Entries.Size() - 1 : 0;
	};
	bool FetchMeshSystem(uint32_t Index, CMeshSystem &System);
	bool FetchMeshSystem(const char* pName, CMeshSystem &System) {
		uint32_t i = 0;
		for (i = 0; i < (uint32_t)Entries.Size(); i++) {
			if (_stricmp(Entries[i].Name, pName) == 0) {
				return FetchMeshSystem(i, System);
			}
		}
		return false;
	}
	const void* ReadMeshData(size_t n) {
		if (!m_pMeshDataCur) {
			return nullptr;
		}
		if (m_pMeshDataEnd - m_pMeshDataCur < n) {
			return nullptr;
		}
		const void* p = m_pMeshDataCur;
		m_pMeshDataCur += n;
		return p;
	}
	CVec3f GetVec3(uint32_t index) {
		return Vec3Data[LookupData[index]];
	}
	uint32_t GetLookup(uint32_t size) {
		uint32_t res = m_LookupOffset;
		m_LookupOffset += size;
		return res;
	}
	uint32_t GetU32(uint32_t size) {
		uint32_t res = m_U32Offset;
		m_U32Offset += size;
		return res;
	}
protected:
	CDynMemoryBuffer	m_ModelDataCache;
	const uint8_t*		m_pMeshDataCur = nullptr;
	const uint8_t*		m_pMeshDataEnd = nullptr;
	uint32_t			m_LookupOffset = 0;
	uint32_t			m_U32Offset = 0;
};

enum class eVertexType : uint32_t {
	None = 0,
	Lit = 1,
	Unlit = 2,
	Thin = 3,
	Raw = 4,
	FatDeform = 5,
	Fat = 6,
	Deform = 14,
};

template<typename T> bool LoadArray(TDynArray<T> &Array, CDynMemoryReader &r, CUberData &data) {
	uint32_t i, count;
	if (!r.Get(count)) {
		return false;
	}
	Array.Resize(count);
	for (i = 0; i < count; i++) {
		if (!Array[i].Load(r, data)) {
			return false;
		}
	}
	return true;
}
template<typename T> bool LoadPrimitiveArray(TDynArray<T> &Array, CDynMemoryReader &r) {
	uint32_t i, count;
	if (!r.Get(count)) {
		return false;
	}
	Array.Resize(count);
	for (i = 0; i < count; i++) {
		if (!r.Get(Array[i])) {
			return false;
		}
	}
	return true;
}
bool LoadStringArray(TDynArray<CStringA> &Array, CDynMemoryReader &r) {
	uint32_t i, count;
	if (!r.Get(count)) {
		return false;
	}
	Array.Resize(count);
	const char* s;
	for (i = 0; i < count; i++) {

		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Array[i] = s;
	}
	return true;
}


#define MESHSECTION_FLAG_TRISTRIP	2

class CMeshSection {
protected:
	CVec3f DecodeNormal(uint32_t n) {
		int32_t x = ((int32_t)((n >> 20) & 0x3FF)) - 0x200;
		int32_t y = ((int32_t)((n >> 10) & 0x3FF)) - 0x200;
		int32_t z = ((int32_t)((n) & 0x3FF)) - 0x200;
		return CVec3f(
			(float)x * 1.0f / 512.0f,
			(float)y * 1.0f / 512.0f,
			(float)z * 1.0f / 512.0f);
	}
	void BuildVertices_Lit(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);
		HasColor = HasUV0 = true;

		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
			v.Diffuse = data.U32Data[u32o++];
			v.UV0 = data.Vec2Data[data.U32Data[u32o++]];
		}
	}
	void BuildVertices_Unlit(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);
		HasNormal = HasUV0 = true;

		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
			v.Normal = DecodeNormal(data.U32Data[u32o++]);
			v.UV0 = data.Vec2Data[data.U32Data[u32o++]];
		}
	}

	void DecodeDeform(SUberVert&v, uint32_t val) {
		v.Bones[0] = (val >> 8) & 0xFF;
		v.Bones[1] = (val) & 0xFF;
		int16_t w = (int16_t)(val >> 16);
		v.Weight = (float)w / 16384.0f;
	}
	void BuildVertices_Deform(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);
		HasNormal = HasUV0 = HasSkin = true;

		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
			v.Normal = DecodeNormal(data.U32Data[u32o++]);
			v.UV0 = data.Vec2Data[data.U32Data[u32o++]];
			DecodeDeform(v, data.U32Data[u32o++]);
		}
	}
	void BuildVertices_Thin(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);
		HasUV0 = true;

		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
			v.UV0 = data.Vec2Data[data.U32Data[u32o++]];
		}
	}
	void BuildVertices_Raw(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);

		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
		}
	}
	void BuildVertices_Fat(CUberData &data) {
		uint32_t i;
		Verts.Resize(VertexCount);
		HasNormal = HasUV0 = HasUV1 = true;
		uint32_t u32o = U32Offset;
		for (i = 0; i < VertexCount; i++) {
			SUberVert &v = Verts[i];
			v.Position = data.GetVec3(LookupOffset + i);
			v.Normal = DecodeNormal(data.U32Data[u32o++]);
			v.UV0 = data.Vec2Data[data.U32Data[u32o++]];
			v.UV1 = data.Vec2Data[data.U32Data[u32o++]];
		}
	}
	uint32_t		LookupOffset;
	uint32_t		U32Offset;
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

	}
public:
	CStringA		MaterialName;

	uint32_t		Flags = 0; //2 = tristrip
	uint32_t		ID = 0;
	uint32_t		MeshID = 0;
	uint32_t		LOD = 0;
	eVertexType		VertexType = eVertexType::None;
	CVec3f			BBMin;
	CVec3f			BBMax;
	uint32_t		IndexCount = 0;
	uint32_t		VertexCount = 0;
	TDynArray<SUberVert>	Verts;
	TDynArray<uint16_t>		Indices;
	//Internal
	bool			HasNormal = false;
	bool			HasUV0 = false;
	bool			HasUV1 = false;
	bool			HasColor = false;
	bool			HasSkin = false;

	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s0;
		s0 = r.ReadPascalStr();
		if (!s0) {
			return false;
		}
		MaterialName = s0;
		uint32_t n;

		if (!r.Get(Flags)) {
			return false;
		}
		if (!r.Get(ID)) {
			return false;
		}
		if (!r.Get(MeshID)) {
			return false;
		}
		if (!r.Get(LOD)) {
			return false;
		}
		if (!r.Get(VertexType)) {
			return false;
		}
		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}
		if (!r.Get(IndexCount)) {
			return false;
		}

		const void* pIndexData = data.ReadMeshData(sizeof(uint16_t)*IndexCount);
		if (!pIndexData) {
			return false;
		}
		Indices.Resize(IndexCount);
		memcpy(Indices.begin(), pIndexData, sizeof(uint16_t)*IndexCount);

		uint32_t d;
		switch (VertexType) {
		case eVertexType::Fat:
		case eVertexType::Lit:
		case eVertexType::Unlit:
		case eVertexType::Thin:
		case eVertexType::Raw:
		case eVertexType::FatDeform:
		case eVertexType::Deform:
			if (!r.Get(VertexCount)) {
				return false;
			}
			LookupOffset = data.GetLookup(VertexCount);
			n = U32PerVert(VertexType);
			if (n != 0) {
				U32Offset = data.GetU32(VertexCount * n);
			}
			else {
				U32Offset = 0;
			}
			break;
		default:
			VertexCount = 0;
		}
		switch (VertexType) {
		case eVertexType::Fat:
			BuildVertices_Fat(data);
			break;
		case eVertexType::Lit:
			BuildVertices_Lit(data);
			break;
		case eVertexType::Unlit:
			BuildVertices_Unlit(data);
			break;
		case eVertexType::Thin:
			BuildVertices_Thin(data);
			break;
		case eVertexType::Raw:
			BuildVertices_Raw(data);
			break;
		case eVertexType::Deform:
			BuildVertices_Deform(data);
			break;
		case eVertexType::FatDeform:
			DebugBreak();
			break;
		}

		return true;
	}
};

class CMesh {
public:
	CStringA				Name;
	CStringA				ModelName; //?
	uint32_t				LOD;
	CVec3f					BBMin;
	CVec3f					BBMax;
	uint32_t				MeshSectionCount2; //Always equal to mesh section count
	uint32_t				ID; //CMeshSection::MeshID
	uint32_t				MeshSectionCount;
	TDynArray<CMeshSection>	MeshSections;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s0, *s1;

		s0 = r.ReadPascalStr();
		if (!s0) {
			return false;
		}
		Name = s0;
		s1 = r.ReadPascalStr();
		if (!s1) {
			return false;
		}
		ModelName = s1;
		if (!r.Get(LOD)) {
			return false;
		}

		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}
		if (!r.Get(MeshSectionCount2)) {
			return false;
		}
		//Could be quality
		if (!r.Get(ID)) {
			return false;
		}
		if (!r.Get(MeshSectionCount)) {
			return false;
		}

		return true;
	}

	bool LoadMeshSections(CDynMemoryReader &r, CUberData &data) {
		uint32_t i;
		MeshSections.Resize(MeshSectionCount);

		for (CMeshSection &section : MeshSections) {
			if (!section.Load(r, data)) {
				return false;
			}
		}
		return true;
	}
};


class CPortalMesh {
public:
	CStringA	Name;
	//NOTE:These don't seem to be used, hardware breakpoints didn't fire
	float		A = 0;
	float		B = 0;
	void Clear() {
		Name = "";
		A = B = 0;
	}
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;

		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;

		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(B)) {
			return false;
		}
		return true;
	};
};

class CPortalSystemPlane {
public:
	CVec3f	Normal;
	float	Distance;
	float	E; //TODO
	bool Load(CDynMemoryReader &r, CUberData &data) {

		if (!r.Get(Normal.x)) {
			return false;
		}
		if (!r.Get(Normal.y)) {
			return false;
		}
		if (!r.Get(Normal.z)) {
			return false;
		}
		if (!r.Get(Distance)) {
			return false;
		}
		if (!r.Get(E)) {
			return false;
		}
		return true;
	}
};


class CPortalSystemPortal {
public:
	CPortalSystemPlane	Plane;
	uint32_t			A; //TODO
	//IDs of the connected regions
	uint32_t			RegionA;
	uint32_t			RegionB;
	CVec3f				Points[4];
	int32_t				MeshItemID; //ID of the attached mesh ID, e. g. a door
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t i;

		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(RegionA)) {
			return false;
		}
		if (!r.Get(RegionB)) {
			return false;
		}
		for (i = 0; i < 4; i++) {
			if (!r.Get(Points[i])) {
				return false;
			}
		}
		if (!Plane.Load(r, data)) {
			return false;
		}
		if (!r.Get(MeshItemID)) {
			return false;
		}

		return true;
	}
};


class CConvexHull {
public:
	uint32_t	A; //TODO
	CVec3f		BBMin;
	CVec3f		BBMax;
	TDynArray<CPortalSystemPlane>			Planes;
	TDynArray<CVec3f>						Verts;
	bool Load(CDynMemoryReader &r, CUberData &data) {

		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}
		if (!LoadArray(Planes, r, data)) {
			return false;
		}
		if (!LoadPrimitiveArray(Verts, r)) {
			return false;
		}
		return true;
	}
};

class CPortalRegionSubStructure0 {
public:
	float	A; //TODO
	CVec3f	B[4];
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t i;
		if (!r.Get(A)) {
			return false;
		}
		for (i = 0; i < 4; i++) {

			if (!r.Get(B[i])) {
				return false;
			}
		}
		return true;
	};
};

class CPortalRegion {
public:
	CStringA								Name;
	uint32_t								ID;
	CVec3f									BBMin;
	CVec3f									BBMax;
	TDynArray<CPortalSystemPortal>			Portals;
	TDynArray<CConvexHull>					ConvexHulls;
	TDynArray<uint32_t>						U32Array;
	TDynArray<CPortalRegionSubStructure0>	RegionSubStructs0;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!r.Get(ID)) {
			return false;
		}
		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}

		if (!LoadArray(Portals, r, data)) {
			return false;
		}

		if (!LoadArray(ConvexHulls, r, data)) {
			return false;
		}
		if (!LoadPrimitiveArray(U32Array, r)) {
			return false;
		}
		if (!LoadArray(RegionSubStructs0, r, data)) {
			return false;
		}
		return true;
	}
};



#define MESHITEM_FLAG_HIDDEN	0x100 //1 << 8
#define MESHITEM_FLAG_EDITABLE	0x200 //1 << 9

class CMeshItem {
public:
	uint32_t	A; //TODO
	uint32_t	Index;
	uint32_t	ID; //TODO, may be GUID
	uint32_t	Flags;
	uint32_t	RegionA;
	uint32_t	RegionB;
	CStringA	InstanceName;
	CStringA	AssetName;
	TDynArray<uint32_t>	Materials;
	float		Transform[4 * 4];//?
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;
		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(Index)) {
			return false;
		}
		if (!r.Get(ID)) {
			return false;
		}
		if (!r.Get(Flags)) {
			return false;
		}
		if (!r.Get(RegionA)) {
			return false;
		}
		if (!r.Get(RegionB)) {
			return false;
		}
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		InstanceName = s;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		AssetName = s;
		if (!LoadPrimitiveArray(Materials, r)) {
			return false;
		}
		if (!r.Get(Transform)) {
			return false;
		}
		return true;
	}
};

#define MESHITEMAABV_FLAG_LEAF	0x01
#define MESHITEMAABV_FLAG_LEFT	0x10
#define MESHITEMAABV_FLAG_RIGHT	0x20

class CMeshItemAABV {
public:
	uint8_t				Flags;
	CVec3f				BBMin;
	CVec3f				BBMax;
	TDynArray<uint16_t>	LeafIDs;
	CMeshItemAABV*		pLeftNode = nullptr;
	CMeshItemAABV*		pRightNode = nullptr;

	bool Load(CDynMemoryReader &r, CUberData &data, TDynArray<CMeshItemAABV> &AABVs, uint32_t &Index) {
		uint16_t NumLeafs;
		if (!r.Get(Flags)) {
			return false;
		}
		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}
		//Leaf
		if ((Flags & MESHITEMAABV_FLAG_LEAF) != 0) {
			if (!r.Get(NumLeafs)) {
				return false;
			}
			LeafIDs.Resize(NumLeafs);
			if (!r.GetRaw(LeafIDs.begin(), sizeof(uint16_t) * NumLeafs)) {
				return false;
			}
		}
		if ((Flags & MESHITEMAABV_FLAG_LEFT) != 0) {
			pLeftNode = &AABVs[Index++];
			if (!pLeftNode->Load(r, data, AABVs, Index)) {
				return false;
			}
		}
		if ((Flags & MESHITEMAABV_FLAG_RIGHT) != 0) {
			pRightNode = &AABVs[Index++];
			if (!pRightNode->Load(r, data, AABVs, Index)) {
				return false;
			}
		}

		return true;
	}
};
class CAABV {
public:
	TDynArray<CMeshItemAABV>	AABVs;
	CMeshItemAABV				RootAABV;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t n;

		AABVs.ClearFast();
		RootAABV.LeafIDs.ClearFast();
		RootAABV.pLeftNode = nullptr;
		RootAABV.pRightNode = nullptr;
		if (!r.Get(n)) {
			return false;
		}

		if (n != 0) {

			AABVs.Resize(n);

			uint32_t index = 0;
			return RootAABV.Load(r, data, AABVs, index);
		}
		return true;
	}

};

class CMeshSystem;

class CPortalSystem {
public:
	CVec3f		BBMin;
	CVec3f		BBMax;

	CPortalMesh	PortalMesh0;
	CPortalMesh	PortalMesh1;
	CPortalMesh	PortalMesh2;
	CPortalMesh	PortalMesh3;
	CPortalMesh	PortalMesh4;

	TDynArray<CPortalSystemPortal>			ExteriorPortals;
	TDynArray<CPortalRegion>				Regions;
	TDynArray<CStringA>						Strings;
	TDynArray<CMeshItem>					MeshItems;
	//These come from <assetname>.lst
	TDynArray<CMeshItem>					AdditionalMeshItems;
	TDynArray<uint32_t>						U32s;
	TDynArray<CPortalRegionSubStructure0>	SubStructs1; //Never used
	bool									Loaded;

	CAABV									AABV;

	CMeshSystem*							MeshSystem = nullptr;

	void Clear() {
		ExteriorPortals.ClearFast();
		Regions.ClearFast();
		Strings.ClearFast();
		MeshItems.ClearFast();
		U32s.ClearFast();
		SubStructs1.ClearFast();

		PortalMesh0.Clear();
		PortalMesh1.Clear();
		PortalMesh2.Clear();
		PortalMesh3.Clear();
		PortalMesh4.Clear();

		Loaded = false;
	}
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t n, i;
		const char* s;
		Clear();
		if (!r.Get(n)) {
			return false;
		}
		if (n == 0) {
			return true;
		}

		if (!r.Get(BBMin)) {
			return false;
		}
		if (!r.Get(BBMax)) {
			return false;
		}


		if (!PortalMesh0.Load(r, data)) {
			return false;
		}
		if (!PortalMesh1.Load(r, data)) {
			return false;
		}
		if (!PortalMesh2.Load(r, data)) {
			return false;
		}
		if (!PortalMesh3.Load(r, data)) {
			return false;
		}
		if (!PortalMesh4.Load(r, data)) {
			return false;
		}


		if (!LoadArray(ExteriorPortals, r, data)) {
			return false;
		}
		if (!LoadArray(Regions, r, data)) {
			return false;
		}
		if (!LoadStringArray(Strings, r)) {
			return false;
		}
		if (!r.Get(n)) {
			return false;
		}

		CAABV aabv;
		if (n != 0) {
			MeshItems.Resize(n);
			for (i = 0; i < n; i++) {
				if (!MeshItems[i].Load(r, data)) {
					return false;
				}
			}
			if (!r.Get(n)) {
				return false;
			}
			if (n != 0) {
				if (!AABV.Load(r, data)) {
					return false;
				}
			}
			for (i = 0; i < (uint32_t)Regions.Size(); i++) {
				uint32_t PortalID;
				uint32_t HasAABV;
				if (!r.Get(PortalID)) {
					return false;
				}
				if (!r.Get(HasAABV)) {
					return false;
				}
				if (HasAABV != 0) {
					if (!aabv.Load(r, data)) {
						return false;
					}
				}
			}
		}


		if (!LoadPrimitiveArray(U32s, r)) {
			return false;
		}
		if (U32s.Size() != 0) {
			if (!r.Get(n)) {
				return false;
			}
			if (n != 0) {
				if (!aabv.Load(r, data)) {
					return false;
				}
			}
		}
		if (!LoadArray(SubStructs1, r, data)) {
			return false;
		}
		Loaded = true;

		return true;
	}
};

#define BONE_FLAG_POSITION	1
#define BONE_FLAG_ROTATION	2
#define BONE_FLAG_XFORM		4

class CBoneTransform {
public:
	uint32_t	Flags; //Flags indicate non-identity for the given component
	CVec3f		Position;
	CVec4f		RotationQuat;
	float		RotationMatrix[3 * 3]; //Scale xform?
	bool Load(CDynMemoryReader &r, CUberData &data) {
		if (!r.Get(Flags)) {
			return false;
		}
		if (!r.Get(Position)) {
			return false;
		}
		if (!r.Get(RotationQuat)) {
			return false;
		}
		if (!r.Get(RotationMatrix)) {
			return false;
		}

		return true;
	}
};

class CBone {
public:
	CStringA		Name;
	int32_t			Parent;
	CBoneTransform	Transform;

	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!r.Get(Parent)) {
			return false;
		}
		if (!Transform.Load(r, data)) {
			return false;
		}
		return true;
	}
};

class CExternalModelInstance {
public:
	CStringA	Name;
	int32_t		BoneIndex;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;
		uint32_t n;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!r.Get(BoneIndex)) {
			return false;
		}
		return true;
	}
};
class CSkeleton {
public:
	CStringA							Name;
	TDynArray<CBone>					Bones;
	TDynArray<CExternalModelInstance>	ExternalInstances;

	bool Load(CDynMemoryReader &r, CUberData &data) {
		const char* s;
		uint32_t n;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!LoadArray(Bones, r, data)) {
			return false;
		}
		if (!r.Get(n)) {
			return false;
		}
		if (n != 0) {
			if (!LoadArray(ExternalInstances, r, data)) {
				return false;
			}
		}

		return true;
	}
};
#pragma pack(push, 2)

struct SAABFace {
	uint16_t	MeshID;
	uint16_t	MeshSectionID;
	uint16_t	V0;
	uint16_t	V1;
	uint16_t	V2;
};

#define AABBNODE_FLAG_LEAF			0x01
#define AABBNODE_FLAG_X_SPLIT		0x02
#define AABBNODE_FLAG_Y_SPLIT		0x04
#define AABBNODE_FLAG_Z_SPLIT		0x08
#define AABBNODE_FLAG_LEFT_CHILD	0x10
#define AABBNODE_FLAG_RIGHT_CHILD	0x20

/*
Seems to continue process while the max side length of a node is >= 2.0f
*/

struct SAABBNode {
	uint16_t	Flags; //2 - 1 = has leaves
	uint16_t	FaceCount; //4
	CVec3f		BBMin; //10
	CVec3f		BBMax; //1C
	uint32_t	FaceOffset; //20
	uint32_t	LeftChild; //24
	uint32_t	RightChild; //28
};

#pragma pack(pop)

class CAAB {
public:
	uint32_t	FaceCount;
	uint32_t	NodeCount;
	uint32_t	FaceIndexMapCount; //Maps the face indices used by nodes to face array indices
	TDynArray<SAABFace>		Faces;
	TDynArray<SAABBNode>	Nodes;
	TDynArray<uint32_t>		FaceIndexMap;
	SAABBNode				RootNode;
	void Clear() {
		FaceCount = NodeCount = FaceIndexMapCount = 0;
		Faces.ClearFast();
		Nodes.Clear();
	}
	bool Load(CDynMemoryReader &r, CUberData &data) {
		const void* d;
		if (!r.Get(FaceCount)) {
			return false;
		}
		if (FaceCount) {
			d = data.ReadMeshData(FaceCount * sizeof(SAABFace)); //uint32_t, uint16_t, uint16_t, uint16_t
			if (d == nullptr) {
				return false;
			}
			Faces.Resize(FaceCount);
			memcpy(Faces.begin(), d, sizeof(SAABFace)*FaceCount);
		}

		if (!r.Get(NodeCount)) {
			return false;
		}

		d = data.ReadMeshData(0x28);
		if (d == nullptr) {
			return false;
		}
		memcpy(&RootNode, d, sizeof(SAABBNode));
		if (NodeCount) {
			d = data.ReadMeshData(NodeCount * sizeof(SAABBNode));
			if (d == nullptr) {
				return false;
			}
			Nodes.Resize(NodeCount);
			memcpy(Nodes.begin(), d, sizeof(SAABBNode)*NodeCount);
		}

		if (!r.Get(FaceIndexMapCount)) {
			return false;
		}
		if (FaceIndexMapCount) {
			d = data.ReadMeshData(FaceIndexMapCount * sizeof(uint32_t));
			if (d == nullptr) {
				return false;
			}
			FaceIndexMap.Resize(FaceIndexMapCount);
			memcpy(FaceIndexMap.begin(), d, sizeof(uint32_t)*FaceIndexMapCount);
		}

		return true;
	}
};

enum class eCollisionType : int32_t {
	None = -1,
	Sphere = 0,
	Box = 1,
	OOBB = 6,
	Cylinder = 2, //?
	Mesh = 3,
	Undefined = 4,
};

class CCollisionPartSphere {
public:
	CVec3f	Center;
	float	Radius;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		if (!r.Get(Center)) {
			return false;
		}
		if (!r.Get(Radius)) {
			return false;
		}
		return true;
	}
};

class CCollisionPartBox {
public:
	CVec3f	Min;
	CVec3f	Max;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		if (!r.Get(Min)) {
			return false;
		}
		if (!r.Get(Max)) {
			return false;
		}
		return true;
	}
};
class CCollisionPartCylinder {
public:
	CVec3f	Center;
	float	Length;
	float	Radius;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		if (!r.Get(Center)) {
			return false;
		}
		if (!r.Get(Length)) {
			return false;
		}
		if (!r.Get(Radius)) {
			return false;
		}
		return true;
	}
};

class CCollisionPartOOBB {
public:
	float	Matrix[4 * 4];
	CVec3f	ExtendMin;
	CVec3f	ExtendMax;
	float	A; //TODO
	float	B; //TODO
	bool Load(CDynMemoryReader &r, CUberData &data) {
		if (!r.Get(Matrix)) {
			return false;
		}
		if (!r.Get(ExtendMin)) {
			return false;
		}
		if (!r.Get(ExtendMax)) {
			return false;
		}
		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(B)) {
			return false;
		}
		return true;
	}
};
class CCollisionPartMesh {
public:
	struct STri {
		CVec3f	Verts[3];
	};
	CVec3f	BBoxMin;
	CVec3f	BBoxMax;
	TDynArray<STri>		Tris;
	TDynArray<CVec3f>	Normals;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t n;
		if (!r.Get(n)) {
			return false;
		}
		if (!r.Get(BBoxMin)) {
			return false;
		}
		if (!r.Get(BBoxMax)) {
			return false;
		}
		Tris.Resize(n);
		Normals.Resize(n);
		if (!r.GetRaw(Tris.begin(), sizeof(STri) * n)) {
			return false;
		}
		if (!r.GetRaw(Normals.begin(), sizeof(CVec3f) * n)) {
			return false;
		}
		return true;
	}
};

class CCollisionPart {
protected:
	union {
		CCollisionPartSphere*	pSphere;
		CCollisionPartBox*		pBox;
		CCollisionPartMesh*		pMesh;
		CCollisionPartCylinder*	pCylinder;
		CCollisionPartOOBB*		pOOBB;
	};
	void Clear() {
		switch (Type) {
		case eCollisionType::Sphere:
			if (pSphere != nullptr) {
				delete pSphere;
				pSphere = nullptr;
			}
			break;
		case eCollisionType::Box:
			if (pBox != nullptr) {
				delete pBox;
				pBox = nullptr;
			}
			break;
		case eCollisionType::Mesh:
			if (pMesh != nullptr) {
				delete pMesh;
				pMesh = nullptr;
			}
			break;
		case eCollisionType::Cylinder:
			if (pCylinder != nullptr) {
				delete pCylinder;
				pCylinder = nullptr;
			}
			break;
		case eCollisionType::OOBB:
			if (pOOBB != nullptr) {
				delete pOOBB;
				pOOBB = nullptr;
			}
			break;
		case eCollisionType::Undefined:
			break;
		};
		Type = eCollisionType::None;
	}

	eCollisionType	Type = eCollisionType::None;
public:
	CStringA	Name;
	CVec3f		BBoxMin;
	CVec3f		BBoxMax;

	CCollisionPart() {
	}
	~CCollisionPart() {
		Clear();
	}
	CCollisionPart &operator=(const CCollisionPart &b) = delete;
	CCollisionPart(const CCollisionPart &b) = delete;
	CCollisionPart(CCollisionPart &&b) {
		switch (b.Type) {
		case eCollisionType::Sphere:
			pSphere = b.pSphere;
			break;
		case eCollisionType::Box:
			pBox = b.pBox;
			break;
		case eCollisionType::Mesh:
			pMesh = b.pMesh;
			break;
		case eCollisionType::Cylinder:
			pCylinder = b.pCylinder;
			break;
		case eCollisionType::OOBB:
			pOOBB = b.pOOBB;
			break;
		case eCollisionType::Undefined:
			break;
		};
		Type = b.Type;
		b.Type = eCollisionType::None;
		Name = move(b.Name);
		BBoxMin = b.BBoxMin;
		BBoxMax = b.BBoxMax;
	}
	CCollisionPart &operator=(CCollisionPart &&b) {
		if (this != &b) {
			Clear();
			switch (b.Type) {
			case eCollisionType::Sphere:
				pSphere = b.pSphere;
				break;
			case eCollisionType::Box:
				pBox = b.pBox;
				break;
			case eCollisionType::Mesh:
				pMesh = b.pMesh;
				break;
			case eCollisionType::Cylinder:
				pCylinder = b.pCylinder;
				break;
			case eCollisionType::OOBB:
				pOOBB = b.pOOBB;
				break;
			case eCollisionType::Undefined:
				break;
			};
			Type = b.Type;
			b.Type = eCollisionType::None;
			Name = move(b.Name);
			BBoxMin = b.BBoxMin;
			BBoxMax = b.BBoxMax;
		}
		return *this;
	}
	bool Load(CDynMemoryReader &r, CUberData &data) {
		Clear();
		const char* s;
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!r.Get(Type)) {
			return false;
		}
		if (!r.Get(BBoxMin)) {
			return false;
		}
		if (!r.Get(BBoxMax)) {
			return false;
		}
		switch (Type) {
		case eCollisionType::Sphere:
			pSphere = new CCollisionPartSphere();
			if (!pSphere) {
				return false;
			}
			if (!pSphere->Load(r, data)) {
				return false;
			}
			break;
		case eCollisionType::Box:
			pBox = new CCollisionPartBox();
			if (!pBox) {
				return false;
			}
			if (!pBox->Load(r, data)) {
				return false;
			}
			break;
		case eCollisionType::Mesh:
			pMesh = new CCollisionPartMesh();
			if (!pMesh) {
				return false;
			}
			if (!pMesh->Load(r, data)) {
				return false;
			}
			break;
		case eCollisionType::Cylinder:
			pCylinder = new CCollisionPartCylinder();
			if (!pCylinder) {
				return false;
			}
			if (!pCylinder->Load(r, data)) {
				return false;
			}
			break;
		case eCollisionType::OOBB:
			pOOBB = new CCollisionPartOOBB();
			if (!pOOBB) {
				return false;
			}
			if (!pOOBB->Load(r, data)) {
				return false;
			}
			break;
		case eCollisionType::Undefined:
			break;
		default:
			//Should never happen
			break;
		};

		return true;
	}
};

class CCollisionRepresentation {
public:

	CStringA	Name;
	TDynArray<CCollisionPart>	Parts;
	bool Load(CDynMemoryReader &r, CUberData &data) {
		uint32_t n;
		const char*s;
		if (!r.Get(n)) {
			return false;
		}
		if (n == 0) {
			return true;
		}
		if (!(s = r.ReadPascalStr())) {
			return false;
		}
		Name = s;
		if (!LoadArray(Parts, r, data)) {
			return false;
		}
		return true;
	}
};

#define MESHSYS_FLAG_PORTALSYSTEM	0x1 //Seen
#define MESHSYS_FLAG_VEC3ARRAY		0x4 //Seen
#define MESHSYS_FLAG_SKELETONS		0x8 //Seen
#define MESHSYS_FLAG_USERDATA		0x20
//40
#define MESHSYS_FLAG_COLLISION		0x80 //Seen
#define MESHSYS_FLAG_NO_AAB			0x2000
#define MESHSYS_FLAG_MAP_GEOMETRY	0x4000 //Seen
/*
lava09_0
lava09_1
lava09_10
lava09_2
lava09_3
lava09_7
lava09_8
lava09_9
*/
#define MESHSYS_FLAG_HAS_EF			0x8000 //Set when skeletons contain something starting with "ef_"
#define MESHSYS_FLAG_UNK2			0x10000 //LARGE_ASSET?
#define MESHSYS_FLAG_HAS_ANIM		0x20000
#define MESHSYS_FLAG_HAS_EFFECT		0x40000

class CMeshSystem {
public:
	CStringA						Name;
	TDynArray<CMesh>				MeshArray;
	THashMap<CStringA, CMesh*>		Meshes;
	uint32_t						Flags;
	CVec3f							BBMin;
	CVec3f							BBMax;
	uint32_t						A;
	uint32_t						B;

	CPortalSystem*					PortalSystem;
	TDynArray<TDynArray<CVec3f>>	Vec3Data;
	TDynArray<uint8_t>				UserData;
	TDynArray<CSkeleton>			Skeletons;
	CAAB							AAB;
	CCollisionRepresentation		Collision;
	CMesh*							LODs[10] = {
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
		nullptr,
	};
	~CMeshSystem() {
		Clear();
	}

	void Clear() {
		Meshes.ClearFast();
		MeshArray.ClearFast();

		Vec3Data.ClearFast();
		UserData.ClearFast();
		Skeletons.ClearFast();
		if (PortalSystem) {
			delete PortalSystem;
			PortalSystem = nullptr;
		}
		AAB.Clear();

		uint32_t i;
		for (i = 0; i < 10; i++) {
			LODs[i] = nullptr;
		}
	}
	bool Load(CDynMemoryReader &r, CUberData &data) {
		Clear();

		uint32_t i, n;

		if (!r.Get(Flags)) {
			return false;
		}

		if (!r.Get(BBMin)) {
			return false;
		}

		if (!r.Get(BBMax)) {
			return false;
		}

		if (!r.Get(A)) {
			return false;
		}
		if (!r.Get(B)) {
			return false;
		}

		if (!LoadArray(MeshArray, r, data)) {
			return false;
		}

		for (CMesh &m : MeshArray) {
			Meshes[m.Name] = &m;
		}

		if ((Flags & MESHSYS_FLAG_PORTALSYSTEM) != 0) {
			PortalSystem = new CPortalSystem();
			if (!PortalSystem) {
				return false;
			}
			if (!PortalSystem->Load(r, data)) {
				return false;
			}
		}
		if ((Flags & MESHSYS_FLAG_VEC3ARRAY) != 0) {
			if (!r.Get(n)) {
				return false;
			}
			Vec3Data.Resize(n);
			for (i = 0; i < n; i++) {
				if (!LoadPrimitiveArray(Vec3Data[i], r)) {
					return false;
				}
			}
		}
		if ((Flags & MESHSYS_FLAG_USERDATA) != 0) {
			if (!r.Get(n)) {
				return false;
			}

			UserData.Resize(n);
			if (!r.GetRaw(UserData.begin(), sizeof(uint8_t) * n)) {
				return false;
			}
		}
		if ((Flags & MESHSYS_FLAG_SKELETONS) != 0) {
			if (!LoadArray(Skeletons, r, data)) {
				return false;
			}
			for (const CSkeleton &skel : Skeletons) {
				for (const CBone &bone : skel.Bones) {
					if (strncmp(bone.Name(), "ef_", 3) == 0) {
						Flags |= MESHSYS_FLAG_HAS_EF;
						break;
					}
				}
				if ((Flags & MESHSYS_FLAG_HAS_EF) != 0) {
					break;
				}
			}
		}
		for (CMesh &mesh : MeshArray) {
			if (!mesh.LoadMeshSections(r, data)) {
				return false;
			}
		}
		if ((Flags & MESHSYS_FLAG_NO_AAB) == 0) {
			if (!AAB.Load(r, data)) {
				return false;
			}
		}
		if ((Flags & MESHSYS_FLAG_COLLISION) != 0) {
			if (!Collision.Load(r, data)) {
				return false;
			}
		}

		for (CMesh &m : MeshArray) {
			if (m.LOD == 14) {
				if (!LODs[0]) {
					LODs[0] = &m;
				}
			}
			else if (m.LOD >= 1001 && m.LOD <= 1008) {
				n = m.LOD - 1000;

				if (!LODs[n]) {
					LODs[n] = &m;
				}
			}

		}

		return true;
	}
};


bool CUberData::FetchMeshSystem(uint32_t Index, CMeshSystem &System) {
	if (Index >= NumEntries()) {
		return false;
	}
	SUberEntry &Entry = Entries[Index];
	SUberEntry &NextEntry = Entries[Index + 1];


	m_ModelDataCache.Resize(NextEntry.ModelDataOffset - Entry.ModelDataOffset);
	memcpy(m_ModelDataCache.GetBytePtr(), ModelData.begin() + Entry.ModelDataOffset, NextEntry.ModelDataOffset - Entry.ModelDataOffset);
	//WriteBufferToFile("I:\\PLanetside 1\\Modeldata.bin", m_ModelDataCache);
	CDynMemoryReader r(&m_ModelDataCache);

	m_pMeshDataCur = MeshData.begin() + Entry.MeshDataOffset;
	m_pMeshDataEnd = m_pMeshDataCur + (NextEntry.MeshDataOffset - Entry.MeshDataOffset);

	m_LookupOffset = Entry.LookupOffset;
	m_U32Offset = Entry.U32Offset;

	/*ProbeMeshSystem(r, Index, NextEntry.ModelDataOffset-Entry.ModelDataOffset, Entry.Name);
	return true;//*/
	if (!System.Load(r, *this)) {
		return false;
	}

	if (NextEntry.LookupOffset != m_LookupOffset) {
		DebugBreak();
	}
	if (NextEntry.U32Offset != m_U32Offset) {
		DebugBreak();
	}
	if (m_pMeshDataEnd != m_pMeshDataCur) {
		DebugBreak();
	}
	System.Name = Entry.Name;
	const char* pName = Entry.Name;
	if (
		pName[0] == 'm' &&
		pName[1] == 'a' &&
		pName[2] == 'p' &&
		isdigit(pName[3]) &&
		isdigit(pName[4])
		) {
		System.Flags |= MESHSYS_FLAG_MAP_GEOMETRY;
	}

	if (
		pName[0] == 'u' &&
		pName[1] == 'g' &&
		pName[2] == 'd' &&
		isdigit(pName[3]) &&
		isdigit(pName[4])
		) {
		System.Flags |= MESHSYS_FLAG_MAP_GEOMETRY;
	}
	if (
		pName[0] == 'l' &&
		pName[1] == 'a' &&
		pName[2] == 'v' &&
		pName[3] == 'a' &&
		isdigit(pName[4]) &&
		isdigit(pName[5])
		) {
		System.Flags |= MESHSYS_FLAG_MAP_GEOMETRY;
	}

	return true;
}
#endif //!PS1UBR_H