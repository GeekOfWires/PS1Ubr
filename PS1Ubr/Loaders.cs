using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public static class Loaders
    {
        public static bool LoadArray<T>(ref List<T> array, ref CDynMemoryReader r, ref CUberData data) where T : ILoadable, new()
        {
            uint i = 0, count = 0;
            if (!r.Get(ref count)) return false;
            array.Capacity = (int)count;

            for (i = 0; i < count; i++)
            {
                if(array.Count <= i) array.Add(new T());
                if (!array[(int)i].Load(ref r, ref data)) return false;
            }

            return true;
        }

        public static bool LoadStringArray(ref List<string> array, ref CDynMemoryReader r)
        {
            uint i = 0, count = 0;
            if (!r.Get(ref count)) return false;
            array.Capacity = (int)count;
            string s;
            for (i = 0; i < count; i++)
            {
                s = r.ReadPascalStr();
                if (s == "") return false;
                if(array.Count <= i) array.Add("");
                array[(int)i] = s;
            }

            return true;
        }

        public static bool LoadPrimitiveArray<T>(ref List<T> array, ref CDynMemoryReader r) where T : struct
        {
            uint i = 0, count = 0;
            if (!r.Get(ref count)) return false;
            array.Capacity = (int)count;
            for (i = 0; i < count; i++)
            {
                if(array.Count <= i) { array.Add(new T()); }
                var element = array[(int)i];
                if (!r.Get(ref element)) return false;
                array[(int)i] = element;
            }

            return true;
        }

        public static bool LoadBytesIntoObjectList<T>(ref List<T> list, ref byte[] bytes)
        {

            var objectSize = Marshal.SizeOf(typeof(T));
            var count = bytes.Length / objectSize;

            for (int i = 0; i < count; i++)
            {
                T obj;
                var buffer = new byte[objectSize];
                Array.Copy(bytes, i * objectSize, buffer, 0, objectSize);

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                }
                finally
                {
                    handle.Free();
                }

                if(list.Count <= i) { list.Add(obj);} else { list[i] = obj; }
            }

            return true;
        }

        public static bool LoadBytesIntoObject<T>(ref T obj, ref byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return true;
        }
    }
}
