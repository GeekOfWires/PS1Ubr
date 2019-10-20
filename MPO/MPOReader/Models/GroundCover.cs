using System;
using System.Collections.Generic;
using System.Text;

namespace FileReaders.Models
{
    public class GroundCover
    {
        public string LstType { get; set; }
        public string ObjectType { get; set; }
        public int Id { get; set; }
        public float AbsX { get; set; }
        public float AbsY { get; set; }
        public float AbsZ { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
        public int Pitch { get; set; }
        public int Yaw { get; set; }
        public int Roll { get; set; }
        public string ObjectName { get; set; }
    }
}
