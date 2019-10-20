using FileReaders.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileReaders
{
    public class ZplReader
    {
        public static List<Zipline> ReadZplFile(string basePath, string mapNumber)
        {
            var allFiles = Directory.GetFiles(basePath, "*.zpl", SearchOption.AllDirectories);
            var fullPath = allFiles.Where(x => !x.Contains("expansion1.pak-out")).Single(x => x.EndsWith($"ugd{mapNumber}.zpl"));
            var data = File.ReadAllLines(fullPath);

            var ziplines = new List<Zipline>();
            Zipline current = null;
            foreach (var entry in data)
            {
                var line  = entry.Split(" ").Where(x => x != string.Empty).ToArray();
                if (line.Length == 0) continue;
                if (line[0] == "zip_begin_path")
                {
                    current = new Zipline();
                    current.Id = ziplines.Count() + 1;
                }

                if (line[0] == "zip_is_teleporter" && line[1] == "true")
                {
                    current.IsTeleporter = true;
                }

                if (line[0] == "zip_path_point")
                {
                    current.PathPoints.Add((float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3])));
                }

                if (line[0] == "zip_end_path")
                {
                    ziplines.Add(current);
                    current = null;
                }

            }

            return ziplines;
        }
    }
}
