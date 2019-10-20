using System;
using System.Collections.Generic;
using System.Text;

namespace FileReaders.Models
{
    public class Pe_Edit
    {
        public string ObjectName { get; set; }
        public int ID { get; set; }
        public float RelX { get; set; }
        public float RelY { get; set; }
        public float RelZ { get; set; }
        public string Unk1 { get; set; }
        public string Unk2 { get; set; }
        public string Unk3 { get; set; }
        public string AdditionalType { get; set; } // For objects such as ad billboards that have an addition type of "inside1/outside/inside2"
    }
}
