using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Breadth-First Search (Genişlik Öncelikli Arama) algoritması
    /// Graph üzerinde seviye seviye dolaşır
    /// </summary>
    public class BFS : IGraphAlgorithm
    {
        public string Name => "Breadth-First Search (BFS)";

        /// <summary>
        /// BFS algoritmasını çalıştırır
        /// </summary>
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <returns>Ziyaret edilen düğümlerin ID listesi (ziyaret sırasına göre)</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (!graph.NodeById.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with Id {startNodeId} not found in graph.", nameof(startNodeId));

            // Ziyaret edilen node'ların listesi
            List<int> visitedOrder = new List<int>();
            
            // Ziyaret edildi mi kontrolü
            HashSet<int> visited = new HashSet<int>();
            
            // BFS için kuyruk
            Queue<int> queue = new Queue<int>();

            // Başlangıç node'unu kuyruğa ekle
            queue.Enqueue(startNodeId);
            visited.Add(startNodeId);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                visitedOrder.Add(currentId);

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null)
                    continue;

                // Komşuları ziyaret et
                foreach (int neighborId in currentNode.Neighbors)
                {
                    if (!visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            return visitedOrder;
        }
    }
}

