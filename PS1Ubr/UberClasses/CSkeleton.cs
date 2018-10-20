using System.Collections.Generic;

namespace PS1Ubr
{
    public class CSkeleton : ILoadable
    {
        public string Name;
        public List<CBone> Bones = new List<CBone>();
        public List<CExternalModelInstance> ExternalInstances = new List<CExternalModelInstance>();

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            string s;
            uint n = 0;
            s = r.ReadPascalStr();
            if (s == "") return false;
            Name = s;

            if (!Loaders.LoadArray(ref Bones, ref r, ref data)) return false;
            if (!r.Get(ref n)) return false;
            if (n != 0)
            {
                if (!Loaders.LoadArray(ref ExternalInstances, ref r, ref data)) return false;
            }

            return true;
        }
    }
}