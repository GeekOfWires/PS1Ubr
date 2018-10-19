using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PS1Ubr
{
    public class CDynMemoryReader
    {
        public byte[] m_pBuffer { get; set; }
        public uint m_Size { get; set; }
        //public uint m_Capacity { get; set; }
        public uint m_Offset { get; set; }
        public CDynMemoryReader(FileStream fs)
        {
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int) fs.Length);
            m_pBuffer = buffer;
            m_Size = (uint) fs.Length;
        }

        public CDynMemoryReader(ref CDynMemoryBuffer buffer)
        {
            m_pBuffer = buffer.m_pBuffer;
            m_Size = buffer.m_Size;
        }

        private byte[] GetBytes(int length)
        {
            var buffer = new byte[length];
            Array.Copy(m_pBuffer, (int) m_Offset, buffer, 0, length);
            m_Offset += (uint) length;
            return buffer;
        }

        public string ReadPascalStr()
        {
            byte len = new byte();
            if (!Get(ref len)) return "";

            var bytes = GetBytes(len);
            GetBytes(1); // Skip 1 byte ahead to realign data stream
            return Encoding.ASCII.GetString(bytes);
        }

        public bool Get<T>(ref T section) where T : struct
        {
            // Note: Marshal.SizeOf doesn't work with enums. Make sure you pass a temporary variable in instead, then cast it back as the original enum when the data has been returned.
            var bytes = GetBytes(Marshal.SizeOf(typeof(T)));
            T obj;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            section = obj;
            return true;
        }

        public bool GetRaw<T>(ref List<T> list, uint numOfObjects)
        {
            var objectSize = Marshal.SizeOf(typeof(T));

            for (int i = 0; i < numOfObjects; i++)
            {
                T obj;
                var bytes = GetBytes(objectSize);

                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
                    obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                }
                finally
                {
                    handle.Free();
                }

                list.Add(obj);
            }

            return true;
        }
    }
}
