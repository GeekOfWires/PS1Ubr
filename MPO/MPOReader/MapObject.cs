using System;

namespace MPOReader
{
    public class MapObject
    {
        public Int16 ObjectType { get; set; }
        public Int16 Subtype { get; set; }
        public float HorizontalPosition { get; set; }
        public float VerticalPosition { get; set; }
        public float HeightPosition { get; set; }
        public float HorizontalSize { get; set; }
        public float VerticalSize { get; set; }
        public float HeightSize { get; set; }
        public Int32 WestEastRotation { get; set; }
        public Int32 NorthSouthRotation { get; set; }
        public Int32 HorizontalRotation { get; set; }
    }
}