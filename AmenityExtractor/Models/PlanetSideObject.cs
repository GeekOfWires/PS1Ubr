namespace AmenityExtractor.Models
{
    public class PlanetSideObject
    {
        private string objectType;

        public int Id { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { 
            get => objectType; 
            set  {
                if (value.StartsWith("!")) HadBangPrefix = true;
                objectType = value;
            }
        }
        public int? Owner { get; set; }

        public float AbsX { get; set; }
        public float AbsY { get; set; }
        public float AbsZ { get; set; }

        public double Yaw { get; set; }

        public int? GUID { get; set; }

        // The MapID is the index the object appears in the MPO file (if at all), plus one to begin the index from 1 not 0.
        // This is primarily used in the BIUM packet and is also known as the ModelID on the server side currently
        public int? MapID { get; set; }

        // If the entity has "!" prefixing ObjectType at any point it is considered an "inside" or "ownable" object
        // and appears in the second portion of entities during GUID assignment
        public bool HadBangPrefix { get; set; }
    }
}