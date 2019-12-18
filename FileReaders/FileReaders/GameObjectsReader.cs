using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileReaders
{
    public class GameObjectsReader
    {
        public static IEnumerable<List<string>> data = null;
        private static string _resourceParentString = "set_resource_parent";
        private static string _addPropertyString = "add_property";

        public GameObjectsReader(string basePath)
        {
            if (data == null)
            {
                data = ReadGameObjectsFile(basePath);
            }
        }

        static private IEnumerable<List<string>> ReadGameObjectsFile(string basePath)
        {
            var files = Directory.GetFiles(basePath, "game_objects.adb.lst", SearchOption.AllDirectories);
            var file = files.First();
            var allData = File.ReadAllLines(file);

            var lines = new List<List<string>>();

            foreach (var data in allData)
            {
                lines.Add(data.Split(' ').ToList());
            }

            return lines;
        }

        public IEnumerable<List<string>> GetByObjectName(string objectName)
        {
            var resourceParent = data.FirstOrDefault(x => x[0] == _resourceParentString && x[1] == objectName);
            var resourceParentProperties = new List<List<string>>();
            if (resourceParent != null)
            {
                var resourceParentName = resourceParent[3];
                resourceParentProperties = data.Where(x => x[0] == _addPropertyString && x[1] == resourceParentName).ToList();
            }

            var properties = data.Where(x => x[0] == _addPropertyString && x[1] == objectName).Concat(resourceParentProperties);

            return properties;
        }
    }
}
