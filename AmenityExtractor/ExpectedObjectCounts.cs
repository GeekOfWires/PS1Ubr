﻿using System.Collections.Generic;

namespace AmenityExtractor
{
    // This class is just used for debug/testing to ensure that all objects on a continent have been accounted for.
    // Building the counts is done by hand, refer to the order objects are initialized with GUIDs to try and find ranges for each continent.
    public static class ExpectedObjectCounts
    {
        public static Dictionary<string, int> Ishundar = new Dictionary<string, int>()
        {
            {"amp_station", 1},
            {"bunker_gauntlet", 5},
            {"bunker_lg", 4},
            {"bunker_sm", 5},
            {"comm_station", 2},
            {"comm_station_dsp", 1},
            {"cryo_facility", 4},
            {"hst", 2},
            {"monolith", 1},
            {"tech_plant", 4},
            {"tower_a", 12},
            {"tower_b", 11},
            {"tower_c", 12},
            {"warpgate", 4},
            {"ad_billboard_inside", 59},
            {"ad_billboard_inside_big", 8},
            {"ad_billboard_outside", 8},
            {"adv_med_terminal", 4},
            {"air_vehicle_terminal", 8},
            {"amp_cap_door", 2},
            {"bfr_door", 12},
            {"bfr_terminal", 12},
            {"capture_core_fx", 12},
            {"capture_terminal", 12},
            {"cert_terminal", 32},
            {"cryo_tubes", 16},
            {"cs_comm_dish", 3},
            {"door_dsp", 1},
            {"dropship_pad_doors", 1},
            {"dropship_vehicle_terminal", 1},
            {"g_barricades", 4},
            {"gen_control", 12},
            {"generator", 12},
            {"gr_door_ext", 339},
            {"gr_door_garage_ext", 4},
            {"gr_door_garage_int", 4},
            {"gr_door_int", 259},
            {"gr_door_main", 12},
            {"gr_door_med", 8},
            {"implant_terminal", 8},
            {"implant_terminal_mech", 8},
            {"llm_socket", 12},
            {"lock_external", 12},
            {"lock_garage", 4},
            {"lock_small", 273},
            {"locker_cryo", 460},
            {"locker_med", 32},
            {"main_terminal", 12},
            {"manned_turret", 118},
            {"mb_pad_creation", 20},
            {"medical_terminal", 20},
            {"order_terminal", 165},
            {"pad_landing", 74},
            {"pad_landing_frame", 50},
            {"pad_landing_tower_frame", 24},
            {"painbox", 12},
            {"painbox_continuous", 12},
            {"painbox_door_radius", 12},
            {"painbox_door_radius_continuous", 36},
            {"painbox_radius_continuous", 105 },
            {"repair_silo", 24},
            {"resource_silo", 12},
            {"respawn_tube", 106},
            {"secondary_capture", 35},
            {"spawn_terminal", 83},
            {"spawn_tube_door", 106},
            {"vanu_module_node", 72},
            {"vehicle_terminal", 12}
        };

        public static Dictionary<string, Dictionary<string, int>> MapToCounts =
            new Dictionary<string, Dictionary<string, int>>
            {
                {"04", Ishundar}
            };
    }
}