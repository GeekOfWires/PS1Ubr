using System;
using System.Collections.Generic;
using System.Text;

namespace MPOReader
{

    // Reads the contents_map*.lst files to get the associated lst file for each object type
    static class LSTReader
    {
        public static List<Pse_link> ReadLSTFile(string lstPath, string mapNumber)
        {
            var lstData = System.IO.File.ReadAllLines($"{lstPath}\\objects_map{mapNumber}.lst");

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

    public class Pse_link
    {
        public string Type => "pse_link";
        public string ObjectName { get; set; }
        public string LstFile { get; set; }
    }
}
