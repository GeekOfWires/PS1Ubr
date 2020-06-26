using System.Collections.Generic;
using System.Linq;

namespace AmenityExtractor
{
    public static class ExpectedGuids
    {
        public static (Dictionary<string, IEnumerable<int>> topLevelObjects, Dictionary<string, IEnumerable<int>> childObjects) Ishundar =
            (new Dictionary<string, IEnumerable<int>>()
            {
                { "amp_station", new List<int> { 1 } },
                { "bunker_gauntlet", Enumerable.Range(4, 5) }, // 4-8
                { "bunker_lg", Enumerable.Range(9, 4) }, // 9-12
                { "bunker_sm", Enumerable.Range(13, 5) }, // 13-17
                { "comm_station", new List<int>() { 18, 21 } },
                { "comm_station_dsp", new List<int>() { 24 } },
                { "cryo_facility", new List<int>() { 27, 30, 33, 36 } },
                { "hst", new List<int>() { 39, 40 } },
                { "monolith", new List<int>() { 41 } },
                { "tech_plant", new List<int>() { 42, 45, 48, 51 } },
                { "tower_a", Enumerable.Range(54, 12) }, // 54-65
                { "tower_b", Enumerable.Range(66, 11) }, // 66-76
                { "tower_c", Enumerable.Range(77, 12) }, // 77-88
                { "warpgate", Enumerable.Range(89, 4) }, // 89-92
            },
            new Dictionary<string, IEnumerable<int>>()
            {
                { "ad_billboard_inside", Enumerable.Range(93, 59) }, // 93-151
                { "ad_billboard_inside_big", Enumerable.Range(152, 8) }, // 152-159
                { "ad_billboard_outside", Enumerable.Range(160, 8) }, // 160-167
                { "adv_med_terminal", Enumerable.Range(168, 4) }, // 167-171
                { "air_vehicle_terminal", Enumerable.Range(172, 8) }, //172-179
                { "amp_cap_door", new List<int>() { 180, 181 } },
                { "bfr_door", Enumerable.Range(182, 12) }, // 182-193
                { "bfr_terminal", Enumerable.Range(194, 12) }, // 194-205
                { "capture_core_fx", Enumerable.Range(206, 12) }, // 206-217
                { "capture_terminal", Enumerable.Range(218, 12) }, // 218-229
                { "cert_terminal", Enumerable.Range(230, 32) }, // 230-261
                { "cryo_tubes", Enumerable.Range(262, 16) }, // 262-277
                { "cs_comm_dish", Enumerable.Range(278, 3) }, // 278 - 280
                { "door_dsp", new List<int>() {281 } },
                { "dropship_pad_doors", new List<int>() { 282 } },
                { "dropship_vehicle_terminal", new List<int>() { 283 } },
                { "g_barricades", Enumerable.Range(284, 4) }, // 284-287
                { "gen_control", Enumerable.Range(288, 12) }, // 288-299
                { "generator", Enumerable.Range(300, 12) }, // 300-311
                { "gr_door_ext", Enumerable.Range(312, 339) }, // 312-650
                { "gr_door_garage_ext", Enumerable.Range(651, 4) }, // 651-654
                { "gr_door_garage_int", Enumerable.Range(655, 4) }, //655-658
                { "gr_door_int", Enumerable.Range(659, 259) }, //659-917
                { "gr_door_main", Enumerable.Range(918, 12) }, // 918-929
                { "gr_door_med", Enumerable.Range(930, 8) }, // 930-937
                { "implant_terminal", Enumerable.Range(938, 8) }, // 938-945
                { "implant_terminal_mech", Enumerable.Range(946, 8) }, // 946-953
                { "llm_socket", Enumerable.Range(954, 12) }, // 954-965
                { "lock_external", Enumerable.Range(966, 12) }, // 966-977
                { "lock_garage", Enumerable.Range(978, 4) }, // 978-981
                { "lock_small", Enumerable.Range(982, 273) }, // 982-1254
                { "locker_cryo", Enumerable.Range(1255, 460) }, // 1255-1714
                { "locker_med", Enumerable.Range(1715, 32) }, // 1715-1746
                { "main_terminal", Enumerable.Range(1747, 12) }, // 1747-1758
                { "manned_turret", Enumerable.Range(1759, 118) }, // 1759-1876
                { "mb_pad_creation", Enumerable.Range(1877, 20) }, // 1877-1896
                { "medical_terminal", Enumerable.Range(1897, 20) }, // 1897-1916
                { "order_terminal", Enumerable.Range(1917, 165) }, // 1917-2081
                { "pad_landing", Enumerable.Range(2082, 74) }, // 2082-2155
                { "pad_landing_frame", Enumerable.Range(2156, 150) }, // 2156-2305
                { "pad_landing_tower_frame", Enumerable.Range(2306, 72) }, // 2306-2377
                { "painbox", Enumerable.Range(2378, 12) }, // 2378-2389
                { "painbox_continuous", Enumerable.Range(2390, 12) }, // 2390-2401
                { "painbox_door_radius", Enumerable.Range(2402, 12) }, // 2402-2413
                { "painbox_door_radius_continuous", Enumerable.Range(2414, 36) }, // 2414-2449
                { "painbox_radius_continuous", Enumerable.Range(2450, 105) }, // 2450-2554
                { "repair_silo", Enumerable.Range(2555, 96) }, // 2555-2650
                { "resource_silo", Enumerable.Range(2651, 12) }, // 2651-2662
                { "respawn_tube", Enumerable.Range(2663, 106) }, // 2663-2768
                { "secondary_capture", Enumerable.Range(2769, 35) }, // 2769-2803
                { "spawn_terminal", Enumerable.Range(2804, 83) }, // 2804-2886
                { "spawn_tube_door", Enumerable.Range(2887, 106) }, // 2887-2992
                { "vanu_module_node", Enumerable.Range(2993, 72) }, // 2993-3064
                { "vehicle_terminal", Enumerable.Range(3065, 12) }, // 2065-3076
            });

