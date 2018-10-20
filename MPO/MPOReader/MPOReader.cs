using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MPOReader
{
    public static class MPOReader
    {
        public static List<MapObject> ReadMPOFile(string filePath)
        {
            var map_objects_start = FindBytes(filePath, Encoding.ASCII.GetBytes("map_objects")) + 18; // Skip to end of map_objects + 7 blank bytes
            var mapObjects = new List<MapObject>();

            using (FileStream fs = File.OpenRead(filePath))
            {
                fs.Seek(map_objects_start, SeekOrigin.Begin);
                var buffer = new byte[4];
                fs.Read(buffer, 0, buffer.Length);
                var map_objects_length = BitConverter.ToInt32(buffer); // Total byte length of upcoming objects + map_objects_total bytes
                fs.Read(buffer, 0, buffer.Length);
                var map_objects_total = BitConverter.ToInt32(buffer); // Total number of upcoming objects

                for (int i = 0; i < map_objects_total; i++)
                {
                    var objectBuffer = new byte[40]; // Each object is 40 bytes long
                    fs.Read(objectBuffer, 0, objectBuffer.Length);
                    var mapObject = new MapObject()
                    {
                        // Using LINQ to pull bytes out can be quite slow, but since the MPO files are usually quite small it should be fine.
                        ObjectType = BitConverter.ToInt16(objectBuffer.Take(2).ToArray()),
                        Subtype = BitConverter.ToInt16(objectBuffer.Skip(2).Take(2).ToArray()),
                        HorizontalPosition = BitConverter.ToSingle(objectBuffer.Skip(4).Take(4).ToArray()),
                        VerticalPosition = BitConverter.ToSingle(objectBuffer.Skip(8).Take(4).ToArray()),
                        HeightPosition = BitConverter.ToSingle(objectBuffer.Skip(12).Take(4).ToArray()),
                        HorizontalSize = BitConverter.ToSingle(objectBuffer.Skip(16).Take(4).ToArray()),
                        VerticalSize = BitConverter.ToSingle(objectBuffer.Skip(20).Take(4).ToArray()),
                        HeightSize = BitConverter.ToSingle(objectBuffer.Skip(24).Take(4).ToArray()),
                        WestEastRotation = BitConverter.ToInt32(objectBuffer.Skip(28).Take(4).ToArray()),
                        NorthSouthRotation = BitConverter.ToInt32(objectBuffer.Skip(32).Take(4).ToArray()),
                        HorizontalRotation = BitConverter.ToInt32(objectBuffer.Skip(36).Take(4).ToArray())
                    };

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
                        if (fs.ReadByte() != bytes[j]) break;
                    if (j == bytes.Length) break;
                }
            }
            return i;
        }

    }
}
