using AmenityExtractor.Models;
using FileReaders;
using FileReaders.Models;
using Newtonsoft.Json;
using PS1Ubr;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Formatting = Newtonsoft.Json.Formatting;

namespace AmenityExtractor
{
    class Program
    {
        // Path to the mod ready planetside folder that has everything pre-extracted from the pak files.
        private static string _planetsideModReadyFolder = "C:\\Planetside (Mod ready)\\Planetside";

        private static List<string> _objectsWithGuids = new List<string>(File.ReadAllLines("ObjectsWithGuids.txt"));
        private static ConcurrentDictionary<string, CUberData> _ubrData = new ConcurrentDictionary<string, CUberData>();

        static void Main(string[] args)
        {
            var maps = new List<(string name, string mapNumber, bool isCave)> {
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
            Parallel.ForEach(Directory.GetFiles(_planetsideModReadyFolder, "*.ubr", SearchOption.AllDirectories),
                file => { _ubrData.TryAdd(file, UBRReader.GetUbrData(file)); });

            foreach (var map in maps)
            {
                var identifiableObjects = new List<PlanetSideObject>();

                var (mapName, mapNumber, isCave) = map;
                Console.WriteLine($"Processing map {mapNumber} - {mapName}");

                int id = 0;

                if (!isCave)
                {
                    // Read the contents_map mpo file, and the associated objects_map lst file
                    var mpoData = MPOReader.ReadMPOFile(_planetsideModReadyFolder, mapNumber);
                    foreach (var entry in mpoData)
                    {
                        ProcessObject(identifiableObjects, entry, ref id, mapId: mpoData.IndexOf(entry) + 1, isTopLevel: true);
                    }
                }

                // Read groundcover data
                var groundCoverData = LSTReader.ReadGroundCoverLST(_planetsideModReadyFolder, mapNumber, isCave);
                foreach (var line in groundCoverData)
                {
                    var mapObj = LstObjectToMapObject(line);
                    ProcessObject(
                        identifiableObjects,
                        mapObj,
                        ref id,
                        mapId: line.Id,
                        isTopLevel: !mapObj.HasBangPrefix
                    );
                }

                // Battle islands for some reason have an X/Y offset applied to all coordinates in game_objects.adb.lst. Thus, we need to account for that.
                //19318:add_property map99 mapOffsetX 1024.0
                //19319:add_property map99 mapOffsetY 1024.0
                if (new List<string>() { "96", "97", "98", "99" }.Contains(mapNumber))
                {
                    for (int i = 0; i < identifiableObjects.Count(); i++)
                    {
                        identifiableObjects[i].AbsX += 1024;
                        identifiableObjects[i].AbsY += 1024;
                    }
                }

                // bfr_building doesn't actually have a GUID itself - the door does which is a sub-object in the ubr file.
                // Remove it from the list before assigning GUIDs
                identifiableObjects.RemoveAll(x => x.ObjectType == "bfr_building");

                // Assign GUIDs to loaded map objects
                GUIDAssigner.AssignGUIDs(identifiableObjects);

                // Sanity checking to make sure the amount of objects we've got matches a list of expected object counts
                SanityChecker.CheckObjectCounts(identifiableObjects, mapName, mapNumber);

                // Sanity checking that assigned GUIDs match expected GUID ranges
                SanityChecker.CheckGuidRanges(identifiableObjects, mapName, mapNumber);

                // Export to json file
                var json = JsonConvert.SerializeObject(identifiableObjects.OrderBy(x => x.GUID), Formatting.Indented);

                var filename = !isCave ? $"guids_map{mapNumber}.json" : $"guids_ugd{mapNumber}.json";
                File.WriteAllText(filename, json);
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static MapObject LstObjectToMapObject(GroundCover groundCoverObject)
        {
            return new MapObject()
            {
                ObjectType = groundCoverObject.ObjectType,
                ObjectName = groundCoverObject.ObjectName ?? groundCoverObject.ObjectType + "_" + groundCoverObject.Id,
                HorizontalPosition = groundCoverObject.AbsX,
                VerticalPosition = groundCoverObject.AbsY,
                HeightPosition = groundCoverObject.AbsZ,
                HorizontalRotation = groundCoverObject.Roll,
                LstType = groundCoverObject.LstType,
            };
        }

        private static void ProcessObject(List<PlanetSideObject> identifiableObjects, MapObject entry, ref int id, int? mapId, bool isTopLevel = false)
        {
            if (!_objectsWithGuids.Contains(entry.ObjectType)) return;
            Console.WriteLine($"Processing {entry.ObjectType}");

            // Load the relevant *.lst files for this object
            var (peHiddens, peEdits, pseRelativeObjects) = LSTReader.ReadLSTFile(_planetsideModReadyFolder, entry.ObjectType, entry.LstType);

            // Get the root mesh for this object
            var uberData = _ubrData.First(x =>
                x.Value.Entries.Select(y => y.Name).Contains(entry.ObjectType, StringComparer.OrdinalIgnoreCase));

            var objectRotationDegrees = MathFunctions.PS1RotationToDegrees(entry.HorizontalRotation);
            var objectRotationRadians = MathFunctions.DegreesToRadians(objectRotationDegrees);

            var entryObject = new PlanetSideObject
            {
                Id = id,
                ObjectName = entry.ObjectName,
                ObjectType = entry.ObjectType,
                AbsX = entry.HorizontalPosition,
                AbsY = entry.VerticalPosition,
                AbsZ = entry.HeightPosition,
                Yaw = objectRotationDegrees,
                MapID = mapId,
                IsChildObject = !isTopLevel
            };
            identifiableObjects.Add(entryObject);
            id++;

            var parentRotationClockwise =
                MathFunctions.CounterClockwiseToClockwiseRotation(entry.HorizontalRotation);

            // Get the sub-entities from the UBR file that would have a GUID within this object
            var baseMesh = UBRReader.GetMeshSystem(entry.ObjectType, uberData.Value);
            foreach (var meshItem in baseMesh.PortalSystem.MeshItems)
            {
                // If it's not an entity that would be assigned a GUID we don't care about it and should skip it
                if (!_objectsWithGuids.Contains(meshItem.AssetName, StringComparer.OrdinalIgnoreCase))
                    continue;

                // If a line is in the pe_hidden list it should be removed from the game world e.g. Neti pad_landing is removed where the BFR building now exists
                if (peHiddens.Any(x => x.InstanceName == meshItem.InstanceName))
                    continue;

                var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13],
                    objectRotationRadians);
                var meshItemYaw = MathFunctions.TransformToRotationDegrees(meshItem.Transform);

                // Convert from CCW to CW and apply 180 degree offset
                var yaw = parentRotationClockwise + (360 - (180 - meshItemYaw));

                identifiableObjects.Add(new PlanetSideObject
                {
                    Id = id,
                    ObjectName = meshItem.AssetName,
                    ObjectType = meshItem.AssetName,
                    Owner = entryObject.Id,
                    AbsX = entry.HorizontalPosition + rotX,
                    AbsY = entry.VerticalPosition + rotY,
                    AbsZ = entry.HeightPosition + meshItem.Transform[14],
                    Yaw = MathFunctions.NormalizeDegrees((int)yaw),
                    IsChildObject = true
                });
                id++;
            }
            
            ProcessLSTPeEdits(peEdits, identifiableObjects, objectRotationRadians, baseX: entry.HorizontalPosition, baseY: entry.VerticalPosition, baseZ: entry.HeightPosition, ownerId: entryObject.Id, id: ref id);

            ProcessLSTPseRelativeObjects(pseRelativeObjects, identifiableObjects, objectRotationRadians, baseZ: entry.HeightPosition, ownerId: entryObject.Id, id: ref id);
        }

        private static void ProcessLSTPeEdits(List<Pe_Edit> peEdits, List<PlanetSideObject> identifiableObjects, double baseRotationRadians, float baseX, float baseY, float baseZ, int ownerId, ref int id)
        {
            foreach (var peEdit in peEdits)
            {
                if (!_objectsWithGuids.Contains(peEdit.ObjectType, StringComparer.OrdinalIgnoreCase)) continue;

                var (rotX, rotY) = MathFunctions.RotateXY(peEdit.RelX, peEdit.RelY, baseRotationRadians);

                identifiableObjects.Add(new PlanetSideObject
                {
                    Id = id,
                    ObjectName = peEdit.ObjectType,
                    ObjectType = peEdit.ObjectType,
                    Owner = ownerId,
                    AbsX = baseX + rotX,
                    AbsY = baseY + rotY,
                    AbsZ = baseZ + peEdit.RelZ,
                    IsChildObject = true
                });
                id++;
            }
        }

        private static void ProcessLSTPseRelativeObjects(List<Pse_RelativeObject> pseRelativeObjects, List<PlanetSideObject> identifiableObjects, double baseRotationRadians, float baseZ, int ownerId, ref int id)
        {
            foreach (var line in pseRelativeObjects)
            {
                if (!_objectsWithGuids.Contains(line.ObjectName.ToLower())) continue;
                Console.WriteLine($"Processing sub-object {line.ObjectName}");

                var uber = _ubrData.Single(x =>
                    x.Value.Entries.Select(y => y.Name).Contains(line.ObjectName.ToLower()));
                var mesh = UBRReader.GetMeshSystem(line.ObjectName, uber.Value);

                var (parentRotX, parentRotY) =
                    MathFunctions.RotateXY(line.RelX, line.RelY, baseRotationRadians);
                (float x, float y, float z) parentPos = (parentRotX, parentRotY,
                    baseZ + line.RelZ);

                foreach (var meshItem in mesh.PortalSystem.MeshItems)
                {
                    if (!_objectsWithGuids.Contains(meshItem.AssetName)) continue;

                    var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13],
                        baseRotationRadians);
                    identifiableObjects.Add(new PlanetSideObject
                    {
                        Id = id,
                        ObjectName = meshItem.AssetName,
                        ObjectType = meshItem.AssetName,
                        Owner = ownerId,
                        AbsX = parentPos.x + rotX,
                        AbsY = parentPos.y + rotY,
                        AbsZ = parentPos.z + line.RelZ,
                        IsChildObject = true
                    });
                    id++;
                }
            }
        }
    }
}
