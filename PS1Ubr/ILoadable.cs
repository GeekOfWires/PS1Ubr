using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Ubr
{
    public interface ILoadable
    {
        bool Load(ref CDynMemoryReader r, ref CUberData data);
    }
}
