using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    
    /// Bağlı Bileşenler (Connected Components) Algoritması
    /// Graf içindeki ayrık toplulukları (disjoint communities) bulur
    
    public class ConnectedComponents : IGraphAlgorithm
    {
        public string Name => "Connected Components (Disjoint Communities)";

        // Son çalıştırmadan kalan bileşen bilgileri
        private Dictionary<int, int> nodeToComponent = new Dictionary<int, int>();
        private List<List<int>> components = new List<List<int>>();

        
        /// Bağlı bileşenleri bulur
        
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Kullanılmıyor (tüm graf taranır)</param>
        /// <returns>İlk bileşendeki düğümlerin ID listesi</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            // Önceki sonuçları temizle
            nodeToComponent = new Dictionary<int, int>();
            components = new List<List<int>>();

            if (graph.Nodes.Count == 0)
                return new List<int>();

            // Ziyaret edilmemiş düğümleri takip et
            var visited = new HashSet<int>();
            int componentId = 0;

            // Her ziyaret edilmemiş düğümden BFS/DFS başlat
            foreach (var node in graph.Nodes)
            {
                if (!visited.Contains(node.Id))
                {
                    var component = new List<int>();
                    BFSComponent(graph, node.Id, visited, component, componentId);
                    components.Add(component);
                    componentId++;
                }
            }

            // İlk bileşeni döndür (en büyük bileşen olabilir)
            return components.Count > 0 ? components[0] : new List<int>();
        }

        
        /// BFS ile bir bileşeni keşfeder
        /
        private void BFSComponent(Graph graph, int startId, HashSet<int> visited, List<int> component, int componentId)
        {
            var queue = new Queue<int>();
            queue.Enqueue(startId);
            visited.Add(startId);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                component.Add(currentId);
                nodeToComponent[currentId] = componentId;

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null)
                    continue;

                foreach (int neighborId in currentNode.Neighbors)
                {
                    if (!visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }
        }

        
        /// Bileşen sayısını döndürür
        
        public int GetComponentCount()
        {
            return components.Count;
        }

        
        /// Tüm bileşenleri döndürür
        
        public List<List<int>> GetAllComponents()
        {
            return components.Select(c => new List<int>(c)).ToList();
        }

        
        /// Belirli bir düğümün hangi bileşende olduğunu döndürür
        
        public int GetComponentId(int nodeId)
        {
            if (!nodeToComponent.ContainsKey(nodeId))
                throw new InvalidOperationException($"Node {nodeId} not found. Execute() must be called first.");

            return nodeToComponent[nodeId];
        }

        
        /// En büyük bileşeni döndürür
        
        public List<int> GetLargestComponent()
        {
            if (components.Count == 0)
                return new List<int>();

            return components.OrderByDescending(c => c.Count).First();
        }

        
        /// Belirli bir bileşeni döndürür
        
        public List<int> GetComponent(int componentId)
        {
            if (componentId < 0 || componentId >= components.Count)
                throw new ArgumentOutOfRangeException(nameof(componentId));

            return new List<int>(components[componentId]);
        }

        
        /// İki düğümün aynı bileşende olup olmadığını kontrol eder
        
        public bool AreConnected(int nodeId1, int nodeId2)
        {
            if (!nodeToComponent.ContainsKey(nodeId1) || !nodeToComponent.ContainsKey(nodeId2))
                return false;

            return nodeToComponent[nodeId1] == nodeToComponent[nodeId2];
        }

        
        /// Grafın bağlı olup olmadığını kontrol eder
        
        public bool IsGraphConnected()
        {
            return components.Count == 1;
        }
    }
}