#include "Buffer.h"


bool CDynMemoryBuffer::Reserve(size_t size){
	size = (size+ BufferAlign -1)/BufferAlign*BufferAlign;
	void *pBuf;

	if (size <= m_Capacity){
		return true;
	}

	pBuf = MemAlloc(size);

	if (m_pBuffer){
		if (pBuf){
			memcpy(pBuf, m_pBuffer, m_Capacity);
		}
		MemFree(m_pBuffer);
	}
	if (!pBuf){
		return false;
	}
	memset(((LPBYTE)pBuf)+ m_Capacity, 0, size- m_Capacity);
	m_pBuffer = pBuf;
	m_Capacity = size;
	return true;
}
CDynMemoryBuffer::CDynMemoryBuffer(){
	m_Size = m_Capacity = m_Offset = 0;
	m_pBuffer = nullptr;
}
CDynMemoryBuffer::~CDynMemoryBuffer(){
	Clear(true);
}
void CDynMemoryBuffer::Clear(bool freedata){
	if (freedata){
		if (m_pBuffer){
			MemFree(m_pBuffer);
		}
		m_pBuffer = nullptr;
		m_Capacity = 0;
	} else {
		memset(m_pBuffer, 0, m_Capacity);
	}
	m_Size = 0;
}

bool CDynMemoryBuffer::Append(const void* pData, size_t datasize){
	size_t size;
	size = m_Size;
	if (!Resize(size + datasize)){
		return false;
	}
	memcpy(GetBytePtr()+size, pData, datasize);
	return true;
}

/*template<typename T, int align> class TArray {
private:
	CDynMemoryBuffer	m_Buffer;
public:
	TArray() : m_Buffer(sizeof(T)*align) {
	}
	~TArray(){
		m_Buffer.Clear(true);
	}
	void Reset(bool clear){
		m_Buffer.Clear(clear);
	}
	T* Get(int index){
		unsigned int count;
		if (index < 0)
			return nullptr;
		count = m_Buffer.GetSize()/sizeof(T);
		if (i >= count)
			return nullptr;
		return m_Buffer.GetBytePtr()+(i*sizeof(T));
	}
	T* Alloc(){
		unsigned int size;
		size = m_Buffer.GetSize();
		if (!m_Buffer.Alloc(size+sizeof(T)))
			return nullptr;
		return (T*)(m_Buffer.GetBytePtr()+size);
	}
	T &AllocRef(){
		return (T&)Alloc();
	}
	unsigned int GetCount(){
		return m_Buffer.GetSize()/sizeof(T);
	}
	unsigned int GetRawSize(){
		return m_Buffer.GetSize();
	}
	T* Array(){
		return (T*)m_Buffer.GetPtr();
	}
	T* AllocArray(int count){
		if (!m_Buffer.Alloc(count*sizeof(T)))
			return nullptr;
		return (T*)m_Buffer.GetPtr();
	}
};*/

/*CStringArray::CStringArray() : m_Strings(2048){
}

CStringArray::~CStringArray(){
	Clear();
}

void CStringArray::Clear(){
	m_StringPtrs.Reset(true);
	m_Strings.Clear(true);
}*/


/*void* CStringArray::GetData(){
	return m_Strings.GetPtr();
}

unsigned int CStringArray::ReturnDataSize(){
	return m_Strings.GetSize();
}*/
