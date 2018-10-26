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

            //var expectedIshundarCounts = new Dictionary<string, int>()
            //{
            //    {"amp_station", 1},
            //    {"bunker_gauntlet", 5},
            //    {"bunker_lg", 4},
            //    {"bunker_sm", 5},
            //    {"comm_station", 2},
            //    {"comm_station_dsp", 1},
            //    {"cryo_facility", 4},
            //    {"hst", 2},
            //    {"monolith", 1},
            //    {"tech_plant", 4},
            //    {"tower_a", 12},
            //    {"tower_b", 11},
            //    {"tower_c", 12},
            //    {"warpgate", 4},
            //    {"ad_billboard_inside", 59},
            //    {"ad_billboard_inside_big", 8},
            //    {"ad_billboard_outside", 8},
            //    {"adv_med_terminal", 4},
            //    {"air_vehicle_terminal", 8},
            //    {"amp_cap_door", 2},
            //    {"bfr_door", 12},
            //    {"bfr_terminal", 12},
            //    {"capture_core_fx", 12},
            //    {"capture_terminal", 12},
            //    {"cert_terminal", 32},
            //    {"cryo_tubes", 16},
            //    {"cs_comm_dish", 3},
            //    {"door_dsp", 1},
            //    {"dropship_pad_doors", 1},
            //    {"dropship_vehicle_terminal", 1},
            //    {"g_barricades", 4},
            //    {"gen_control", 12},
            //    {"generator", 12},
            //    {"gr_door_ext", 339},
            //    {"gr_door_garage_ext", 4},
            //    {"gr_door_garage_int", 4},
            //    {"gr_door_int", 259},
            //    {"gr_door_main", 12},
            //    {"gr_door_med", 8},
            //    {"implant_terminal", 8},
            //    {"implant_terminal_mech", 8},
            //    {"llm_socket", 12},
            //    {"lock_external", 12},
            //    {"lock_garage", 4},
            //    {"lock_small", 273},
            //    {"locker_cryo", 460},
            //    {"locker_med", 32},
            //    {"main_terminal", 12},
            //    {"manned_turret", 118},
            //    {"mb_pad_creation", 20},
            //    {"medical_terminal", 20},
            //    {"order_terminal", 165},
            //    {"pad_landing", 74},
            //    {"pad_landing_frame", 50},
            //    {"pad_landing_tower_frame", 24},
            //    {"painbox", 12},
            //    {"painbox_continuous", 12},
            //    {"painbox_door_radius", 12},
            //    {"painbox_door_radius_continuous", 36},
            //    {"painbox_radius_continuous", 105 },
            //    {"repair_silo", 24},
            //    {"resource_silo", 12},
            //    {"respawn_tube", 106},
            //    {"secondary_capture", 35},
            //    {"spawn_terminal", 83},
            //    {"spawn_tube_door", 106},
            //    {"vanu_module_node", 72},
            //    {"vehicle_terminal", 12}
            //};

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
                var rotationRadians = MathFunctions.DegreesToRadians(rotationDegrees);

                var entryObject = new PlanetSideObject
                {
                    Id = id,
                    ObjectName = entry.ObjectName,
                    ObjectType = entry.ObjectType,
                    AbsX = entry.HorizontalPosition,
                    AbsY = entry.VerticalPosition,
                    AbsZ = entry.HeightPosition
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

                    var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13], rotationRadians);
                    var yaw = parentRotationClockwise + MathFunctions.TransformToRotationDegrees(meshItem.Transform).z;

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

                    var (rotX, rotY) = MathFunctions.RotateXY(line.RelX, line.RelY, rotationRadians);
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

                    var (parentRotX, parentRotY) = MathFunctions.RotateXY(line.RelX, line.RelY, rotationRadians);
                    (float x, float y, float z) parentPos = (parentRotX, parentRotY, entry.VerticalPosition + line.RelZ);

                    foreach (var meshItem in mesh.PortalSystem.MeshItems)
                    {
                        if (!allObjectsWithGuids.Contains(meshItem.AssetName)) continue;

                        var (rotX, rotY) = MathFunctions.RotateXY(meshItem.Transform[12], meshItem.Transform[13], rotationRadians);
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
                Console.WriteLine($"MISSING: {item}");
            }

            //var objectCounts = mapObjects.GroupBy(x => x.ObjectType).Select(group => new { Name = group.Key, Count = group.Count() })
            //    .OrderBy(x => x.Name);
            //foreach (var item in objectCounts)
            //{
            //    if (item.Count != expectedIshundarCounts[item.Name]) Console.WriteLine($"Mismatch: {item.Name}, Got: {item.Count} Expected: {expectedIshundarCounts[item.Name]}");
            //}

            //var expectingTotal = expectedIshundarCounts.Sum(x => x.Value);
            //Console.WriteLine($"Expecting {expectingTotal} entities, found {mapObjects.Count}. {expectingTotal - mapObjects.Count} missing");

            // Assign GUIDs to loaded mapObjects
            GUIDAssigner.AssignGUIDs(mapObjects, structuresWithGuids, entitiesWithGuids);

            // Export to json file
            var json = JsonConvert.SerializeObject(mapObjects.OrderBy(x => x.GUID), Formatting.Indented);
            File.WriteAllText($"guids_map{mapNumber}.json", json);
        }
    }
}
