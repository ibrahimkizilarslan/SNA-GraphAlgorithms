using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Dijkstra's Shortest Path algoritması
    /// Weighted graph'ta bir node'dan diğer tüm node'lara en kısa yolu bulur
    /// </summary>
    public class Dijkstra : IGraphAlgorithm
    {
        public string Name => "Dijkstra's Shortest Path";

        // Son çalıştırmadan kalan mesafe bilgileri
        private Dictionary<int, double> distances = new Dictionary<int, double>();
        private Dictionary<int, int?> previousNodes = new Dictionary<int, int?>();

        /// <summary>
        /// Dijkstra algoritmasını çalıştırır
        /// </summary>
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <returns>Ziyaret edilen düğümlerin ID listesi (shortest path tree sırasına göre)</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (!graph.NodeById.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with Id {startNodeId} not found in graph.", nameof(startNodeId));

            // Initialize
            distances = new Dictionary<int, double>();
            previousNodes = new Dictionary<int, int?>();
            var visited = new HashSet<int>();
            var visitedOrder = new List<int>();

            // Tüm node'lara sonsuz mesafe ata
            foreach (var node in graph.Nodes)
            {
                distances[node.Id] = double.PositiveInfinity;
                previousNodes[node.Id] = null;
            }

            // Başlangıç node'unun mesafesi 0
            distances[startNodeId] = 0;

            // Priority queue (mesafeye göre sıralı)
            var priorityQueue = new SortedSet<(double distance, int nodeId)>(
                Comparer<(double, int)>.Create((a, b) =>
                {
                    int result = a.Item1.CompareTo(b.Item1);
                    return result != 0 ? result : a.Item2.CompareTo(b.Item2);
                })
            );

            priorityQueue.Add((0, startNodeId));

            while (priorityQueue.Count > 0)
            {
                // En kısa mesafeli node'u al
                var (currentDistance, currentId) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                // Zaten ziyaret edildiyse atla
                if (visited.Contains(currentId))
                    continue;

                // Ziyaret et
                visited.Add(currentId);
                visitedOrder.Add(currentId);

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null)
                    continue;

                // Komşuları kontrol et
                var edges = graph.GetEdges(currentNode);
                foreach (var edge in edges)
                {
                    int neighborId = edge.ToNodeId;

                    if (visited.Contains(neighborId))
                        continue;

                    // Yeni mesafe hesapla
                    double newDistance = distances[currentId] + edge.Weight;

                    // Daha kısa bir yol bulunduysa güncelle
                    if (newDistance < distances[neighborId])
                    {
                        // Eski entry'yi kaldır (varsa)
                        priorityQueue.Remove((distances[neighborId], neighborId));

                        // Güncelle
                        distances[neighborId] = newDistance;
                        previousNodes[neighborId] = currentId;

                        // Yeni entry ekle
                        priorityQueue.Add((newDistance, neighborId));
                    }
                }
            }

            return visitedOrder;
        }

        /// <summary>
        /// Belirli bir hedef node'a en kısa yolu döndürür
        /// Execute() çalıştırıldıktan sonra kullanılabilir
        /// </summary>
        /// <param name="targetNodeId">Hedef node ID</param>
        /// <returns>Başlangıçtan hedefe olan yol (node ID listesi)</returns>
        public List<int> GetShortestPath(int targetNodeId)
        {
            if (!previousNodes.ContainsKey(targetNodeId))
                throw new InvalidOperationException("Execute() must be called before GetShortestPath().");

            var path = new List<int>();
            int? currentId = targetNodeId;

            // Eğer hedef node'a ulaşılamıyorsa
            if (distances[targetNodeId] == double.PositiveInfinity)
                return path; // Boş liste

            // Geriye doğru yolu takip et
            while (currentId.HasValue)
            {
                path.Add(currentId.Value);
                currentId = previousNodes[currentId.Value];
            }

            // Yolu ters çevir (başlangıçtan hedefe)
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Belirli bir node'a olan en kısa mesafeyi döndürür
        /// Execute() çalıştırıldıktan sonra kullanılabilir
        /// </summary>
        public double GetDistance(int nodeId)
        {
            if (!distances.ContainsKey(nodeId))
                throw new InvalidOperationException("Execute() must be called before GetDistance().");

            return distances[nodeId];
        }

        /// <summary>
        /// Tüm mesafeleri döndürür (debugging için)
        /// </summary>
        public Dictionary<int, double> GetAllDistances()
        {
            return new Dictionary<int, double>(distances);
        }
    }
}
