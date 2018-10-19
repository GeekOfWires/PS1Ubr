using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CCollisionRepresentation
    {
        public string Name;
        public List<CCollisionPart> Parts = new List<CCollisionPart>();

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            uint n = 0;
            string s;

            if (!r.Get(ref n)) return false;
            if (n == 0) return true;

            s = r.ReadPascalStr();
            if (s == "") return false;
            Name = s;

            if (!Loaders.LoadArray(ref Parts, ref r, ref data)) return false;

            return true;
        }
    }
}
