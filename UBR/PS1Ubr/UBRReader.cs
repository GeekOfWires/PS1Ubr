using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS1Ubr
{
    public static class UBRReader
    {
        private static readonly Dictionary<string, CMeshSystem> UBRCache = new Dictionary<string, CMeshSystem>();

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
            if (UBRCache.ContainsKey(entityName.ToLower())) return UBRCache[entityName.ToLower()];

            var mesh = uberData.FetchMeshSystem(entityName.ToLower().ToCharArray(), new CMeshSystem());
            UBRCache.Add(entityName.ToLower(), mesh);
            return mesh;
        }
    }
}
