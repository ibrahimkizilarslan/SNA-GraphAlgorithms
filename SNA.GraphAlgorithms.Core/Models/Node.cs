using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNA.GraphAlgorithms.Core.Models
{
    public class Node
    {
        public int Id { get; set; }

        // For searching by name in the future
        public string Name { get; set; } = string.Empty;

        // SNA Attributes
        public double Activity { get; set; }      // Node activity level
        public int InteractionCount { get; set; } // Interaction count
        public int ConnectionCount { get; set; }  // Degree / total connections

        // IDs of neighbor nodes
        public List<int> Neighbors { get; set; } = new List<int>();

        // Optional position data for A* algorithm
        // Defaults to (0,0) if not set
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        
        /// Calculates Euclidean distance between two nodes
        /// Used for A* heuristic
        
        public double DistanceTo(Node other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

