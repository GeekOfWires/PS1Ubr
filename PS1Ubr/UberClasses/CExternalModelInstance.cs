using System;

namespace PS1Ubr
{
    public class CExternalModelInstance : ILoadable
    {
        public string Name;
        public int BoneIndex;

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            string s;
            uint n = 0;

            s = r.ReadPascalStr();
            if (s == "") return false;
            Name = s;

            if (!r.Get(ref BoneIndex)) return false;

            return true;
        }
    }
}