using System;
using System.Collections.Generic;
using System.Numerics;

namespace AmenityExtractor
{
    class MathFunctions
    {
        protected internal static double PS1RotationToDegrees(int rotation)
        {
            // 16384 is the maximum rotation from the MPO files, which is equal to 360 or 0 degrees.
            double incrementsPerDegree = 16384f / 360f;

            return Math.Round(rotation / incrementsPerDegree);
        }

        protected internal static double CounterClockwiseToClockwiseRotation(int rotation)
        {
            var ccw = PS1RotationToDegrees(rotation);
            var cw = 360 - ccw;

            return cw;
        }

        protected internal static double DegreesToRadians(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        protected static double RadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        protected internal static (float, float) RotateXY(float x, float y, double radians)
        {
            var rotX = x * Math.Cos(radians) - y * Math.Sin(radians);
            var rotY = y * Math.Cos(radians) + x * Math.Sin(radians);

            return ((float) rotX, (float) rotY);
        }

        protected internal static (double x, double y, double z) TransformToRotationDegrees(List<float> t)
        {
            var m = new Matrix4x4(t[0], t[4], t[8], t[12], t[1], t[5], t[9], t[13], t[2], t[6], t[10], t[14], t[3], t[7], t[11], t[15]);
            var q = new Quaternion();
            q.W = (float)(Math.Sqrt(1 + m.M11 + m.M22 + m.M33) / 2);
            q.X = (m.M32 - m.M23) / (q.W * 4);
            q.Y = (m.M13 - m.M31) / (q.W * 4);
            q.Z = (m.M21 - m.M12) / (q.W * 4);

            double[] axis = { 0, 0, 0 };
            var angle = 2 * Math.Acos(q.W);
            if (1 - (q.W * q.W) < 0.000001)
            {
                axis[0] = q.X;
                axis[1] = q.Y;
                axis[2] = q.Z;
            }
            else
            {
                // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/
                var s = Math.Sqrt(1 - (q.W * q.W));
                axis[0] = q.X / s;
                axis[1] = q.Y / s;
                axis[2] = q.Z / s;
            }

            var x = Math.Round(RadiansToDegrees(axis[0] * angle));
            var y = Math.Round(RadiansToDegrees(axis[1] * angle));
            var z = Math.Round(RadiansToDegrees(axis[2] * angle));

            return (x, y, z);
        }

        internal static int NormalizeDegrees(int degrees)
        {
            var norm = degrees % 360;
            if (norm < 0)
            {
                norm += 360;
            }

            return norm;
        }
    }
}