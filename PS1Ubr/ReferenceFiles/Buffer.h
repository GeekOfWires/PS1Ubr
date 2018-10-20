#ifndef SHARED_BUFFER_H
#define SHARED_BUFFER_H

#include <stdio.h>
#include <stdint.h>
#include "String.h"
#include "StringBuffer.h"
#include "Lists.h"
#include "Helpers.h"

//CDynMemoryBuffer


//Enables use of VirtualAlloc/VirtualFree instead of malloc/free, allowing execution on the data
//#define USE_VALLOC

class CDynMemoryBuffer {
private:
	size_t	m_Size;
	size_t	m_Capacity;
	void*	m_pBuffer;
	size_t	m_Offset;

	const size_t BufferAlign = 8192;

public:
	CDynMemoryBuffer();
	~CDynMemoryBuffer();



	CDynMemoryBuffer(const CDynMemoryBuffer &b) : 
		m_Size(0), m_Capacity(0), m_pBuffer(nullptr), m_Offset(0){
		Resize(b.GetSize());
		memcpy(GetPtr(), b.GetPtr(), m_Size);
	}
	CDynMemoryBuffer(CDynMemoryBuffer &&b){
		m_Size = b.m_Size;
		m_Capacity = b.m_Capacity;
		m_pBuffer = b.m_pBuffer;
		m_Offset = b.m_Offset;

		b.m_Size = 0;
		b.m_Capacity = 0;
		b.m_pBuffer = nullptr;
		b.m_Offset = 0;
	}
	CDynMemoryBuffer &operator=(const CDynMemoryBuffer&b){
		if (this != &b){
			Resize(b.GetSize());
			memcpy(GetPtr(), b.GetPtr(), m_Size);
		}
		return *this;
	}
	CDynMemoryBuffer &operator=(CDynMemoryBuffer&&b){
		if (this != &b){
			size_t Size = b.m_Size;
			size_t Capacity = b.m_Capacity;
			void* pBuffer = b.m_pBuffer;
			size_t Offset = b.m_Offset;

			b.m_Size = m_Size;
			b.m_Capacity = m_Capacity;
			b.m_pBuffer = m_pBuffer;
			b.m_Offset = m_Offset;

			m_Size = Size;
			m_Capacity = Capacity;
			m_pBuffer = pBuffer;
			m_Offset = Offset;
		}
		return *this;
	}

	void Clear(bool freedata = false);
	bool Reserve(size_t size);
	inline size_t GetCapacity() const;
	inline size_t GetSize() const;
	inline bool Resize(size_t length);
	inline void* GetPtr();
	inline uint8_t* GetBytePtr();
	inline const void* GetPtr() const;
	inline const uint8_t* GetBytePtr() const;
	bool Append(const void* pData, size_t datasize);
	inline operator void*();
	inline operator const void*() const;

	
};



inline size_t CDynMemoryBuffer::GetCapacity() const{
	return m_Capacity;
}

inline size_t CDynMemoryBuffer::GetSize() const{
	return m_Size;
}

inline bool CDynMemoryBuffer::Resize(size_t size){
	if (size > m_Capacity && !Reserve(size)){
		return false;
	}
	m_Size = size;
	return true;
}

inline void* CDynMemoryBuffer::GetPtr(){
	return m_pBuffer;
}
inline uint8_t* CDynMemoryBuffer::GetBytePtr(){
	return (uint8_t*)m_pBuffer;
}


inline const void* CDynMemoryBuffer::GetPtr() const {
	return m_pBuffer;
}
inline const uint8_t* CDynMemoryBuffer::GetBytePtr() const{
	return (const uint8_t*)m_pBuffer;
}
inline CDynMemoryBuffer::operator void*(){
	return m_pBuffer;
}
inline CDynMemoryBuffer::operator const void*() const{
	return m_pBuffer;
}


