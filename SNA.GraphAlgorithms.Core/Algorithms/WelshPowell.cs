using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Welsh-Powell Graf Renklendirme Algoritması
    /// Greedy yaklaşımla minimum sayıda renk kullanarak graf renklendirme yapar
    /// </summary>
    public class WelshPowell : IGraphAlgorithm
    {
        public string Name => "Welsh-Powell Graph Coloring";

        // Son çalıştırmadan kalan renk bilgileri
        private Dictionary<int, int> nodeColors = new Dictionary<int, int>();
        private int chromaticNumber = 0;

        /// <summary>
        /// Welsh-Powell algoritmasını çalıştırır
        /// </summary>
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Kullanılmıyor (interface uyumu için)</param>
        /// <returns>Renklendirme sırasına göre düğüm ID listesi</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            // Renkleri sıfırla
            nodeColors = new Dictionary<int, int>();
            chromaticNumber = 0;

            if (graph.Nodes.Count == 0)
                return new List<int>();

            // 1. Düğümleri degree'ye göre azalan sırada sırala
            var sortedNodes = graph.Nodes
                .OrderByDescending(n => n.Neighbors.Count)
                .ToList();

            var coloredOrder = new List<int>();

            // 2. Renklendirme işlemi
            foreach (var node in sortedNodes)
            {
                // Bu düğümün komşularının renkleri
                var neighborColors = new HashSet<int>();
                foreach (int neighborId in node.Neighbors)
                {
                    if (nodeColors.ContainsKey(neighborId))
                    {
                        neighborColors.Add(nodeColors[neighborId]);
                    }
                }

                // En küçük kullanılmayan rengi bul
                int color = 1;
                while (neighborColors.Contains(color))
                {
                    color++;
                }

                // Rengi ata
                nodeColors[node.Id] = color;
                coloredOrder.Add(node.Id);

                // Kromatik sayıyı güncelle
                if (color > chromaticNumber)
                {
                    chromaticNumber = color;
                }
            }

            return coloredOrder;
        }

        /// <summary>
        /// Belirli bir düğümün rengini döndürür
        /// Execute() çalıştırıldıktan sonra kullanılabilir
        /// </summary>
        public int GetNodeColor(int nodeId)
        {
            if (!nodeColors.ContainsKey(nodeId))
                throw new InvalidOperationException($"Node {nodeId} has not been colored. Execute() must be called first.");

            return nodeColors[nodeId];
        }

        /// <summary>
        /// Tüm düğüm renklerini döndürür
        /// </summary>
        public Dictionary<int, int> GetAllColors()
        {
            return new Dictionary<int, int>(nodeColors);
        }

        /// <summary>
        /// Kullanılan renk sayısını (kromatik sayı) döndürür
        /// </summary>
        public int GetChromaticNumber()
        {
            return chromaticNumber;
        }

        /// <summary>
        /// Aynı renkteki düğümleri gruplar halinde döndürür
        /// </summary>
        public Dictionary<int, List<int>> GetColorGroups()
        {
            var groups = new Dictionary<int, List<int>>();

            foreach (var kvp in nodeColors)
            {
                if (!groups.ContainsKey(kvp.Value))
                {
                    groups[kvp.Value] = new List<int>();
                }
                groups[kvp.Value].Add(kvp.Key);
            }

            return groups;
        }
    }
}