using FileReaders;

namespace MPOReader_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapNumber = "04";

            // Use an absolute path to the planetside mod ready location
            var mpoData = MPOReader.ReadMPOFile($"D:\\Planetside (Mod ready)\\Planetside", mapNumber);
        }
    }
}
