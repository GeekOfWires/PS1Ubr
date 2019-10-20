using FileReaders.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileReaders
{

    // Reads the contents_map*.lst files to get the associated lst file for each object type
    static class LSTReader
    {
        public static List<Pse_link> ReadLSTFile(string basePath, string mapNumber)
        {
            var allLstFiles = Directory.GetFiles(basePath, "*.lst", SearchOption.AllDirectories);
            var lstFullPath = allLstFiles.Single(x => x.EndsWith($"objects_map{mapNumber}.lst"));
            var lstData = System.IO.File.ReadAllLines(lstFullPath);

            var pseLinks = new List<Pse_link>();
            foreach (var data in lstData)
            {
                var line = data.Split(" ");
                pseLinks.Add(new Pse_link()
                {
                    ObjectName = line[1],
                    LstFile = line[2]
                });
            }

            return pseLinks;
        }
    }
}
