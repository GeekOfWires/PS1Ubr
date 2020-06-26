namespace AmenityExtractor.Models
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

        // The MapID is the index the object appears in the MPO file (if at all), plus one to begin the index from 1 not 0.
        // This is primarily used in the BIUM packet and is also known as the ModelID on the server side currently
        // It is also used in LST files, but the ranges on these seem to be somewhat arbitrary (10000+) and irrelevant to our needs
        public int? MapID { get; set; }

        // If the entity has "!" prefixing ObjectType at any point it is considered an "inside" or "ownable" object
        // and appears in the second portion of entities during GUID assignment
        // The same is true for any object that is included as a sub-object of another, for example an equipment_terminal within a tech_plant
        public bool IsChildObject { get; set; }
    }
}