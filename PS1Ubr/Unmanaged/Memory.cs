using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr.Unmanaged
{
    public class Memory
    {
        [DllImport("msvcrt.dll")]
        static extern int memcmp(byte[] one, byte[] two, UInt32 count);

        public static bool ArraysCompare(byte[] one, byte[] two)
        {
            return (one.Length == two.Length) && memcmp(one, two, (uint)(one.Length)) == 0;
        }
    }
}
