using System;
using System.Collections.Generic;
using System.Text;

namespace FileReaders.Models
{
    public class Pse_RelativeObject
    {
        public string ObjectName { get; set; }
        public float RelX { get; set; }
        public float RelY { get; set; }
        public float RelZ { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }
    }
}
