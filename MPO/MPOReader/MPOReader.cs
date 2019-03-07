using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MPOReader
{
    public static class MPOReader
    {
        public static List<MapObject> ReadMPOFile(string folderPath, string mapNumber)
        {
            var mpoList = Directory.GetFiles(folderPath, "*.mpo", SearchOption.AllDirectories);
            var mpoPath = mpoList.Single(x => x.EndsWith($"contents_map{mapNumber}.mpo"));

            var mapNames = ReadMapNames(mpoPath);
            var pseLinks = LSTReader.ReadLSTFile(folderPath, mapNumber);
            var mapObjects = ReadMapObjects(mpoPath, mapNames, pseLinks);

            return mapObjects;
        }

        private static List<string> ReadMapNames(string filePath)
        {
            var mapNamesStart = FindBytes(filePath, Encoding.ASCII.GetBytes("map_names")) + 18; // Skip to end of map_names + 9 blank bytes
            var mapNames = new List<string>();
            using (FileStream fs = File.OpenRead(filePath))
            {
                fs.Seek(mapNamesStart, SeekOrigin.Begin);
                var buffer = new byte[4];
                fs.Read(buffer, 0, buffer.Length);
                var mapNamesLength = BitConverter.ToInt32(buffer); // Total byte length of upcoming objects + mapNamesStart bytes

                buffer = new byte[2];
                fs.Read(buffer, 0, buffer.Length);
                short mapObjectsTotal = BitConverter.ToInt16(buffer); // Total number of upcoming objects


                buffer = new byte[1];
                fs.Read(buffer, 0, buffer.Length); // Skip 1 byte ahead to align data stream


                buffer = new byte[mapNamesLength - 4];
                fs.Read(buffer, 0, buffer.Length);

                // map_names are delimited by a NUL (0x00) character. We need to split the byte array on that first.
                IEnumerable<byte[]> mapNamesByteArray = SplitByteArray(buffer, 0x00);

                foreach (var bytes in mapNamesByteArray)
                {
                    mapNames.Add(Encoding.ASCII.GetString(bytes.Skip(1).ToArray())); // Skip the NUL delimiter byte
                }

                return mapNames;
            }
        }

        private static List<MapObject> ReadMapObjects(string filePath, List<string> mapNames, List<Pse_link> pseLinks)
        {
            var mapObjectsStart = FindBytes(filePath, Encoding.ASCII.GetBytes("map_objects")) + 18; // Skip to end of map_objects + 7 blank bytes
            var mapObjects = new List<MapObject>();

            using (FileStream fs = File.OpenRead(filePath))
            {
                fs.Seek(mapObjectsStart, SeekOrigin.Begin);
                var buffer = new byte[4];
                fs.Read(buffer, 0, buffer.Length);
                var mapObjectsLength = BitConverter.ToInt32(buffer); // Total byte length of upcoming objects + mapObjectsTotal bytes
                fs.Read(buffer, 0, buffer.Length);
                var mapObjectsTotal = BitConverter.ToInt32(buffer); // Total number of upcoming objects

                for (int i = 0; i < mapObjectsTotal; i++)
                {
                    var objectBuffer = new byte[40]; // Each object is 40 bytes long
                    fs.Read(objectBuffer, 0, objectBuffer.Length);
                    var mapObject = new MapObject()
                    {
                        // Using LINQ to pull bytes out can be quite slow, but since the MPO files are usually quite small it should be fine.
                        MapNamesObjectTypeIndex = BitConverter.ToInt16(objectBuffer.Take(2).ToArray()),
                        MapNamesObjectNameIndex = BitConverter.ToInt16(objectBuffer.Skip(2).Take(2).ToArray()),
                        HorizontalPosition = BitConverter.ToSingle(objectBuffer.Skip(4).Take(4).ToArray()),
                        VerticalPosition = BitConverter.ToSingle(objectBuffer.Skip(8).Take(4).ToArray()),
                        HeightPosition = BitConverter.ToSingle(objectBuffer.Skip(12).Take(4).ToArray()),
                        HorizontalSize = BitConverter.ToSingle(objectBuffer.Skip(16).Take(4).ToArray()),
                        VerticalSize = BitConverter.ToSingle(objectBuffer.Skip(20).Take(4).ToArray()),
                        HeightSize = BitConverter.ToSingle(objectBuffer.Skip(24).Take(4).ToArray()),
                        WestEastRotation = BitConverter.ToInt32(objectBuffer.Skip(28).Take(4).ToArray()),
                        NorthSouthRotation = BitConverter.ToInt32(objectBuffer.Skip(32).Take(4).ToArray()),
                        HorizontalRotation = BitConverter.ToInt32(objectBuffer.Skip(36).Take(4).ToArray()),
                    };

                    mapObject.ObjectName = mapNames[mapObject.MapNamesObjectNameIndex - 1];
                    mapObject.ObjectType = mapNames[mapObject.MapNamesObjectTypeIndex];
                    mapObject.LstType = pseLinks.SingleOrDefault(x => x.ObjectName.ToLower() == mapObject.ObjectName.ToLower())?.LstFile;

                    mapObjects.Add(mapObject);
                }

            }

            return mapObjects;
        }

        private static long FindBytes(string fileName, byte[] bytes)
        {
            long i, j;
            using (FileStream fs = File.OpenRead(fileName))
            {
                for (i = 0; i < fs.Length - bytes.Length; i++)
                {
                    fs.Seek(i, SeekOrigin.Begin);
                    for (j = 0; j < bytes.Length; j++)
                    {
                        if (fs.ReadByte() != bytes[j])
                        {
                            break;
                        }
                    }

                    if (j == bytes.Length)
                    {
                        break;
                    }
                }
            }
            return i;
        }

        public static IEnumerable<byte[]> SplitByteArray(IEnumerable<byte> source, byte marker)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));

            var current = new List<byte>();

            foreach (byte b in source)
            {
                if (b == marker)
                {
                    if (current.Count > 0)
                        yield return current.ToArray();

                    current.Clear();
                }

                current.Add(b);
            }

            if (current.Count > 0)
                yield return current.ToArray();
        }

    }
}
