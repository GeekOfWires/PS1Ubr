using System;
using System.Collections.Generic;

namespace MPOReader
{
    public class MapObject
    {
        public Int16 MapNamesObjectTypeIndex { get; set; }
        public Int16 MapNamesObjectNameIndex { get; set; }
        public float HorizontalPosition { get; set; }
        public float VerticalPosition { get; set; }
        public float HeightPosition { get; set; }
        public float HorizontalSize { get; set; }
        public float VerticalSize { get; set; }
        public float HeightSize { get; set; }
        public Int32 WestEastRotation { get; set; }
        public Int32 NorthSouthRotation { get; set; }
        public Int32 HorizontalRotation { get; set; }

        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public string LstType { get; set; } // This is the .lst file associated with this object that contains the additional mesh items

        public string WarpCoordinates => String.Join(" ", new List<int>{(int) Math.Round(HorizontalPosition), (int)Math.Round(VerticalPosition), (int) Math.Round(HeightPosition) });
    }
}