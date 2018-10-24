using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PS1Ubr;
using System.Threading.Tasks;

namespace AmenityExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var uberData = new List<CUberData>();
            Parallel.ForEach(Directory.GetFiles("C:\\temp", "*.ubr"),
                file => { uberData.Add(UBRReader.GetUbrData(file)); });

            var entities = uberData.SelectMany(x => x.Entries).Select(x => x.Name).Distinct();
        }

        private static double PS1RotationToDegrees(int rotation)
        {
            // 16384 is the maximum rotation from the MPO files, which is equal to 360 or 0 degrees.
            var incrementsPerDegree = 16384 / 360;

            return rotation / incrementsPerDegree;
        }

        private static double DegreesToRadians(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        private static (double, double) RotateXY(float x, float y, float radians)
        {
            var rotX = x * Math.Cos(radians) - y * Math.Sin(radians);
            var rotY = y * Math.Cos(radians) + x * Math.Sin(radians);

            return (rotX, rotY);
        }
    }
}
