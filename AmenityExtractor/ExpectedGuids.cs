using System.Collections.Generic;
using System.Linq;

namespace AmenityExtractor
{
    public static class ExpectedGuids
    {
        public static Dictionary<string, IEnumerable<int>> Ishundar = new Dictionary<string, IEnumerable<int>>()
        {
            // Structures / Zone owned objects
            { "amp_station", Enumerable.Range(1, 1) },
            { "bunker_gauntlet", Enumerable.Range(4, 8) },
            { "bunker_lg", Enumerable.Range(9, 12) },
            { "bunker_sm", Enumerable.Range(13, 17) },
            { "comm_station", new List<int>() { 18, 21 } },
            { "comm_station_dsp", new List<int>() { 24 } },
            { "cryo_facility", new List<int>() { 27, 30, 33, 36 } },
            { "hst", Enumerable.Range(39, 40) },
            { "monolith", new List<int>() { 41 } },
            { "tech_plant", new List<int>() { 42, 45, 48, 51 } },
            { "tower_a", Enumerable.Range(54, 65) },
            { "tower_b", Enumerable.Range(66, 76) },
            { "tower_c", Enumerable.Range(77, 88) },
            { "warpgate", Enumerable.Range(89, 92) },

            // "Entities"
            { "ad_billboard_inside", Enumerable.Range(93, 151) },
            { "ad_billboard_inside_big", Enumerable.Range(152, 159) },
            { "ad_billboard_outside", Enumerable.Range(160, 167) },
            { "adv_med_terminal", Enumerable.Range(168, 171) },
            { "air_vehicle_terminal", Enumerable.Range(172, 179) },
            { "amp_cap_door", Enumerable.Range(180, 181) },
            { "bfr_door", Enumerable.Range(182, 193) },
            { "bfr_terminal", Enumerable.Range(194, 205) },
            { "capture_core_fx", Enumerable.Range(206, 217) },
            { "capture_terminal", Enumerable.Range(218, 229) },
            { "cert_terminal", Enumerable.Range(230, 261) },
            { "cryo_tubes", Enumerable.Range(262, 277) },
            { "cs_comm_dish", Enumerable.Range(278, 280) },
            { "door_dsp", new List<int>() {281 } },
            { "dropship_pad_doors", new List<int>() { 282 } },
            { "dropship_vehicle_terminal", new List<int>() { 283 } },
            { "g_barricades", Enumerable.Range(284, 287) },
            { "gen_control", Enumerable.Range(288, 299) },
            { "generator", Enumerable.Range(300, 311) },
            { "gr_door_ext", Enumerable.Range(312, 650) },
            { "gr_door_garage_ext", Enumerable.Range(651, 654) },
            { "gr_door_garage_int", Enumerable.Range(655, 658) },
            { "gr_door_int", Enumerable.Range(659, 917) },
            { "gr_door_main", Enumerable.Range(918, 929) },
            { "gr_door_med", Enumerable.Range(930, 937) },
            { "implant_terminal", Enumerable.Range(938, 945) },
            { "implant_terminal_mech", Enumerable.Range(946, 953) },
            { "llm_socket", Enumerable.Range(954, 965) },
            { "lock_external", Enumerable.Range(966, 977) },
            { "lock_garage", Enumerable.Range(978, 981) },
            { "lock_small", Enumerable.Range(982, 1254) },
            { "locker_cryo", Enumerable.Range(1255, 1714) },
            { "locker_med", Enumerable.Range(1715, 1746) },
            { "main_terminal", Enumerable.Range(1747, 1758) },
            { "manned_turret", Enumerable.Range(1759, 1876) },
            { "mb_pad_creation", Enumerable.Range(1877, 1896) },
            { "medical_terminal", Enumerable.Range(1897, 196) },
            { "order_terminal", Enumerable.Range(1917, 2081) },
            { "pad_landing", Enumerable.Range(2082, 2155) },
            { "pad_landing_frame", Enumerable.Range(2156, 2305) },
            { "pad_landing_tower_frame", Enumerable.Range(2306, 2377) },
            { "painbox", Enumerable.Range(2378, 2389) },
            { "painbox_continuous", Enumerable.Range(2390, 2401) },
            { "painbox_door_radius", Enumerable.Range(2402, 2413) },
            { "painbox_door_radius_continuous", Enumerable.Range(2414, 2449) },
            { "painbox_radius_continuous", Enumerable.Range(2450, 2554) },
            { "repair_silo", Enumerable.Range(2555, 2650) },
            { "resource_silo", Enumerable.Range(2651, 2662) },
            { "respawn_tube", Enumerable.Range(2663, 2768) },
            { "secondary_capture", Enumerable.Range(2769, 2803) },
            { "spawn_terminal", Enumerable.Range(2804, 2886) },
            { "spawn_tube_door", Enumerable.Range(2887, 2992) },
            { "vanu_module_node", Enumerable.Range(2993, 3064) },
            { "vehicle_terminal", Enumerable.Range(3065, 3076) },
        };

        public static Dictionary<string, Dictionary<string, IEnumerable<int>>> MapToCounts =
            new Dictionary<string, Dictionary<string, IEnumerable<int>>>
            {
                {"04", Ishundar}
            };
    }
}