class CDynMemoryReader {
private:
	const CDynMemoryBuffer*	m_pBuffer;
	size_t					m_Offset;
	CStringBufferA			m_BufA;
	CStringBufferW			m_BufW;
public:
	CDynMemoryReader(const CDynMemoryBuffer* pBuffer){
		m_Offset = 0;
		m_pBuffer = pBuffer;
	}
	template<typename T> bool Get(T &p){
		if (m_pBuffer->GetSize() < m_Offset + sizeof(T)){
			return false;
		}
		memcpy(&p, m_pBuffer->GetBytePtr() + m_Offset, sizeof(T));
		m_Offset += sizeof(T);
		return true;
	}
	template<typename T> bool Peek(T &p){
		if (m_pBuffer->GetLength() < m_Offset + sizeof(T)){
			return false;
		}
		memcpy(&p, m_pBuffer->GetBytePtr() + m_Offset, sizeof(T));
		return true;
	}
	size_t GetLength(){
		return m_pBuffer->GetSize();
	}
	/**/
	void Align(size_t align){
		size_t offset;
		if (align == 0){
			return;
		}
		offset = (m_Offset + (align - 1)) / align*align;
		if (offset > m_pBuffer->GetSize()){
			offset = m_pBuffer->GetSize();
		}
		m_Offset = offset;
	}
	const void* GetPtr(size_t size){
		if (m_pBuffer->GetSize() < m_Offset + size){
			return nullptr;
		}
		m_Offset += size;
		return m_pBuffer->GetBytePtr() + m_Offset - size;
	}
	inline size_t GetOffset(){
		return m_Offset;
	}
	inline void SetOffset(size_t offset){
		if (offset > m_pBuffer->GetSize()){
			offset = m_pBuffer->GetSize();
		}
		m_Offset = offset;
	}
	inline const char* GetZTString(){
		const char* pStr;
		pStr = (const char*)GetPtr(0);
		if (pStr){
			m_Offset += (uint32_t)strlen(pStr) + 1;
		}
		return pStr;
	}
	inline bool EndOfBuffer(){
		return m_Offset == m_pBuffer->GetSize();
	}
#define READ_FUNC(_type, _name) \
	_type Get##_name(){ _type v; if (!Get(v)) v = 0; return v; };
	READ_FUNC(int8_t, Int8);
	READ_FUNC(int16_t, Int16);
	READ_FUNC(int32_t, Int32);
	READ_FUNC(int64_t, Int64);

	READ_FUNC(uint8_t, UInt8);
	READ_FUNC(uint16_t, UInt16);
	READ_FUNC(uint32_t, UInt32);
	READ_FUNC(uint64_t, UInt64);

	READ_FUNC(float, Float);
	READ_FUNC(double, Double);

	const char* ReadPascalStr(){
		uint8_t len;
		if (!Get(len)){
			return nullptr;
		}
		if (m_Offset + len + 1 > m_pBuffer->GetSize()){
			return nullptr;
		}
		const char* s = (const char*)(m_pBuffer->GetBytePtr() + m_Offset);
		if (s[len] != 0){
			return nullptr;
		}
		m_Offset += len + 1;
		return s;

	}

	bool GetRaw(void* buf, size_t size){
		if (m_pBuffer->GetSize() < (m_Offset + size)){
			return false;
		}
		memcpy(buf, m_pBuffer->GetBytePtr() + m_Offset, size);
		return m_Offset += size, true;
	}
	bool GetStringBufferA(CStringBufferA &str){
		uint32_t len;
		if (!Get(len)){
			return false;
		}
		str.Alloc(len + 1);
		str.GetBufferPtr()[len] = 0;
		if (!GetRaw(str.GetBufferPtr(), (size_t)len)){
			return false;
		}
		return true;
	}
	bool GetStringA(CStringA &str){
		uint32_t len;
		if (!Get(len)){
			return false;
		}
		m_BufA.Alloc(len + 1);
		m_BufA.GetBufferPtr()[len] = 0;
		if (!GetRaw(m_BufA.GetBufferPtr(), (size_t)len)){
			return false;
		}
		str = m_BufA;
		return true;
	}
	bool GetStringW(CStringW &str){
		uint32_t len;
		if (!Get(len)){
			return false;
		}
		m_BufW.Alloc(len + 1);
		m_BufW.GetBufferPtr()[len] = 0;
		if (!GetRaw(m_BufW.GetBufferPtr(), (uint32_t)len * sizeof(wchar_t))){
			return false;
		}
		str = m_BufW;
		return true;
	}
	bool GetBuffer(CDynMemoryBuffer &buf){
		uint32_t len;
		buf.Clear(false);
		if (!Get(len)){
			return false;
		}
		buf.Resize(len);
		return GetRaw(buf.GetBytePtr(), len);
	}
	bool SeekCur(size_t d){
		size_t no;
		no = m_Offset + d;
		if (no > m_pBuffer->GetSize())
			return false;
		m_Offset += d;
		return true;
	}
	int64_t ReadVLQI(){
		int64_t i;
		uint64_t v;
		v = ReadVLQU();
		i = v >> 1;
		if (v & 1)
			i = -i;
		return i;
	}
	uint64_t ReadVLQU(){
		uint64_t v;
		uint8_t c;
		v = 0;
		do {
			Get(c);
			v = (v << 7) + (c & 0x7F);
		} while (c & 0x80);
		return v;
	}
	uint32_t ReadVLQS(){
		uint64_t v;
		v = ReadVLQU();
		if (v == 0){
			return 0xFFFFFFFF;
		}
		return (uint32_t)(v - 1);
	}
	template<typename T> bool GetSwapped(T* v){
		uint32_t i;
		uint8_t buf[sizeof(T)];
		uint8_t* p;
		if (!GetRaw(buf, sizeof(T))){
			return false;
		}
		p = (uint8_t*)v;
		for (i = 0; i < sizeof(T); i++){
			p[i] = buf[sizeof(T) - i - 1];
		}
		return true;
	}
	bool ReadCompressedNumber(int32_t &value){
		//int32_t value;
		uint8_t v;
		bool sign;

		if (!Get(v)){
			return false;
		}
		sign = (v & 0x80) != 0;

		value = v & 0x3F; //26

		if (!(v & 0x40)){
			if (sign){
				value = -value;
			}
			return true;
		}
		if (!Get(v)){
			return false;
		}
		value |= (v & 0x7F) << (6); //19
		if (!(v & 0x80)){
			if (sign){
				value = -value;
			}
			return true;
		}
		if (!Get(v)){
			return false;
		}
		value |= (v & 0x7F) << (6 + 7); //19
		if (!(v & 0x80)){
			if (sign){
				value = -value;
			}
			return true;
		}
		if (!Get(v)){
			return false;
		}
		value |= (v & 0x7F) << (6 + 7 + 7); //12
		if (!(v & 0x80)){
			if (sign){
				value = -value;
			}
			return true;
		}
		if (!Get(v)){
			return false;
		}
		value |= (v & 0x7F) << (6 + 7 + 7 + 7); //5
		if (sign){
			value = -value;
		}
		return true;
	}
};


