﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MPOReader;

namespace AmenityExtractor
{
    public static class GUIDAssigner
    {
        /*
         GUIDs are assigned in a very specific order, and certain objects have priority over others, for example structures are assigned GUIDs first

         Some objects such as aircraft landing pads or repair silos have multiple GUIDs assigned. This is likely one GUID per function (e.g. one GUID for the rearm, one or more for repair. Exact details are unknown currently)

         GUIDs are typically assigned in the order of Object Type -> X Position -> Y Position -> Z Position
        */

        public static void AssignGUIDs(List<PlanetSideObject> mapObjects, List<string> structuresWithGuids, List<string> entitiesWithGuids)
        {
            var currentGUID = 1;

            // Some objects reserve extra guids for unknown reasons currently, so we need to skip ahead if one of those is found
            // A good example of this that was previously unknown is the repair_silo object. It has three extra GUIDs assigned after it for terminals: which are for BFR Rearming, Ground vehicle rearming and ground vehicle repair
            // Adding these objects to the map is currently hardcoded within the PSF-MapGenerator project
            var objectTypesWithMultipleGuids = new Dictionary<string, int>()
            {
                { "amp_station", 2 }, { "comm_station", 2 }, {"comm_station_dsp", 2}, {"cryo_facility", 2 }, {"tech_plant", 2}, {"pad_landing_frame", 2},
                { "pad_landing_tower_frame", 2}, {"repair_silo", 3}
            };

            var objectNamesWithMultipleGuids = new Dictionary<string, int>();


            // For some unknown reason Cyssor is the only map with a gap between the monolith object (GUID 59 clientside) and the first tech_plant (Wele - GUID 66 clientside) object.
            // So far efforts to determine why have proved fruitless, so the below is a workaround to skip 6 GUIDs ahead after the monolith.
            objectNamesWithMultipleGuids.Add("monolith_cyssor", 6);

            //Repair silos in sanctuaries aren't assigned to a building and are added in the groundcover file. Currently this will break the GUID assignments as a repair_silo is suddenly a structure in it's own right
            // For now, skip them by jumping ahead after the last HART building in each Sanctuary.
            objectNamesWithMultipleGuids.Add("Esamir_HART", 24); // VS - 6 repair silos
            objectNamesWithMultipleGuids.Add("Hart_Forseral", 48); // TR - 12 repair silos
            objectNamesWithMultipleGuids.Add("Hart_Hossin", 24); // NC - 6 repair silos

            // First iterate over structures in the correct order
            foreach (var obj in mapObjects.Where(x => structuresWithGuids.Contains(x.ObjectType, StringComparer.OrdinalIgnoreCase)).OrderBy(x => x.ObjectType).ThenBy(x => x.AbsX).ThenBy(x => x.AbsY).ThenBy(x => x.AbsZ))
            {
                obj.GUID = currentGUID;
                currentGUID++;
                if(objectTypesWithMultipleGuids.ContainsKey(obj.ObjectType)) currentGUID += objectTypesWithMultipleGuids[obj.ObjectType]; // Skip reserved GUIDS
                if (objectNamesWithMultipleGuids.ContainsKey(obj.ObjectName)) currentGUID += objectNamesWithMultipleGuids[obj.ObjectName]; // Skip reserved GUIDS

            }

            // Then do everything else
            foreach (var obj in mapObjects.Where(x => entitiesWithGuids.Contains(x.ObjectType, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => x.ObjectType).ThenBy(x => x.AbsX).ThenBy(x => x.AbsY).ThenBy(x => x.AbsZ))
            {
                obj.GUID = currentGUID;
                currentGUID++;
                if(objectTypesWithMultipleGuids.ContainsKey(obj.ObjectType)) currentGUID += objectTypesWithMultipleGuids[obj.ObjectType]; // Skip reserved GUIDS
                if (objectNamesWithMultipleGuids.ContainsKey(obj.ObjectName)) currentGUID += objectNamesWithMultipleGuids[obj.ObjectName]; // Skip reserved GUIDS
            }
        }
    }
}