using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Depth-First Search (Derinlik Öncelikli Arama) algoritması
    /// Graph üzerinde mümkün olduğunca derine iner
    /// </summary>
    public class DFS : IGraphAlgorithm
    {
        public string Name => "Depth-First Search (DFS)";

        /// <summary>
        /// DFS algoritmasını çalıştırır
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

            // DFS'i başlat
            DFSRecursive(graph, startNodeId, visited, visitedOrder);

            return visitedOrder;
        }

        /// <summary>
        /// Recursive DFS helper metodu
        /// </summary>
        private void DFSRecursive(Graph graph, int currentId, HashSet<int> visited, List<int> visitedOrder)
        {
            // Node'u ziyaret et
            visited.Add(currentId);
            visitedOrder.Add(currentId);

            var currentNode = graph.GetNode(currentId);
            if (currentNode == null)
                return;

            // Komşuları recursive olarak ziyaret et
            foreach (int neighborId in currentNode.Neighbors)
            {
                if (!visited.Contains(neighborId))
                {
                    DFSRecursive(graph, neighborId, visited, visitedOrder);
                }
            }
        }
    }
}

