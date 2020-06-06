using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PS1Ubr;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using FileReaders;
using FileReaders.Models;
using AmenityExtractor.Models;
using AmenityExtractor.Extensions;

namespace AmenityExtractor
{
    class Program
    {
        private static List<string> structuresWithGuids = new List<string>(File.ReadAllLines("StructuresWithGuids.txt"));
        private static List<string> entitiesWithGuids = new List<string>(File.ReadAllLines("EntitiesWithGuids.txt"));
        private static List<string> relativeObjectsToLoad = new List<string>(File.ReadAllLines("RelativeObjectsToLoad.txt"));

        private static IEnumerable<string> allObjectsWithGuids
        {
            get
            {
                return structuresWithGuids.Concat(entitiesWithGuids).ToList();
            }
        }

        // Path to the mod ready planetside folder that has everything pre-extracted from the pak files.
        private static string planetsideModReadyFolder = "C:\\Planetside (Mod ready)\\Planetside";

        static void Main(string[] args)
        {


            var maps = new List<(string, string, bool)> { // Name, mapNumber, isCave
                                                            ( "Solsar", "01", false),
                                                            ( "Hossin", "02", false),
                                                            ( "Cyssor", "03", false ),
                                                            ( "Ishundar", "04", false ),
                                                            ( "Forseral", "05", false ),
                                                            ( "Ceryshen", "06", false ),
                                                            ( "Esamir", "07", false ),
                                                            ( "Oshur Prime", "08", false ),
                                                            ( "Searhus", "09", false ),
                                                            ( "Amerish", "10", false ),
                                                            ( "HOME1 (NEW CONGLOMORATE SANCTUARY)", "11", false ),
                                                            ( "HOME2 (TERRAN REPUBLIC SANCTUARY)", "12", false ),
                                                            ( "HOME3 (VANU SOVREIGNTY SANCTUARY)", "13", false ),
                                                            ( "Nexus", "96", false ),
                                                            ( "Desolation", "97", false ),
                                                            ( "Ascension", "98", false ),
                                                            ( "Extinction", "99", false ),
                                                            ( "Supai", "01", true ),
                                                            ( "Hunhau", "02", true ),
                                                            ( "Adlivun", "03", true ),
                                                            ( "Byblos", "04", true ),
                                                            ( "Annwn", "05", true ),
                                                            ( "Drugaskan", "06", true )
            };

            // Load all *.ubr files
            var uberDataList = new List<CUberData>();
            Parallel.ForEach(Directory.GetFiles(planetsideModReadyFolder, "*.ubr", SearchOption.AllDirectories),
                file => { uberDataList.Add(UBRReader.GetUbrData(file)); });

            foreach (var map in maps)
            {
                var mapObjects = new List<PlanetSideObject>();
                var allObjects = new List<string>(); // This list should contain ALL object types/names, not just the ones that need GUIDs. Can be used for debugging missing objects.

                var mapName = map.Item1;
                var mapNumber = map.Item2;
                var isCave = map.Item3;
                Console.WriteLine($"Processing map {mapNumber} - {mapName}");

                int id = 0;
                // Process top level objects from MPO file

                // Read the contents_map mpo file, and the associated objects_map lst file
                var mpoData = !isCave ? MPOReader.ReadMPOFile(planetsideModReadyFolder, mapNumber) : new List<MapObject>();
                foreach (var entry in mpoData)
                {
                    ProcessTopLevelObject(mapObjects, allObjects, uberDataList, entry, ref id, mapId: mpoData.IndexOf(entry) + 1);
                }

                var groundCoverData = LSTReader.ReadGroundCoverLST(planetsideModReadyFolder, mapNumber, isCave);
                foreach (var line in groundCoverData)
                {
                    allObjects.Add(line.ObjectType);
                }

                // Load objects like monoliths / geowarps / warpgate_small from the map groundcover file
                foreach (var line in groundCoverData.Where(x => allObjectsWithGuids.Contains(x.ObjectType)))
                {
                   string[] cavernBuildingTypes = { "ceiling_bldg_a", "ceiling_bldg_b", "ceiling_bldg_c", "ceiling_bldg_d", "ceiling_bldg_e", "ceiling_bldg_f",
                                                                    "ceiling_bldg_g", "ceiling_bldg_h", "ceiling_bldg_i", "ceiling_bldg_j", "ceiling_bldg_z",
                                                                    "ground_bldg_a", "ground_bldg_b", "ground_bldg_c", "ground_bldg_d", "ground_bldg_e", "ground_bldg_f",
                                                                    "ground_bldg_g", "ground_bldg_h", "ground_bldg_i", "ground_bldg_j", "ground_bldg_z",
                                                                    "redoubt", "vanu_control_point", "vanu_core", "vanu_vehicle_station" };

                    int parentId = id;
                    PlanetSideObject parent = null;

                    // For cave structures treat them as top level objects
                    if(cavernBuildingTypes.Contains(line.ObjectType.ToLower()))
                    {
                        ProcessTopLevelObject(mapObjects,
                        allObjects,
                        uberDataList,
                        new MapObject()
                        {
                            ObjectType = line.ObjectType,
                            ObjectName = line.ObjectName ?? line.ObjectType + "_" + line.Id,
                            HorizontalPosition = line.AbsX,
                            VerticalPosition = line.AbsY,
                            HeightPosition = line.AbsZ,
                            HorizontalRotation = line.Roll,
                            LstType = line.LstType
                        },
                        ref id,
                        mapId: line.Id);
                    }
                    else
                    {
                        var entry = new PlanetSideObject
                        {
                            Id = id,
                            ObjectName = string.IsNullOrWhiteSpace(line.ObjectName) ? line.ObjectType : line.ObjectName,
                            ObjectType = line.ObjectType,
                            AbsX = line.AbsX,
                            AbsY = line.AbsY,
                            AbsZ = line.AbsZ,
                            MapID = line.Id
                        };
                        mapObjects.Add(entry);
                        id++;

                        parent = mapObjects.Single(x => x.Id == parentId);

                        // If the groundcover object has a .lst file load and process that too (for example facilities within caves are defined in the groundcover file, and contain painboxes in the .lst file)
                        List<Pe_Edit> peEdits = new List<Pe_Edit>();
                        List<Pse_RelativeObject> pseRelativeObjects = new List<Pse_RelativeObject>();
                        List<Pe_Hidden> peHiddens = new List<Pe_Hidden>();
                        (peHiddens, peEdits, pseRelativeObjects) = LSTReader.ReadLSTFile(planetsideModReadyFolder, line.ObjectType, null);

                        var rotationDegrees = MathFunctions.PS1RotationToDegrees(line.Roll);
                        var parentRotationRadians = MathFunctions.DegreesToRadians(rotationDegrees);

                        if (peEdits.Any())
                        {
                            ProcessLSTPeEdits(peEdits, allObjects, mapObjects, parentRotationRadians, baseX: parent.AbsX, baseY: parent.AbsY, baseZ: parent.AbsZ, ownerId: parent.Id, id: ref id);
                        }

                        if (pseRelativeObjects.Any())
                        {
                            ProcessLSTPseRelativeObjects(pseRelativeObjects, allObjects, mapObjects, uberDataList, parentRotationRadians, baseZ: parent.AbsZ, ownerId: parent.Id, id: ref id);
                        }
                    }
                }

                // Load sub-objects that should have GUIDs, e.g. bfr_building -> bfr_door in sanctuary
                foreach (var line in groundCoverData.Where(x => relativeObjectsToLoad.Contains(x.ObjectType.ToLower())))
                {
                    var objectName = string.IsNullOrWhiteSpace(line.ObjectName) ? line.ObjectType : line.ObjectName;
                    Console.WriteLine($"Processing groundcover sub-objects from ubr for {objectName}");

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

                // Sanity checking to make sure the amount of objects we've got matches a hand written list of expected objects
                if (typeof(ExpectedObjectCounts).GetField(mapName) != null)
                {
                    Console.WriteLine($"Sanity Checking object counts on map {mapNumber} {mapName}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    var expectedCounts = (Dictionary<string, int>) typeof(ExpectedObjectCounts).GetField(mapName).GetValue(null);
                    var objectCounts = mapObjects.GroupBy(x => x.ObjectType)
                        .Select(group => new {Name = group.Key, Count = group.Count()})
                        .OrderBy(x => x.Name);
                    foreach (var item in objectCounts)
                    {
                        if (expectedCounts.ContainsKey(item.Name) && item.Count != expectedCounts[item.Name])
                            Console.WriteLine(
                                $"Mismatch: {item.Name}, Got: {item.Count} Expected: {expectedCounts[item.Name]}");
                    }

                    var expectingTotal = ExpectedObjectCounts.Ishundar.Sum(x => x.Value);

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

                // Sanity checking that GUIDs match expected GUID ranges
                if (typeof(ExpectedGuids).GetField(mapName) != null)
                {
                    Console.WriteLine($"Sanity Checking GUIDS on map {mapNumber} {mapName}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    var expectedGuids = (Dictionary<string, IEnumerable<int>>)typeof(ExpectedGuids).GetField(mapName).GetValue(null);

                    foreach ((string objectType, IEnumerable<int> guids) in expectedGuids)
                    {
                        var objects = mapObjects.Where(x => x.ObjectType == objectType);
                        if(objects.Any())
                        {
                            foreach (var item in objects)
                            {
                                if (!guids.Contains((int)item.GUID))
                                {
                                    Console.WriteLine(
                                    $"Mismatch: {item.ObjectType}, GUID: {item.GUID} Expected in range: {guids.ToList().ToRangeString()}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No objects on map {mapNumber} matching expected object type {objectType}");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                }

                // Export to json file
                var json = JsonConvert.SerializeObject(mapObjects.OrderBy(x => x.GUID), Formatting.Indented);

                var filename = !isCave ? $"guids_map{mapNumber}.json" : $"guids_ugd{mapNumber}.json";
                File.WriteAllText(filename, json);
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }



        private static void ProcessTopLevelObject(List<PlanetSideObject> mapObjects, List<string> allObjects, List<CUberData> uberDataList, MapObject entry, ref int id, int mapId)
        {
            allObjects.Add(entry.ObjectType);
            if (!allObjectsWithGuids.Contains(entry.ObjectType)) return;
            Console.WriteLine($"Processing {entry.ObjectType}");

            // Load the relevant *.lst files for this object
            List<Pe_Edit> peEdits = new List<Pe_Edit>();
            List<Pse_RelativeObject> pseRelativeObjects = new List<Pse_RelativeObject>();
            List<Pe_Hidden> peHiddens = new List<Pe_Hidden>();
            (peHiddens, peEdits, pseRelativeObjects) = LSTReader.ReadLSTFile(planetsideModReadyFolder, entry.ObjectType, entry.LstType);

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
                Yaw = rotationDegrees,
                MapID = mapId
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
            
            ProcessLSTPeEdits(peEdits, allObjects, mapObjects, baseRotationRadians, baseX: entry.HorizontalPosition, baseY: entry.VerticalPosition, baseZ: entry.HeightPosition, ownerId: entryObject.Id, id: ref id);

            ProcessLSTPseRelativeObjects(pseRelativeObjects, allObjects, mapObjects, uberDataList, baseRotationRadians, baseZ: entry.HeightPosition, ownerId: entryObject.Id, id: ref id );
        }

        private static void ProcessLSTPeEdits(List<Pe_Edit> peEdits, List<string> allObjects, List<PlanetSideObject> mapObjects, double baseRotationRadians, float baseX, float baseY, float baseZ, int ownerId, ref int id)
        {
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
                    Owner = ownerId,
                    AbsX = baseX + rotX,
                    AbsY = baseY + rotY,
                    AbsZ = baseZ + line.RelZ
                });
                id++;
            }
        }

        private static void ProcessLSTPseRelativeObjects(List<Pse_RelativeObject> pseRelativeObjects, List<string> allObjects, List<PlanetSideObject> mapObjects, List<CUberData> uberDataList, double baseRotationRadians, float baseZ, int ownerId, ref int id)
        {
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
                    baseZ + line.RelZ);

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
                        Owner = ownerId,
                        AbsX = parentPos.x + rotX,
                        AbsY = parentPos.y + rotY,
                        AbsZ = parentPos.z + line.RelZ,
                    });
                    id++;
                }
            }
        }
    }
}
