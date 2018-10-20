using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS1Ubr
{
    public static class UBRReader
    {
        public static CUberData GetUbrData(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                var r = new CDynMemoryReader(fs);

                var uberData = new CUberData();
                uberData.Load(ref r);

                return uberData;
            }
        }

        public static CMeshSystem GetMeshSystem(string entityName, ref CUberData uberData)
        {
            return uberData.FetchMeshSystem(entityName.ToCharArray(), new CMeshSystem());
        }
    }
}
