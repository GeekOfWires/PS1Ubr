using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace MPOReader_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mpoData = MPOReader.MPOReader.ReadMPOFile("contents_map04.mpo"); // Change this to an absolute path or make sure the .mpo is in the bin folder
        }
    }
}
