using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public class CPortalSystemPlane
    {
        public CVec3f Normal;
        public float Distance;
        public float E; //TODO

        // TODO: Figure out CDynMemoryReader and how to reimplement it.
        //       Understood at the moment it's a sequential byte reader.
        bool Load()
        {
            throw new NotImplementedException();
        }
    }
}
