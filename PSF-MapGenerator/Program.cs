using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using AmenityExtractor;
using Newtonsoft.Json;

namespace PSF_MapGenerator
{
    class Program
    {
        private static string _planetsideModReadyFolder = "D:\\Planetside (Mod ready)\\Planetside";
        private static readonly string[] _towerTypes =  { "tower_a", "tower_b", "tower_c" };
        private static readonly string[] _buildingTypes = {"amp_station", "cryo_facility", "comm_station", "comm_station_dsp", "tech_plant"};
        private static readonly string[] _bunkerTypes = {"bunker_gauntlet", "bunker_lg", "bunker_sm"};
        private static readonly string[] _warpGateTypes = {"hst", "warpgate", "warpgate_small"};

        // monolith, hst, warpgate are ignored for now as the scala code isn't ready to handle them.
        // BFR terminals/doors are ignored as top level elements as sanctuaries have them with no associated building. (repair_silo also has this problem, but currently is ignored in the AmenityExtrator project)
        // Force domes have GUIDs but are currently classed as separate entities. The dome is controlled by sending GOAM 44 / 48 / 52 to the building GUID
        private static string[] _blacklistedTypes = {"monolith", "bfr_door", "bfr_terminal", "force_dome_dsp_physics", "force_dome_comm_physics", "force_dome_cryo_physics", "force_dome_tech_physics", "force_dome_amp_physics" };
        

