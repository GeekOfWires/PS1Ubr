using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPOReader;

namespace AmenityExtractor
{
    public static class GUIDAssigner
    {
        public static void AssignGUIDs(List<PlanetSideObject> mapObjects, List<string> structuresWithGuids, List<string> entitiesWithGuids)
        {
            var currentGUID = 1;

            // Some objects reserve an extra 2 guids for unknown reasons currently, so we need to skip ahead if one of those is found
            var objectsWithMultipleGuids = new Dictionary<string, int>()
            {
                { "amp_station", 2 }, { "comm_station", 2 }, {"comm_station_dsp", 2}, {"cryo_facility", 2 }, {"tech_plant", 2}, {"pad_landing_frame", 2},
                { "pad_landing_tower_frame", 2}, {"repair_silo", 3}
            };


            // First iterate over structures in the correct order
            foreach (var obj in mapObjects.Where(x => structuresWithGuids.Contains(x.ObjectType)).OrderBy(x => x.ObjectType).ThenBy(x => x.AbsX))
            {
                obj.GUID = currentGUID;
                currentGUID++;
                if(objectsWithMultipleGuids.ContainsKey(obj.ObjectType)) currentGUID += objectsWithMultipleGuids[obj.ObjectType]; // Skip reserved GUIDS
            }

            // Then do everything else
            foreach (var obj in mapObjects.Where(x => entitiesWithGuids.Contains(x.ObjectType))
                .OrderBy(x => x.ObjectType).ThenBy(x => x.AbsX))
            {
                obj.GUID = currentGUID;
                currentGUID++;
                if(objectsWithMultipleGuids.ContainsKey(obj.ObjectType)) currentGUID += objectsWithMultipleGuids[obj.ObjectType]; // Skip reserved GUIDS
            }
        }
    }
}
