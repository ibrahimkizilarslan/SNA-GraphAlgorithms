using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNA.GraphAlgorithms.Core.Models
{
    public class Edge
    {
        public int FromNodeId { get; set; }
        public int ToNodeId { get; set; }

        // Dinamik hesaplanacak ağırlık (formülden gelecek)
        public double Weight { get; set; }

        // Grafımız yönsüz, ama istersen flag dursun
        public bool IsDirected { get; set; } = false;
    }
}
