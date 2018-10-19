using System;

namespace PS1Ubr
{
    public class CBone : ILoadable
    {
        public string Name;
        public int Parent;
        public CBoneTransform Transform = new CBoneTransform();

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            string s;
            s = r.ReadPascalStr();
            if (s == "") return false;
            Name = s;

            if (!r.Get(ref Parent)) return false;
            if (!Transform.Load(ref r, ref data)) return false;

            return true;
        }
    }
}