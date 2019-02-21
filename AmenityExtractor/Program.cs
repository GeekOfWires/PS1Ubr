using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using PS1Ubr;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AmenityExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapObjects = new List<PlanetSideObject>();
            List<string> structuresWithGuids = new List<string>(File.ReadAllLines("StructuresWithGuids.txt"));
            List<string> entitiesWithGuids = new List<string>(File.ReadAllLines("EntitiesWithGuids.txt"));
            List<string> relativeObjectsToLoad = new List<string>(File.ReadAllLines("RelativeObjectsToLoad.txt"));
            IEnumerable<string> allObjectsWithGuids = structuresWithGuids.Concat(entitiesWithGuids).ToList();

            var mapNumber = "04";
            var mapResourcesFolder = "D:\\Planetside (Mod ready)\\Planetside\\map_resources.pak-out";

            // Read the contents_map mpo file, and the associated objects_map lst file
            var mpoData = MPOReader.MPOReader.ReadMPOFile(mapResourcesFolder, mapNumber);

            // Load all *.ubr files
            var uberDataList = new List<CUberData>();
            Parallel.ForEach(Directory.GetFiles("C:\\temp\\ubr\\ubr", "*.ubr"),
                file => { uberDataList.Add(UBRReader.GetUbrData(file)); });

            int id = 0;

            foreach (var entry in mpoData)
            {
                if (!allObjectsWithGuids.Contains(entry.ObjectType)) continue;
                Console.WriteLine($"Processing {entry.ObjectType}");

                // Load the relevant *.lst files for this object
                List<Pe_Edit> peEdits = new List<Pe_Edit>();
                List<Pse_RelativeObject> pseRelativeObjects = new List<Pse_RelativeObject>();
                List<Pe_Hidden> peHiddens = new List<Pe_Hidden>();
                if (entry.LstType != null)
                {
                    (peHiddens, peEdits, pseRelativeObjects) = LSTReader.ReadLSTFile(mapResourcesFolder, entry.ObjectType, entry.LstType);
                }

                // Get the root mesh for this object
                var uberData = uberDataList.Single(x => x.Entries.Select(y => y.Name).Contains(entry.ObjectType.ToLower()));
                var baseMesh = UBRReader.GetMeshSystem(entry.ObjectType, ref uberData);
                var rotationDegrees = MathFunctions.PS1RotationToDegrees(entry.HorizontalRotation);
                var baseRotationRadians = MathFunctions.DegreesToRadians(rotationDegrees);

                var entryObject = new PlanetSideObject
                {
                    Id = id,
                    ObjectName = entry.ObjectName,
                    ObjectType = entry.ObjectType,
                    AbsX = entry.HorizontalPosition,
                    AbsY = entry.VerticalPosition,
                    AbsZ = entry.HeightPosition,
                    MapID = mpoData.IndexOf(entry) + 1
                };
                mapObjects.Add(entryObject);
                id++;

                var parentRotationClockwise = MathFunctions.CounterClockwiseToClockwiseRotation(entry.HorizontalRotation);

                // Get the entities from the UBR file that would have a GUID within this object
                // If it's not an entity that would be assigned a GUID we don't care about it
                foreach (var meshItem in baseMesh.PortalSystem.MeshItems)
                {
                    if (!allObjectsWithGuids.Contains(meshItem.AssetName, StringComparer.OrdinalIgnoreCase)) continue;
                    if (peHiddens.Any(x => x.InstanceName == meshItem.InstanceName)) continue; // If a line is in the pe_hidden list it should be removed from the game world e.g. Neti pad_landing is removed where the BFR building now exists

                    var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13], baseRotationRadians);
                    var meshItemYaw = MathFunctions.TransformToRotationDegrees(meshItem.Transform);


                    // Convert from CCW to CW and apply 180 degree offset
                    var yaw = parentRotationClockwise + (360 - (180 - meshItemYaw));

                    mapObjects.Add(new PlanetSideObject
                    {
                        Id = id,
                        ObjectName = meshItem.AssetName,
                        ObjectType = meshItem.AssetName,
                        Owner = entryObject.Id,
                        AbsX = entry.HorizontalPosition + rotX,
                        AbsY = entry.VerticalPosition + rotY,
                        AbsZ = entry.HeightPosition + meshItem.Transform[14],
                        Yaw = MathFunctions.NormalizeDegrees((int)yaw)
                    });

                    id++;
                }

                foreach (var line in peEdits)
                {
                    if (!allObjectsWithGuids.Contains(line.ObjectName, StringComparer.OrdinalIgnoreCase)) continue;

                    var (rotX, rotY) = MathFunctions.RotateXY(line.RelX, line.RelY, baseRotationRadians);
                    mapObjects.Add(new PlanetSideObject
                    {
                        Id = id,
                        ObjectName = line.ObjectName,
                        ObjectType = line.ObjectName,
                        Owner = entryObject.Id,
                        AbsX = entry.HorizontalPosition + rotX,
                        AbsY = entry.VerticalPosition + rotY,
                        AbsZ = entry.VerticalPosition + line.RelZ
                    });
                    id++;
                }

                foreach (var line in pseRelativeObjects)
                {
                    if (!relativeObjectsToLoad.Contains(line.ObjectName.ToLower())) continue;
                    Console.WriteLine($"Processing sub-object {line.ObjectName}");

                    var uber = uberDataList.Single(x => x.Entries.Select(y => y.Name).Contains(line.ObjectName.ToLower()));
                    var mesh = UBRReader.GetMeshSystem(line.ObjectName, ref uber);

                    var (parentRotX, parentRotY) = MathFunctions.RotateXY(line.RelX, line.RelY, baseRotationRadians);
                    (float x, float y, float z) parentPos = (parentRotX, parentRotY, entry.VerticalPosition + line.RelZ);

                    foreach (var meshItem in mesh.PortalSystem.MeshItems)
                    {
                        if (!allObjectsWithGuids.Contains(meshItem.AssetName)) continue;

                        var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13], baseRotationRadians);
                        mapObjects.Add(new PlanetSideObject
                        {
                            Id = id,
                            ObjectName = meshItem.AssetName,
                            ObjectType = meshItem.AssetName,
                            Owner = entryObject.Id,
                            AbsX =  parentPos.x + rotX,
                            AbsY = parentPos.y + rotY,
                            AbsZ = parentPos.z + line.RelZ,
                        });
                        id++;
                    }
                }
            }

            // Load objects like monoliths / geowarps from the map groundcover file
            var groundCoverData = LSTReader.ReadGroundCoverLST(mapResourcesFolder, mapNumber);
            foreach (var line in groundCoverData.Where(x => allObjectsWithGuids.Contains(x.ObjectType)))
            {
                mapObjects.Add(new PlanetSideObject
                {
                    Id = id,
                    ObjectName = string.IsNullOrWhiteSpace(line.ObjectName) ? line.ObjectType : line.ObjectName,
                    ObjectType = line.ObjectType,
                    AbsX = line.AbsX,
                    AbsY = line.AbsY,
                    AbsZ = line.AbsZ
                });
                id++;
            }

            
            var diff = allObjectsWithGuids.Except(mapObjects.Select(x => x.ObjectType));
            foreach (var item in diff)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"MISSING: {item}");
                Console.ForegroundColor = ConsoleColor.White;
            }


            // Sanity checking to make sure the amount of objects we've got matches a hand written list of expected objects
            if (ExpectedCounts.MapToCounts.ContainsKey(mapNumber))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var expectedCounts = ExpectedCounts.MapToCounts[mapNumber];
                var objectCounts = mapObjects.GroupBy(x => x.ObjectType).Select(group => new { Name = group.Key, Count = group.Count() })
                    .OrderBy(x => x.Name);
                foreach (var item in objectCounts)
                {
                    if (item.Count != expectedCounts[item.Name]) Console.WriteLine($"Mismatch: {item.Name}, Got: {item.Count} Expected: {expectedCounts[item.Name]}");
                }

                var expectingTotal = ExpectedCounts.ExpectedIshundarCounts.Sum(x => x.Value);

                Console.WriteLine($"Expecting {expectingTotal} entities, found {mapObjects.Count}. {expectingTotal - mapObjects.Count} missing");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Assign GUIDs to loaded mapObjects
            GUIDAssigner.AssignGUIDs(mapObjects, structuresWithGuids, entitiesWithGuids);

            // Export to json file
            var json = JsonConvert.SerializeObject(mapObjects.OrderBy(x => x.GUID), Formatting.Indented);
            File.WriteAllText($"guids_map{mapNumber}.json", json);
        }
    }


}
