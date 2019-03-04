using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmenityExtractor;
using Newtonsoft.Json;

namespace PSF_MapGenerator
{
    class Program
    {
        private static int _lastTurretGUID = 5000; // This keeps track of the last used turret weapon guid, as they seem to be arbitrarily assigned at 5000+
        private static List<PlanetSideObject> _objList;
        private static readonly string[] _towerTypes =  { "tower_a", "tower_b", "tower_c" };
        private static readonly string[] _buildingTypes = {"amp_station", "cryo_facility", "comm_station", "comm_station_dsp", "tech_plant"};
        private static readonly string[] _bunkerTypes = {"bunker_gauntlet", "bunker_lg", "bunker_sm"};

        private static string[] _blacklistedTypes = {"monolith", "hst", "warpgate"}; // types to ignore for now
        private static List<int> _usedLockIds = new List<int>(); // List of lock ids already used to ensure no lock is assigned to two doors
        private static List<int> _usedDoorIds = new List<int>(); // List of door ids already used to ensure no door is assigned two locks (e.g. Akkan CC has two locks on top of each other for one door)

        private static string _mapNumber = "04";

        static void Main(string[] args)
        {
            var json = File.ReadAllText($"guids_map{_mapNumber}.json");
            _objList = JsonConvert.DeserializeObject<List<PlanetSideObject>>(json);

            var file = File.Create($"map{_mapNumber}.txt");
            using (var writer = new System.IO.StreamWriter(file))
            {
                writer.WriteLine("val map" + _mapNumber.TrimStart('0') + " = new ZoneMap(\"map" + _mapNumber + "\") {");

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
                    writer.WriteLine($"LocalBuilding({obj.GUID}, {obj.MapID}, FoundationBuilder(Building.Structure(StructureType.{structureType}, Vector3({obj.AbsX}f, {obj.AbsY}f, {obj.AbsZ}f))))");

                    WriteCaptureConsole(children, writer);
                    WriteDoorsAndLocks(children, writer);
                    WriteLockers(children, writer);
                    WriteTerminalsAndSpawnPads(children, obj, writer);
                    WriteResourceSilos(children, writer);
                    WriteSpawnTubes(children, obj, writer);
                    WriteProximityTerminals(children, writer);
                    WriteTurrets(children, writer);
                    writer.WriteLine("}");
                }

                writer.WriteLine("Projectiles(this)");
                writer.WriteLine("}");

                writer.Flush();
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static void WriteTurrets(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            foreach (var turret in children.Where(x => x.ObjectType == "manned_turret"))
            {
                logWriter.WriteLine($"LocalObject({turret.GUID}, FacilityTurret.Constructor(manned_turret, Vector3({turret.AbsX}f, {turret.AbsY}f, {turret.AbsZ}f)))");
                WriteObjectToBuilding(turret, logWriter);
                logWriter.WriteLine($"TurretToWeapon({turret.GUID}, {_lastTurretGUID})");
                _lastTurretGUID++;
            }
        }

        private static void WriteProximityTerminals(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var proximityTerminalTypes = new[] { "adv_med_terminal", "repair_silo", "pad_landing_frame", "pad_landing_tower_frame", "medical_terminal" };


           

            foreach (var proximityTerminal in children.Where(x => proximityTerminalTypes.Contains(x.ObjectType)))
            {
                logWriter.WriteLine($"LocalObject({proximityTerminal.GUID}, ProximityTerminal.Constructor({proximityTerminal.ObjectType}, Vector3({proximityTerminal.AbsX}f, {proximityTerminal.AbsY}f, {proximityTerminal.AbsZ}f)))");
                WriteObjectToBuilding(proximityTerminal, logWriter);

                // Some objects such as repair_silo and pad_landing_frame have special terminal objects (e.g. bfr rearm, ground vehicle repair, ground vehicle rearm) that should follow immediately after, with incrementing GUIDs.
                // As such, these will be hardcoded for now.
                switch (proximityTerminal.ObjectType)
                {
                    case "repair_silo":
                        //startup.pak-out/game_objects.adb.lst:27235:add_property repair_silo has_aggregate_bfr_terminal true
                        //startup.pak-out/game_objects.adb.lst:27236:add_property repair_silo has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:27237:add_property repair_silo has_aggregate_recharge_terminal true

                        logWriter.WriteLine($"LocalObject({proximityTerminal.GUID + 1}, Terminal.Constructor(ground_rearm_terminal))");
                        WriteObjectToBuilding(proximityTerminal, (int) proximityTerminal.GUID + 1, logWriter);
                        break;
                    case "pad_landing_frame":
                    case "pad_landing_tower_frame":
                        //startup.pak-out/game_objects.adb.lst:22518:add_property pad_landing_frame has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:22519:add_property pad_landing_frame has_aggregate_recharge_terminal true

                        //startup.pak-out/game_objects.adb.lst:22534:add_property pad_landing_tower_frame has_aggregate_rearm_terminal true
                        //startup.pak-out/game_objects.adb.lst:22535:add_property pad_landing_tower_frame has_aggregate_recharge_terminal true
                        logWriter.WriteLine($"LocalObject({proximityTerminal.GUID + 1}, Terminal.Constructor(air_rearm_terminal))");
                        WriteObjectToBuilding(proximityTerminal, (int) proximityTerminal.GUID + 1, logWriter);
                        break;
                }
            }
        }

        private static void WriteSpawnTubes(List<PlanetSideObject> children, PlanetSideObject parent, StreamWriter logWriter)
        {
            foreach (var spawnTube in children.Where(x => x.ObjectType == "respawn_tube"))
            {
                if (_towerTypes.Contains(parent.ObjectType))
                {
                    logWriter.WriteLine($"LocalObject({spawnTube.GUID}, SpawnTube.Constructor(respawn_tube_tower, Vector3({spawnTube.AbsX}f, {spawnTube.AbsY}f, {spawnTube.AbsZ}f), Vector3(0, 0, {spawnTube.Yaw})))");
                }
                else
                {
                    logWriter.WriteLine($"LocalObject({spawnTube.GUID}, SpawnTube.Constructor(Vector3({spawnTube.AbsX}f, {spawnTube.AbsY}f, {spawnTube.AbsZ}f), Vector3(0, 0, {spawnTube.Yaw})))");
                }

                WriteObjectToBuilding(spawnTube, logWriter);
            }
        }

        private static void WriteResourceSilos(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var silo = children.SingleOrDefault(x => x.ObjectType == "resource_silo");
            if (silo == null) return;

            logWriter.WriteLine($"LocalObject({silo.GUID}, ResourceSilo.Constructor)");
            WriteObjectToBuilding(silo, logWriter);
        }

        private static void WriteTerminalsAndSpawnPads(List<PlanetSideObject> children, PlanetSideObject parent, StreamWriter logWriter)
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

                logWriter.WriteLine($"LocalObject({terminal.GUID}, Terminal.Constructor({terminal.ObjectType}))");
                WriteObjectToBuilding(terminal, logWriter);

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

                    logWriter.WriteLine($"LocalObject({closestSpawnPad.GUID}, VehicleSpawnPad.Constructor(Vector3({closestSpawnPad.AbsX}f, {closestSpawnPad.AbsY}f, {closestSpawnPad.AbsZ}f), Vector3(0f, 0f, 0f)))");
                    logWriter.WriteLine($"TerminalToSpawnPad({terminal.GUID}, {closestSpawnPad.GUID})");
                    WriteObjectToBuilding(closestSpawnPad, logWriter);
                }
            }
        }

        private static double DistanceN(float[] first, float[] second)
        {
            var sum = first.Select((x, i) => (x - second[i]) * (x - second[i])).Sum();
            return Math.Sqrt(sum);
        }

        private static void WriteLockers(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var lockerTypes = new[] { "locker_cryo", "locker_med" };

            foreach (var locker in children.Where(x => lockerTypes.Contains(x.ObjectType)))
            {
                logWriter.WriteLine($"LocalObject({locker.GUID}, Locker.Constructor(Vector3({locker.AbsX}f, {locker.AbsY}f, {locker.AbsZ}f)))");
                WriteObjectToBuilding(locker, logWriter);
            }
        }

        private static void WriteDoorsAndLocks(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var doorTypes = new[] { "gr_door_garage_int", "gr_door_int", "gr_door_med", "spawn_tube_door", "amp_cap_door", "door_dsp", "gr_door_ext", "gr_door_garage_ext", "gr_door_main" };

            var lockTypes = new[] { "lock_external", "lock_garage", "lock_small" };

            var lockList = children.Where(x => lockTypes.Contains(x.ObjectType));
            var doorList = children.Where(x => doorTypes.Contains(x.ObjectType));

            foreach (var door in doorList)
            {
                logWriter.WriteLine($"LocalObject({door.GUID}, Door.Constructor(Vector3({door.AbsX}f, {door.AbsY}f, {door.AbsZ}f)))");
                WriteObjectToBuilding(door, logWriter);
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

                logWriter.WriteLine($"LocalObject({doorLock.GUID}, IFFLock.Constructor(Vector3({doorLock.AbsX}f, {doorLock.AbsY}f, {doorLock.AbsZ}f), Vector3(0, 0, {doorLock.Yaw + yawOffset})))");
                logWriter.WriteLine($"DoorToLock({closestDoor.GUID}, {doorLock.GUID})");
                WriteObjectToBuilding(doorLock, logWriter);

                _usedDoorIds.Add((int) closestDoor.GUID);
                _usedLockIds.Add((int) doorLock.GUID);
            }

            // Just in case a lock wasn't linked to a door for some reason we'll print it to the console for further investigation
            var locksNotUsed = lockList.Where(x => !_usedLockIds.Any(y => y == x.GUID));
            if(locksNotUsed.Any()) Console.WriteLine("Found unused locks: " + string.Join(", ", locksNotUsed.Select(x => x.GUID)));
        }

        private static void WriteCaptureConsole(List<PlanetSideObject> children, StreamWriter logWriter)
        {
            var captureTerminals = new[] { "capture_terminal", "secondary_capture" };
            var objList = children.Where(x => captureTerminals.Contains(x.ObjectType));
            foreach (var obj in objList)
            {
                logWriter.WriteLine($"LocalObject({obj.GUID}, CaptureTerminal.Constructor({obj.ObjectType}))");
                WriteObjectToBuilding(obj, logWriter);
            }
        }

        private static void WriteObjectToBuilding(PlanetSideObject obj, StreamWriter logWriter)
        {
            var ownerGUID = _objList.Single(x => x.Id == obj.Owner).GUID;
            logWriter.WriteLine($"ObjectToBuilding({obj.GUID}, {ownerGUID})");
        }

        private static void WriteObjectToBuilding(PlanetSideObject obj, int overrideGUID, StreamWriter logWriter)
        {
            var ownerGUID = _objList.Single(x => x.Id == obj.Owner).GUID;
            logWriter.WriteLine($"ObjectToBuilding({overrideGUID}, {ownerGUID})");
        }
    }
}

