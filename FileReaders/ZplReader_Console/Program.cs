using FileReaders;
using System;

namespace ZplReader_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapNumber = "01";

            // Use an absolute path to the planetside mod ready location
            var data = ZplReader.ReadZplFile($"D:\\Planetside (Mod ready)\\Planetside", mapNumber);
        }
    }
}
