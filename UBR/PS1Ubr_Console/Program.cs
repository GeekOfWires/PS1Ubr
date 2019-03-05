using System;
using System.Diagnostics;
using System.IO;

namespace PS1Ubr
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting");
            var fs = new FileStream("C:\\temp\\ubr\\uber.ubr", FileMode.Open); // Either change this to an absolute path, or make sure this file is in the bin folder before running.
            var r = new CDynMemoryReader(fs);

            var uberData = new CUberData();
            uberData.Load(ref r);

            var data = uberData.FetchMeshSystem("tech_plant".ToCharArray(), new CMeshSystem());
            Debugger.Break(); // Inspect the above returned data in a debugger

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
