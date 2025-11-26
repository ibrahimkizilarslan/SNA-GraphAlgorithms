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

        // İleride isme göre arama vs. için
        public string Name { get; set; } = string.Empty;

        // Projedeki özellikler
        public double Activity { get; set; }      // Aktiflik
        public int InteractionCount { get; set; } // Etkileşim
        public int ConnectionCount { get; set; }  // Bağlantı sayısı (degree)

        // Komşu düğümlerin Id'leri
        public List<int> Neighbors { get; set; } = new List<int>();
    }
}