        private static Dictionary<string, string> maps = new Dictionary<string, string> {
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

        static void Main(string[] args)
        {
            System.Threading.Tasks.Parallel.ForEach(maps, map =>
            {
                var mapNumber = map.Value;
                var mapName = map.Key;

                var json = File.ReadAllText($"guids_map{mapNumber}.json");
                var _objList = JsonConvert.DeserializeObject<List<PlanetSideObject>>(json);

                var _lastTurretGUID = 5000; // This keeps track of the last used turret weapon guid, as they seem to be arbitrarily assigned at 5000+
                var _usedLockIds = new List<int>(); // List of lock ids already used to ensure no lock is assigned to two doors
                var _usedDoorIds = new List<int>(); // List of door ids already used to ensure no door is assigned two locks (e.g. Akkan CC has two locks on top of each other for one door)

                var file = File.Create($"Map{mapNumber}.scala");
                using (var writer = new System.IO.StreamWriter(file))
                {
                    writer.WriteLine("package zonemaps");
                    writer.WriteLine("");
                    writer.WriteLine("import net.psforever.objects.zones.ZoneMap");
                    writer.WriteLine("import net.psforever.objects.GlobalDefinitions._");
                    writer.WriteLine("import net.psforever.objects.LocalProjectile");
                    writer.WriteLine("import net.psforever.objects.ballistics.Projectile");
                    writer.WriteLine("import net.psforever.objects.serverobject.doors.Door");
                    writer.WriteLine("import net.psforever.objects.serverobject.implantmech.ImplantTerminalMech");
                    writer.WriteLine("import net.psforever.objects.serverobject.locks.IFFLock");
                    writer.WriteLine("import net.psforever.objects.serverobject.mblocker.Locker");
                    writer.WriteLine("import net.psforever.objects.serverobject.pad.VehicleSpawnPad");
                    writer.WriteLine("import net.psforever.objects.serverobject.structures.{Building, FoundationBuilder, StructureType, WarpGate}");
                    writer.WriteLine("import net.psforever.objects.serverobject.terminals.{CaptureTerminal, ProximityTerminal, Terminal}");
                    writer.WriteLine("import net.psforever.objects.serverobject.tube.SpawnTube");
                    writer.WriteLine("import net.psforever.objects.serverobject.resourcesilo.ResourceSilo");
                    writer.WriteLine("import net.psforever.objects.serverobject.turret.FacilityTurret");
                    writer.WriteLine("import net.psforever.types.Vector3");
                    writer.WriteLine("");
                    writer.WriteLine($"object Map{mapNumber} {{");
                    writer.WriteLine($"// {mapName}");
                    writer.WriteLine("val ZoneMap = new ZoneMap(\"map" + mapNumber + "\") { ");

                    foreach (var obj in _objList.Where(x => x.Owner == null))
                    {
                        if (_blacklistedTypes.Contains(obj.ObjectType)) continue; // skip blacklisted types

                        var children = _objList.Where(x => x.Owner == obj.Id).ToList();

                        var structureType = "Building";
                        if (_towerTypes.Contains(obj.ObjectType)) structureType = "Tower";
                        if (_buildingTypes.Contains(obj.ObjectType)) structureType = "Facility";
                        if (_bunkerTypes.Contains(obj.ObjectType)) structureType = "Bunker";
                        //todo: Platform types


                        writer.WriteLine("");
                        writer.WriteLine($"Building{obj.MapID}()");
                        writer.WriteLine($"def Building{obj.MapID}() : Unit = {{ // Name: {obj.ObjectName} Type: {obj.ObjectType} GUID: {obj.GUID}, MapID: {obj.MapID}");

                        if (_warpGateTypes.Contains(obj.ObjectType.ToLower()))
                        {
                            writer.WriteLine(obj.ObjectType.ToLower() == "hst"
                                ? $"LocalBuilding({obj.GUID}, {obj.MapID}, FoundationBuilder(WarpGate.Structure(Vector3({obj.AbsX}f, {obj.AbsY}f, {obj.AbsZ}f), hst)))"
                                : $"LocalBuilding({obj.GUID}, {obj.MapID}, FoundationBuilder(WarpGate.Structure(Vector3({obj.AbsX}f, {obj.AbsY}f, {obj.AbsZ}f))))");
                        }
                        else
                        {
                            writer.WriteLine($"LocalBuilding({obj.GUID}, {obj.MapID}, FoundationBuilder(Building.Structure(StructureType.{structureType}, Vector3({obj.AbsX}f, {obj.AbsY}f, {obj.AbsZ}f))))");
                        }


                        WriteCaptureConsole(_objList, children, writer);
                        WriteDoorsAndLocks(_objList, ref _usedDoorIds, ref _usedLockIds, children, writer);
                        WriteLockers(_objList, children, writer);
                        WriteTerminalsAndSpawnPads(_objList, children, obj, writer);
                        WriteResourceSilos(_objList, children, writer);
                        WriteSpawnTubes(_objList, children, obj, writer);
                        WriteProximityTerminals(_objList, children, writer);
                        WriteTurrets(_objList, ref _lastTurretGUID, children, writer);
                        WriteImplantTerminals(_objList, children, writer);
                        writer.WriteLine("}");
                    }

                    writer.WriteLine("}");
                    writer.WriteLine("}");
                    writer.Flush();
                }
            });

            Console.WriteLine("Done");
        }

        private static void WriteTurrets(List<PlanetSideObject> _objList, ref int _lastTurretGUID, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            foreach (var turret in children.Where(x => x.ObjectType == "manned_turret"))
            {
                logWriter.WriteLine($"LocalObject({turret.GUID}, FacilityTurret.Constructor(manned_turret, Vector3({turret.AbsX}f, {turret.AbsY}f, {turret.AbsZ}f)), owning_building_guid = {_objList.Single(x => x.Id == turret.Owner).GUID})");
                logWriter.WriteLine($"TurretToWeapon({turret.GUID}, {_lastTurretGUID})");
                _lastTurretGUID++;
            }
        }

        private static void WriteImplantTerminals(List<PlanetSideObject> _objList, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var terminalList = children.Where(x => x.ObjectType == "implant_terminal");

            foreach (var terminalMech in children.Where(x => x.ObjectType == "implant_terminal_mech"))
            {
                logWriter.WriteLine($"LocalObject({terminalMech.GUID}, ImplantTerminalMech.Constructor, owning_building_guid = {_objList.Single(x => x.Id == terminalMech.Owner).GUID})");

                var closestTerminal = terminalList.Select(x => new {
                        Distance = DistanceN(new float[] { terminalMech.AbsX, terminalMech.AbsY, terminalMech.AbsZ },
                            new float[] { x.AbsX, x.AbsY, x.AbsZ }),
                        x
                    }
                ).OrderBy(x => x.Distance).First(x => x.Distance <= 5).x;

                logWriter.WriteLine($"LocalObject({closestTerminal.GUID}, Terminal.Constructor(implant_terminal_interface), owning_building_guid = {_objList.Single(x => x.Id == closestTerminal.Owner).GUID})");

                logWriter.WriteLine($"TerminalToInterface({terminalMech.GUID}, {closestTerminal.GUID})");
            }
        }

        private static void WriteProximityTerminals(List<PlanetSideObject> _objList, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var proximityTerminalTypes = new[] { "adv_med_terminal", "repair_silo", "pad_landing_frame", "pad_landing_tower_frame", "medical_terminal" };

            foreach (var proximityTerminal in children.Where(x => proximityTerminalTypes.Contains(x.ObjectType)))
            {
                logWriter.WriteLine($"LocalObject({proximityTerminal.GUID}, ProximityTerminal.Constructor({proximityTerminal.ObjectType}, Vector3({proximityTerminal.AbsX}f, {proximityTerminal.AbsY}f, {proximityTerminal.AbsZ}f)), owning_building_guid = {_objList.Single(x => x.Id == proximityTerminal.Owner).GUID})");

                // Some objects such as repair_silo and pad_landing_frame have special terminal objects (e.g. bfr rearm, ground vehicle repair, ground vehicle rearm) that should follow immediately after, with incrementing GUIDs.
                // As such, these will be hardcoded for now.
                switch (proximityTerminal.ObjectType)
                {
                    case "repair_silo":
                        //startup.pak-out/game_objects.adb.lst:27235:add_property repair_silo has_aggregate_bfr_terminal true
                        //startup.pak-out/game_objects.adb.lst:27236:add_property repair_silo has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:27237:add_property repair_silo has_aggregate_recharge_terminal true

                        logWriter.WriteLine($"LocalObject({proximityTerminal.GUID + 1}, Terminal.Constructor(ground_rearm_terminal), owning_building_guid = {_objList.Single(x => x.Id == proximityTerminal.Owner).GUID})");
                        break;
                    case "pad_landing_frame":
                    case "pad_landing_tower_frame":
                        //startup.pak-out/game_objects.adb.lst:22518:add_property pad_landing_frame has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:22519:add_property pad_landing_frame has_aggregate_recharge_terminal true

                        //startup.pak-out/game_objects.adb.lst:22534:add_property pad_landing_tower_frame has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:22535:add_property pad_landing_tower_frame has_aggregate_recharge_terminal true
                        logWriter.WriteLine($"LocalObject({proximityTerminal.GUID + 1}, Terminal.Constructor(air_rearm_terminal), owning_building_guid = {_objList.Single(x => x.Id == proximityTerminal.Owner).GUID})");
                        break;
                }
            }
        }

        private static void WriteSpawnTubes(List<PlanetSideObject> _objList, List<PlanetSideObject> children, PlanetSideObject parent, StreamWriter logWriter)
        {
            var respawnTubeTypes = new[] {"respawn_tube", "mb_respawn_tube"};

            foreach (var spawnTube in children.Where(x => respawnTubeTypes.Contains(x.ObjectType)))
            {
                var tubeType = "";
                if (_towerTypes.Contains(parent.ObjectType))
                {
                    tubeType = "respawn_tube_tower";
                }
                else if (parent.ObjectType.Contains("VT_building", StringComparison.OrdinalIgnoreCase))
                {
                    tubeType = "respawn_tube_sanctuary";
                }

                logWriter.WriteLine(tubeType == ""
                    ? $"LocalObject({spawnTube.GUID}, SpawnTube.Constructor(Vector3({spawnTube.AbsX}f, {spawnTube.AbsY}f, {spawnTube.AbsZ}f), Vector3(0, 0, {spawnTube.Yaw})), owning_building_guid = {_objList.Single(x => x.Id == spawnTube.Owner).GUID})"
                    : $"LocalObject({spawnTube.GUID}, SpawnTube.Constructor({tubeType}, Vector3({spawnTube.AbsX}f, {spawnTube.AbsY}f, {spawnTube.AbsZ}f), Vector3(0, 0, {spawnTube.Yaw})), owning_building_guid = {_objList.Single(x => x.Id == spawnTube.Owner).GUID})");
            }
        }

        private static void WriteResourceSilos(List<PlanetSideObject> _objList, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var silo = children.SingleOrDefault(x => x.ObjectType == "resource_silo");
            if (silo == null) return;

            logWriter.WriteLine($"LocalObject({silo.GUID}, ResourceSilo.Constructor, owning_building_guid = {_objList.Single(x => x.Id == silo.Owner).GUID})");
        }

        private static void WriteTerminalsAndSpawnPads(List<PlanetSideObject> _objList, List<PlanetSideObject> children, PlanetSideObject parent, StreamWriter logWriter)
        {
            var terminalTypes = new[] { "order_terminal", "spawn_terminal", "cert_terminal", "order_terminal" };
            var terminalTypesWithSpawnPad = new[] { "ground_vehicle_terminal", "air_vehicle_terminal", "vehicle_terminal", "vehicle_terminal_combined", "dropship_vehicle_terminal" };

            var allTerminalTypes = terminalTypes.Concat(terminalTypesWithSpawnPad);

            var spawnPadList = children.Where(x => x.ObjectType == "mb_pad_creation" || x.ObjectType == "dropship_pad_doors").ToList();

            foreach (var terminal in children.Where(x => allTerminalTypes.Contains(x.ObjectType)))
            {
                // SoE in their infinite wisdom decided to remap vehicle_terminal to vehicle_terminal_combined in certain cases in the game_objects.adb file.
                // As such, we have to work around it.

                /*
                    startup.pak-out/game_objects.adb.lst:1097:add_property amp_station child_remap vehicle_terminal vehicle_terminal_combined
                    startup.pak-out/game_objects.adb.lst:7654:add_property comm_station child_remap vehicle_terminal vehicle_terminal_combined
                    startup.pak-out/game_objects.adb.lst:7807:add_property cryo_facility child_remap vehicle_terminal vehicle_terminal_combined
                 */
                if (terminal.ObjectType == "vehicle_terminal" &&
                    (parent.ObjectType == "amp_station" ||
                     parent.ObjectType == "comm_station" ||
                     parent.ObjectType == "cryo_facility"))
                {
                    terminal.ObjectType = "vehicle_terminal_combined";
                }

                // The scala codebase also uses ground_vehicle_terminal as the object type instead of vehicle_terminal, so we'll map to that for now.
                if(terminal.ObjectType == "vehicle_terminal") { terminal.ObjectType = "ground_vehicle_terminal"; }

                logWriter.WriteLine($"LocalObject({terminal.GUID}, Terminal.Constructor({terminal.ObjectType}), owning_building_guid = {_objList.Single(x => x.Id == terminal.Owner).GUID})");

                if (terminalTypesWithSpawnPad.Contains(terminal.ObjectType))
                {
                    // find closest spawn pad to this terminal
                    var searchDistance = 25;
                    var closestSpawnPad = spawnPadList.Select(x => new {
                                                                    Distance = DistanceN(new float[] { terminal.AbsX, terminal.AbsY, terminal.AbsZ },
                                                                                        new float[] { x.AbsX, x.AbsY, x.AbsZ }),
                                                                        x
                                                                    }
                                                            ).OrderBy(x => x.Distance).First(x => x.Distance <= searchDistance).x;

                    // It appears that spawn pads have a default rotation that it +90 degrees from where it should be, presumably the model is rotated differently to the expected orientation - this can be handled here in the map generation
                    // On top of that, some spawn pads also have an additional rotation (vehiclecreationzorientoffset) when spawning vehicles set in game_objects.adb.lst - this should be handled on the Scala side
                    var adjustedYaw = closestSpawnPad.Yaw - 90;

                    logWriter.WriteLine($"LocalObject({closestSpawnPad.GUID}, VehicleSpawnPad.Constructor({closestSpawnPad.ObjectType}, Vector3({closestSpawnPad.AbsX}f, {closestSpawnPad.AbsY}f, {closestSpawnPad.AbsZ}f), Vector3(0, 0,{adjustedYaw})), owning_building_guid = {_objList.Single(x => x.Id == closestSpawnPad.Owner).GUID}, terminal_guid = {terminal.GUID})");
                }
            }
        }

        private static double DistanceN(float[] first, float[] second)
        {
            var sum = first.Select((x, i) => (x - second[i]) * (x - second[i])).Sum();
            return Math.Sqrt(sum);
        }

        private static void WriteLockers(List<PlanetSideObject> _objList, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var lockerTypes = new[] { "locker_cryo", "locker_med", "mb_locker" };

            foreach (var locker in children.Where(x => lockerTypes.Contains(x.ObjectType)))
            {
                logWriter.WriteLine($"LocalObject({locker.GUID}, Locker.Constructor(Vector3({locker.AbsX}f, {locker.AbsY}f, {locker.AbsZ}f)), owning_building_guid = {_objList.Single(x => x.Id == locker.Owner).GUID})");
            }
        }

        private static void WriteDoorsAndLocks(List<PlanetSideObject> _objList, ref List<int> _usedDoorIds, ref List<int> _usedLockIds, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var doorTypes = new[] { "gr_door_garage_int", "gr_door_int", "gr_door_med", "spawn_tube_door", "amp_cap_door", "door_dsp", "gr_door_ext", "gr_door_garage_ext", "gr_door_main", "gr_door_mb_ext", "gr_door_mb_int", "gr_door_mb_lrg", "gr_door_mb_obsd", "gr_door_mb_orb", "door_spawn_mb" };

            var lockTypes = new[] { "lock_external", "lock_garage", "lock_small" };

            var lockList = children.Where(x => lockTypes.Contains(x.ObjectType));
            var doorList = children.Where(x => doorTypes.Contains(x.ObjectType));

            foreach (var door in doorList)
            {
                logWriter.WriteLine($"LocalObject({door.GUID}, Door.Constructor(Vector3({door.AbsX}f, {door.AbsY}f, {door.AbsZ}f)), owning_building_guid = {_objList.Single(x => x.Id == door.Owner).GUID})");
            }

            foreach (var doorLock in lockList)
            {
                // Find the closest door to this lock and link them
                var nearbyDoors = doorList.Select(x => new
                {
                    Distance = DistanceN(new float[] {doorLock.AbsX, doorLock.AbsY, doorLock.AbsZ},
                        new float[] {x.AbsX, x.AbsY, x.AbsZ}),
                    x
                }).OrderBy(x => x.Distance);

                var closestDoor = nearbyDoors.FirstOrDefault(x => x.Distance <= 7)?.x;

                if (_usedDoorIds.Contains((int) closestDoor.GUID))
                {
                    Console.WriteLine($"Door already has a lock assigned: door guid {closestDoor.GUID} tried to assign lock guid {doorLock.GUID}");
                    continue;
                }

                // Since tech plant garage locks are the only type where the lock does not face the same direction as the door we need to apply an offset for those, otherwise the door won't operate properly when checking inside/outside angles.
                var yawOffset = doorLock.ObjectType == "lock_garage" ? 90 : 0;

                logWriter.WriteLine($"LocalObject({doorLock.GUID}, IFFLock.Constructor(Vector3({doorLock.AbsX}f, {doorLock.AbsY}f, {doorLock.AbsZ}f), Vector3(0, 0, {doorLock.Yaw + yawOffset})), owning_building_guid = {_objList.Single(x => x.Id == doorLock.Owner).GUID}, door_guid = {closestDoor.GUID})");

                _usedDoorIds.Add((int) closestDoor.GUID);
                _usedLockIds.Add((int) doorLock.GUID);
            }

            // Just in case a lock wasn't linked to a door for some reason we'll print it to the console for further investigation
            var _usedLockIdsTemp = _usedLockIds;
            var locksNotUsed = lockList.Where(x => !_usedLockIdsTemp.Any(y => y == x.GUID));
            if(locksNotUsed.Any()) Console.WriteLine("Found unused locks: " + string.Join(", ", locksNotUsed.Select(x => x.GUID)));
        }

        private static void WriteCaptureConsole(List<PlanetSideObject> _objList, List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var captureTerminals = new[] { "capture_terminal", "secondary_capture" };
            var objList = children.Where(x => captureTerminals.Contains(x.ObjectType));
            foreach (var obj in objList)
            {
                logWriter.WriteLine($"LocalObject({obj.GUID}, CaptureTerminal.Constructor({obj.ObjectType}), owning_building_guid = {_objList.Single(x => x.Id == obj.Owner).GUID})");
            }
        }
    }
}

