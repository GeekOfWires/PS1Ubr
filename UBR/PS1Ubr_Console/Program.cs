using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PS1Ubr
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting");
            var fs = new FileStream("C:\\psforever\\ubr\\expansion1.ubr", FileMode.Open); // Either change this to an absolute path, or make sure this file is in the bin folder before running.
            var r = new CDynMemoryReader(fs);

            var uberData = new CUberData();
            uberData.Load(ref r);

            var data = uberData.FetchMeshSystem("vanu_core".ToCharArray(), new CMeshSystem());
            var a = data.PortalSystem.MeshItems.Where(x => x.AssetName.Contains("term")).ToList();
            Debugger.Break(); // Inspect the above returned data in a debugger

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
