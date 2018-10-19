using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CDynMemoryBuffer
    {
        public uint m_Size { get; set; }
        //public uint m_Capacity { get; set; }
        public byte[] m_pBuffer; // void*
        //public uint m_Offset { get; set; }
        //private const uint BufferAlign = 8192;
    }
}
