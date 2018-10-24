using System.Collections.Generic;

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
