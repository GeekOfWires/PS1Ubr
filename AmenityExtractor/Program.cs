using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using PS1Ubr;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace AmenityExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> structuresWithGuids = new List<string>(File.ReadAllLines("StructuresWithGuids.txt"));
            List<string> entitiesWithGuids = new List<string>(File.ReadAllLines("EntitiesWithGuids.txt"));
            List<string> relativeObjectsToLoad = new List<string>(File.ReadAllLines("RelativeObjectsToLoad.txt"));
            IEnumerable<string> allObjectsWithGuids = structuresWithGuids.Concat(entitiesWithGuids).ToList();

            var maps = new Dictionary<string, string> {
                                                            { "Solsar", "01"},
                                                            { "Hossin", "02"},
                                                            { "Cyssor", "03" },
                                                            { "Ishundar", "04" },
                                                            { "Forseral", "05" },
                                                            { "Ceryshen", "06" },
                                                            { "Esamir", "07" },
                                                            { "Oshur Prime", "08" },
                                                            { "Searhus", "09" },
                                                            { "Amerish", "10" },
                                                            { "HOME1 (NEW CONGLOMORATE SANCTUARY)", "11" },
                                                            { "HOME2 (TERRAN REPUBLIC SANCTUARY)", "12" },
                                                            { "HOME3 (VANU SOVREIGNTY SANCTUARY)", "13" },
                                                            { "Nexus", "96" },
                                                            { "Desolation", "97" },
                                                            { "Ascension", "98" },
                                                            { "Extinction", "99" },
            };

            // Path to the mod ready planetside folder that has everything pre-extracted from the pak files.
            var planetsideModReadyFolder = "D:\\Planetside (Mod ready)\\Planetside";

            
            // Load all *.ubr files
            var uberDataList = new List<CUberData>();
            Parallel.ForEach(Directory.GetFiles(planetsideModReadyFolder, "*.ubr", SearchOption.AllDirectories),
                file => { uberDataList.Add(UBRReader.GetUbrData(file)); });

            foreach (var map in maps)
            {
                var mapObjects = new List<PlanetSideObject>();
                var allObjects = new List<string>(); // This list should contain ALL object types/names, not just the ones that need GUIDs. Can be used for debugging missing objects.

                var mapNumber = map.Value;
                var mapName = map.Key;
                Console.WriteLine($"Processing map {mapNumber} - {mapName}");

                // Read the contents_map mpo file, and the associated objects_map lst file
                var mpoData = MPOReader.MPOReader.ReadMPOFile(planetsideModReadyFolder, mapNumber);

                int id = 0;

                foreach (var entry in mpoData)
                {
                    allObjects.Add(entry.ObjectType);
                    if (!allObjectsWithGuids.Contains(entry.ObjectType)) continue;
                    Console.WriteLine($"Processing {entry.ObjectType}");

                    // Load the relevant *.lst files for this object
                    List<Pe_Edit> peEdits = new List<Pe_Edit>();
                    List<Pse_RelativeObject> pseRelativeObjects = new List<Pse_RelativeObject>();
                    List<Pe_Hidden> peHiddens = new List<Pe_Hidden>();
                    if (entry.LstType != null)
                    {
                        (peHiddens, peEdits, pseRelativeObjects) =
                            LSTReader.ReadLSTFile(planetsideModReadyFolder, entry.ObjectType, entry.LstType);
                    }

                    // Get the root mesh for this object
                    var uberData = uberDataList.Single(x =>
                        x.Entries.Select(y => y.Name).Contains(entry.ObjectType, StringComparer.OrdinalIgnoreCase));
                    
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

                    var parentRotationClockwise =
                        MathFunctions.CounterClockwiseToClockwiseRotation(entry.HorizontalRotation);

                    // Get the sub-entities from the UBR file that would have a GUID within this object
                    var baseMesh = UBRReader.GetMeshSystem(entry.ObjectType, ref uberData);
                    foreach (var meshItem in baseMesh.PortalSystem.MeshItems)
                    {
                        allObjects.Add(meshItem.AssetName);

                        // If it's not an entity that would be assigned a GUID we don't care about it and should skip it
                        if (!allObjectsWithGuids.Contains(meshItem.AssetName, StringComparer.OrdinalIgnoreCase))
                            continue;

                        // If a line is in the pe_hidden list it should be removed from the game world e.g. Neti pad_landing is removed where the BFR building now exists
                        if (peHiddens.Any(x => x.InstanceName == meshItem.InstanceName))
                            continue; 

                        var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13],
                            baseRotationRadians);
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
                        allObjects.Add(line.ObjectName);
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
                            AbsZ = entry.HeightPosition + line.RelZ
                        });
                        id++;
                    }

                    foreach (var line in pseRelativeObjects)
                    {
                        allObjects.Add(line.ObjectName);
                        if (!relativeObjectsToLoad.Contains(line.ObjectName.ToLower())) continue;
                        Console.WriteLine($"Processing sub-object {line.ObjectName}");

                        var uber = uberDataList.Single(x =>
                            x.Entries.Select(y => y.Name).Contains(line.ObjectName.ToLower()));
                        var mesh = UBRReader.GetMeshSystem(line.ObjectName, ref uber);

                        var (parentRotX, parentRotY) =
                            MathFunctions.RotateXY(line.RelX, line.RelY, baseRotationRadians);
                        (float x, float y, float z) parentPos = (parentRotX, parentRotY,
                            entry.VerticalPosition + line.RelZ);

                        foreach (var meshItem in mesh.PortalSystem.MeshItems)
                        {
                            allObjects.Add(meshItem.AssetName);
                            if (!allObjectsWithGuids.Contains(meshItem.AssetName)) continue;

                            var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13],
                                baseRotationRadians);
                            mapObjects.Add(new PlanetSideObject
                            {
                                Id = id,
                                ObjectName = meshItem.AssetName,
                                ObjectType = meshItem.AssetName,
                                Owner = entryObject.Id,
                                AbsX = parentPos.x + rotX,
                                AbsY = parentPos.y + rotY,
                                AbsZ = parentPos.z + line.RelZ,
                            });
                            id++;
                        }
                    }
                }

                var groundCoverData = LSTReader.ReadGroundCoverLST(planetsideModReadyFolder, mapNumber);
                foreach (var line in groundCoverData)
                {
                    allObjects.Add(line.ObjectType);
                }

                // Load objects like monoliths / geowarps / warpgate_small from the map groundcover file
                foreach (var line in groundCoverData.Where(x => allObjectsWithGuids.Contains(x.ObjectType)))
                {
                    mapObjects.Add(new PlanetSideObject
                    {
                        Id = id,
                        ObjectName = string.IsNullOrWhiteSpace(line.ObjectName) ? line.ObjectType : line.ObjectName,
                        ObjectType = line.ObjectType,
                        AbsX = line.AbsX,
                        AbsY = line.AbsY,
                        AbsZ = line.AbsZ,
                        MapID = line.Id
                    });
                    id++;
                }

                // Load sub-objects that should have GUIDs, e.g. bfr_building -> bfr_door in sanctuary
                foreach (var line in groundCoverData.Where(x => relativeObjectsToLoad.Contains(x.ObjectType.ToLower())))
                {
                    Console.WriteLine($"Processing groundcover sub-object {line.ObjectName}");

                    var uber = uberDataList.Single(x =>
                        x.Entries.Select(y => y.Name).Contains(line.ObjectType.ToLower()));
                    var mesh = UBRReader.GetMeshSystem(line.ObjectType, ref uber);

                    foreach (var meshItem in mesh.PortalSystem.MeshItems)
                    {
                        allObjects.Add(meshItem.AssetName);
                        if (!allObjectsWithGuids.Contains(meshItem.AssetName)) continue;

                        mapObjects.Add(new PlanetSideObject
                        {
                            Id = id,
                            ObjectName = meshItem.AssetName,
                            ObjectType = meshItem.AssetName,
                            AbsX = line.AbsX,
                            AbsY = line.AbsY,
                            AbsZ = line.AbsZ,
                        });
                        id++;
                    }
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
                    var objectCounts = mapObjects.GroupBy(x => x.ObjectType)
                        .Select(group => new {Name = group.Key, Count = group.Count()})
                        .OrderBy(x => x.Name);
                    foreach (var item in objectCounts)
                    {
                        if (expectedCounts.ContainsKey(item.Name) && item.Count != expectedCounts[item.Name])
                            Console.WriteLine(
                                $"Mismatch: {item.Name}, Got: {item.Count} Expected: {expectedCounts[item.Name]}");
                    }

                    var expectingTotal = ExpectedCounts.ExpectedIshundarCounts.Sum(x => x.Value);

                    if (expectingTotal != mapObjects.Count)
                    {
                        Console.WriteLine(
                            $"Expecting {expectingTotal} entities, found {mapObjects.Count}. {expectingTotal - mapObjects.Count} missing");
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                }


                // TODO: Fix sanctuary repair_silos
                // Since repair silos are in the groundcover file they count as their own structure in sancs. Currently this isn't supported, so we need to remove them.
                // A workaround is also in GUIDAssigner.AssignGUIDs to skip the requisite amount of GUIDs
                if(mapNumber == "11" || mapNumber == "12" || mapNumber == "13") mapObjects.RemoveAll(x => x.ObjectType == "repair_silo");

                // Battle islands for some reason have an X/Y offset applied to all coordinates in game_objects.adb.lst. Thus, we need to account for that.
                //19318:add_property map99 mapOffsetX 1024.0
                //19319:add_property map99 mapOffsetY 1024.0
                if (new List<string>() {"96", "97", "98", "99"}.Contains(mapNumber))
                {
                    for (int i = 0; i < mapObjects.Count(); i++)
                    {
                        mapObjects[i].AbsX += 1024;
                        mapObjects[i].AbsY += 1024;
                    }
                }

                // Assign GUIDs to loaded mapObjects
                GUIDAssigner.AssignGUIDs(mapObjects, structuresWithGuids, entitiesWithGuids);

                // Export to json file
                var json = JsonConvert.SerializeObject(mapObjects.OrderBy(x => x.GUID), Formatting.Indented);
                File.WriteAllText($"guids_map{mapNumber}.json", json);
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }


}
