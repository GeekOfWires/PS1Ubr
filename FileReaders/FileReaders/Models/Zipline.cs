using System.Collections.Generic;
using System.Numerics;

namespace FileReaders.Models
{
    public class Zipline
    {
        public int PathId { get; set; }
        public bool IsTeleporter { get; set; }
        public List<Vector3> PathPoints { get; set; } = new List<Vector3>();
    }
}
