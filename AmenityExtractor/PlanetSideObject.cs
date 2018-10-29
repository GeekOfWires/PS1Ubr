namespace AmenityExtractor
{
    public class PlanetSideObject
    {
        public int Id { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public int? Owner { get; set; }
        
        public float AbsX { get; set; }
        public float AbsY { get; set; }
        public float AbsZ { get; set; }

        public double Yaw { get; set; }

        public int? GUID { get; set; }
    }
}