class CDynMemoryWriter {
private:
	CDynMemoryBuffer*	m_pBuffer;
	uint32_t			m_Offset;
public:
	CDynMemoryWriter(CDynMemoryBuffer* pBuffer){
		m_pBuffer = pBuffer;
	}
	template<typename T> bool Put(const T &p){
		return m_pBuffer->Append(&p, sizeof(T));
	}
#define WRITE_FUNC(_type, _name) \
	bool Put##_name(_type val){ return Put(val); };
	WRITE_FUNC(int8_t, Int8);
	WRITE_FUNC(int16_t, Int16);
	WRITE_FUNC(int32_t, Int32);
	WRITE_FUNC(int64_t, Int64);

	WRITE_FUNC(uint8_t, UInt8);
	WRITE_FUNC(uint16_t, UInt16);
	WRITE_FUNC(uint32_t, UInt32);
	WRITE_FUNC(uint64_t, UInt64);

	WRITE_FUNC(float, Float);
	WRITE_FUNC(double, Double);

	bool PutRaw(const void* buf, size_t size){
		if (!m_pBuffer->Reserve(m_pBuffer->GetSize() + size)){
			return false;
		}
		return m_pBuffer->Append(buf, size);
	}
	void* PutPtr(size_t size){
		PUINT8 p;
		if (!m_pBuffer->Reserve(m_pBuffer->GetSize() + size)){
			return nullptr;
		}
		p = m_pBuffer->GetBytePtr() + m_pBuffer->GetSize();
		m_pBuffer->Resize(m_pBuffer->GetSize() + size);
		return p;
	}
	bool PutStringA(const char* str){
		uint32_t len;
		if (!str){
			len = 0;
			return Put(len);
		} else {
			len = (uint32_t)strlen(str) + 1;
			if (!Put(len)){
				return false;
			}
			return PutRaw(str, len);
		}
	}
	bool PutZTString(const char* str){
		size_t len;
		if (!str){
			str = "";
		}
		len = strlen(str) + 1;
		return PutRaw(str, len);
	}
	bool PutStringW(const wchar_t* str){
		uint32_t len;
		if (!str){
			len = 0;
			return Put(len);
		} else {
			len = (uint32_t)wcslen(str) + 1;
			if (!Put(len)){
				return false;
			}
			return PutRaw(str, (uint32_t)len * sizeof(wchar_t));
		}
	}
	bool PutBuffer(CDynMemoryBuffer* buf){
		uint32_t len;
		len = (uint32_t)buf->GetSize();
		if (!Put(len)){
			return false;
		}
		if (!m_pBuffer->Reserve(m_pBuffer->GetSize() + len))
			return false;
		return PutRaw(buf->GetPtr(), (uint32_t)len);
	}
	bool PutBuffer2(const void* mem, uint32_t size){
		if (!Put(size)){
			return false;
		}
		if (!m_pBuffer->Reserve(m_pBuffer->GetSize() + size)){
			return false;
		}
		return PutRaw(mem, (uint32_t)size);
	}
};

#endif //!SHARED_BUFFER_H