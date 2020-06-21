using AmenityExtractor.Extensions;
using AmenityExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmenityExtractor
{
    public static class SanityChecker
    {
        public static void CheckObjectCounts(List<PlanetSideObject> identifiableObjects, string mapName, string mapNumber)
        {
            if (typeof(ExpectedObjectCounts).GetField(mapName) != null)
            {
                Console.WriteLine($"Sanity checking object counts on map {mapNumber} {mapName}");
                Console.ForegroundColor = ConsoleColor.Red;
                var expectedCounts = (Dictionary<string, int>)typeof(ExpectedObjectCounts).GetField(mapName).GetValue(null);
                var objectCounts = identifiableObjects.GroupBy(x => x.ObjectType)
                    .Select(group => new { Name = group.Key, Count = group.Count() })
                    .OrderBy(x => x.Name);
                foreach (var item in objectCounts)
                {
                    if (expectedCounts.ContainsKey(item.Name) && item.Count != expectedCounts[item.Name])
                        Console.WriteLine(
                            $"Mismatch: {item.Name}, Got: {item.Count} Expected: {expectedCounts[item.Name]}");
                }

                var expectingTotal = ExpectedObjectCounts.Ishundar.Sum(x => x.Value);

                if (expectingTotal != identifiableObjects.Count)
                {
                    Console.WriteLine(
                        $"Expecting {expectingTotal} entities, found {identifiableObjects.Count}. {expectingTotal - identifiableObjects.Count} missing");
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void CheckGuidRanges(List<PlanetSideObject> identifiableObjects, string mapName, string mapNumber)
        {
            if (typeof(ExpectedGuids).GetField(mapName) != null)
            {
                Console.WriteLine($"Sanity checking GUIDs on map {mapNumber} {mapName}");
                Console.ForegroundColor = ConsoleColor.Red;
                var (topLevelGuids, childGuids) =
                    ((Dictionary<string, IEnumerable<int>> topLevelObjects,
                    Dictionary<string, IEnumerable<int>> childObjects))typeof(ExpectedGuids).GetField(mapName).GetValue(null);


                void CheckGuidRanges(Dictionary<string, IEnumerable<int>> expectedGuids, bool isChildGuids)
                {
                    foreach ((string objectType, IEnumerable<int> guids) in topLevelGuids)
                    {
                        var objects = identifiableObjects.Where(x => x.ObjectType == objectType && x.IsChildObject == isChildGuids);
                        if (objects.Any())
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
                }

                CheckGuidRanges(topLevelGuids, isChildGuids: false);
                CheckGuidRanges(topLevelGuids, isChildGuids: true);

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
