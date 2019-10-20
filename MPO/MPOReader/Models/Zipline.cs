using System.Collections.Generic;

namespace FileReaders.Models
{
    class Zipline
    {
        public bool IsTeleporter { get; set; }
        public List<(float, float, float)> PathPoints { get; set; } = new List<(float, float, float)>();
    }
}
