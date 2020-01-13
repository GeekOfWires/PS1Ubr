using System.Collections.Generic;

namespace FileReaders.Models
{
    public class Zipline
    {
        public int PathId { get; set; }
        public bool IsTeleporter { get; set; }
        public List<(float, float, float)> PathPoints { get; set; } = new List<(float, float, float)>();
    }
}
