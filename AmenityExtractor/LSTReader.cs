using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PS1Ubr
{
    public static class LSTReader
    {
        public static List<GroundCover> ReadGroundCoverLST(string basePath, string mapNumber)
        {
            var allLstFiles = Directory.GetFiles(basePath, "*.lst", SearchOption.AllDirectories);
            var lst = allLstFiles.Single(x => x.EndsWith($"groundcover_map{mapNumber}.lst"));

            var groundCover = new List<GroundCover>();
            if (System.IO.File.Exists(lst))
            {
                var lstData = System.IO.File.ReadAllLines(lst);

                foreach (var data in lstData)
                {
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        var line = Regex.Replace(data, @"\s+", " ").Split(" "); // Replace extra whitespace with a single occurence
                    
                        groundCover.Add(new GroundCover
                        {
                            LstType = line[0],
                            ObjectType = line[1],
                            Id = int.Parse(line[2]),
                            AbsX = float.Parse(line[3]),
                            AbsY = float.Parse(line[4]),
                            AbsZ = float.Parse(line[5]),
                            ScaleX = float.Parse(line[6]),
                            ScaleY = float.Parse(line[7]),
                            ScaleZ = float.Parse(line[8]),
                            Pitch = int.Parse(line[9]),
                            Yaw = int.Parse(line[10]),
                            Roll = int.Parse(line[11]),
                            ObjectName = line.Length >= 13 ? line[12].Replace("\"", string.Empty) : null
                        });
                    }
                }
            }

            return groundCover;
        }

        public static (List<Pe_Hidden> peHiddens, List<Pe_Edit> peEdits, List<Pse_RelativeObject> pseRelativeObjects) ReadLSTFile(string mapResourcesFolder, string objectType, string entryLstType)
        {
            // First read the base .lst file with the pe_edit lines e.g. amp_station.lst if it exists
            var allLstFiles = Directory.GetFiles(mapResourcesFolder, "*.lst", SearchOption.AllDirectories);
            var baseLst = allLstFiles.SingleOrDefault(x => x.EndsWith($"{objectType.ToLower()}.lst"));

            var peEdits = new List<Pe_Edit>();
            var peHiddens = new List<Pe_Hidden>();

            if (System.IO.File.Exists(baseLst))
            {
                var lstData = System.IO.File.ReadAllLines(baseLst);

                foreach (var data in lstData)
                {
                    var line = data.Split(" ");
                    if (line[0] == "pe_edit")
                    {
                        peEdits.Add(new Pe_Edit
                        {
                            ObjectName = line[1].Replace("\"", string.Empty),
                            ID = int.Parse(line[2]),
                            RelX = float.Parse(line[3]),
                            RelY = float.Parse(line[4]),
                            RelZ = float.Parse(line[5]),
                            Unk1 = line[6],
                            Unk2 = line[7],
                            Unk3 = line[8],
                            AdditionalType = line.Length >= 10 ? line[9] : null
                        });
                    }
                    else if (line[0] == "pe_hidden")
                    {
                        peHiddens.Add(new Pe_Hidden
                        {
                            InstanceName = line[1]
                        });
                    }
                }
            }

            var pseRelativeObjects = new List<Pse_RelativeObject>();
            // Then read the additional .lst file for this type if applicable with the pse_relativeobject lines e.g. amp_station_3.lst
            if (entryLstType != null)
            {
                var additionalLst = allLstFiles.SingleOrDefault(x => x.EndsWith($"{entryLstType.ToLower()}.lst"));

                if (System.IO.File.Exists(additionalLst))
                {
                    var lstData = System.IO.File.ReadAllLines(additionalLst);

                    foreach (var data in lstData)
                    {
                        var line = data.Split(" ");

                        if (!string.IsNullOrWhiteSpace(data) && line.Length > 1)
                        {
                            pseRelativeObjects.Add(new Pse_RelativeObject
                            {
                                ObjectName = line[1],
                                RelX = float.Parse(line[2]),
                                RelY = float.Parse(line[3]),
                                RelZ = float.Parse(line[4]),
                                ScaleX = float.Parse(line[5]),
                                ScaleY = float.Parse(line[6]),
                                ScaleZ = float.Parse(line[7]),
                                Pitch = float.Parse(line[8]),
                                Yaw = float.Parse(line[9]),
                                Roll = float.Parse(line[10])
                            });
                        }
                    }
                }
            }

            return (peHiddens, peEdits, pseRelativeObjects);
        }
    }

    public class GroundCover
    {
        public string LstType { get; set; }
        public string ObjectType { get; set; }
        public int Id { get; set; }
        public float AbsX { get; set; }
        public float AbsY { get; set; }
        public float AbsZ { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
        public int Pitch { get; set; }
        public int Yaw { get; set; }
        public int Roll { get; set; }
        public string ObjectName { get; set; }
    }

    public class Pe_Hidden
    {
        public string InstanceName { get; set; }
    }

    public class Pe_Edit
    {
        public string ObjectName { get; set; }
        public int ID { get; set; }
        public float RelX { get; set; }
        public float RelY { get; set; }
        public float RelZ { get; set; }
        public string Unk1 { get; set; }
        public string Unk2 { get; set; }
        public string Unk3 { get; set; }
        public string AdditionalType { get; set; } // For objects such as ad billboards that have an addition type of "inside1/outside/inside2"
    }

    public class Pse_RelativeObject
    {
        public string ObjectName { get; set; }
        public float RelX { get; set; }
        public float RelY { get; set; }
        public float RelZ { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }
    }
}
