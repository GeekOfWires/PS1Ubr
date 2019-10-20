using FileReaders;

namespace MPOReader_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapNumber = "04";

            // Use an absolute path to the extracted map-resources.pak location or make sure the .mpo and *.lst files are in the bin folder
            var mpoData = MPOReader.ReadMPOFile($"D:\\Planetside (Mod ready)\\Planetside", mapNumber);
        }
    }
}