        public static (Dictionary<string, IEnumerable<int>> topLevelObjects, Dictionary<string, IEnumerable<int>> childObjects) Byblos =
            (new Dictionary<string, IEnumerable<int>>()
            {
                { "ceiling_bldg_a", new List<int>() { 1 } },
                { "ceiling_bldg_b", new List<int>() { 2 } },
                { "ceiling_bldg_c", new List<int>() { 3 } },
                { "ceiling_bldg_d", new List<int>() { 4, 5 } },
                { "ceiling_bldg_e", new List<int>() { 6 } },
                { "ceiling_bldg_f", new List<int>() { 7 } },
                { "ceiling_bldg_g", new List<int>() { 8 } },
                { "ceiling_bldg_h", new List<int>() { 9 } },
                { "crystals_energy_a", new List<int>() { 10, 11 } },
                { "crystals_energy_b", Enumerable.Range(12, 6) }, // 12-17
                { "crystals_health_a", Enumerable.Range(18, 8) }, // 18-25
                { "crystals_health_b", Enumerable.Range(26, 11) }, // 26-36
                { "crystals_repair_a", Enumerable.Range(37, 9) }, // 37-45
                { "crystals_repair_b", Enumerable.Range(46, 7) }, // 46-52
                { "crystals_vehicle_a", new List<int>() { 53, 55, 57, 59, 61, 63, 65 } },
                { "crystals_vehicle_b", new List<int>() { 67, 69, 71, 73, 75, 77, 79 } },
                { "ground_bldg_a", new List<int>() { 81, 82 } },
                { "ground_bldg_b", new List<int>() { 83 } },
                { "ground_bldg_c", new List<int>() { 84, 85 } },
                { "ground_bldg_d", new List<int>() { 86, 87 } },
                { "ground_bldg_e", new List<int>() { 88, 89 } },
                { "ground_bldg_f", new List<int>() { 90 } },
                { "ground_bldg_g", new List<int>() { 91 } },
                { "ground_bldg_h", new List<int>() { 92 } },
                { "ground_bldg_i", new List<int>() { 93 } },
                { "redoubt", new List<int>() { 94, 95 } },
                { "stationaryteleportpad", Enumerable.Range(96, 56) }, // 96-151
                { "vanu_control_point", new List<int>() { 152, 153 } },
                { "vanu_core", new List<int>() { 154 } },
                { "vanu_sentry_turret", Enumerable.Range(155, 26) }, // 155-180
                { "vanu_vehicle_station", new List<int>() { 181, 182 } },
                { "warpgate_cavern", new List<int>() { 183, 184, 185, 186 } },
                { "zipline", Enumerable.Range(187, 412) } // 187-598
            },
            new Dictionary<string, IEnumerable<int>>()
            {
                { "ancient_door", Enumerable.Range(599, 124) }, // 599-722
                { "ancient_garage_door", new List<int>() { 723, 724 } },
                { "crystals_energy_a", Enumerable.Range(725, 10) }, // 725-734
                { "crystals_energy_b", Enumerable.Range(735, 8) }, // 735-742
                { "crystals_health_a", new List<int>() { 743, 744 } },
                { "crystals_health_b", Enumerable.Range(745, 6) }, // 745-750
                { "crystals_repair_a", Enumerable.Range(751, 8) }, // 751-758
                { "crystals_repair_b", new List<int>() { 759, 760 } },
                { "painbox_continuous", Enumerable.Range(761, 6) }, // 761-766
                { "painbox_door_radius_continuous", Enumerable.Range(767, 8) }, // 767-774
                { "redoubt_floor", new List<int>() { 775, 776 } },
                { "stationaryteleportpad", Enumerable.Range(777, 32) }, // 777-808
                { "vanu_air_vehicle_term", new List<int>() { 809, 810, 811, 812 } },
                { "vanu_center_beam", new List<int>() { 813 } },
                { "vanu_control_console", Enumerable.Range(814, 6) }, // 814-819
                { "vanu_equipment_term", Enumerable.Range(820, 48) }, // 820-867
                { "vanu_module_node_bind", new List<int>() { 868, 869 } },
                { "vanu_module_node_defender", new List<int>() { 870, 871 } },
                { "vanu_module_node_energy", new List<int>() { 872, 873 } },
                { "vanu_module_node_fortifier", new List<int>() { 874, 875 } },
                { "vanu_module_node_vehicle", new List<int>() { 876, 877 } },
                { "vanu_module_node_weapon", new List<int>() { 878, 879 } },
                { "vanu_spawn_room_pad", new List<int>() { 880, 881 } },
                { "vanu_vehicle_creation_pad", Enumerable.Range(882, 6) }, // 822-887
                { "vanu_vehicle_term", new List<int>() { 888, 889 } },
                { "vanumodulebeam", new List<int>() { 890 } },
                { "zipline", Enumerable.Range(891, 14) } // 891-904
            });
    }
}
