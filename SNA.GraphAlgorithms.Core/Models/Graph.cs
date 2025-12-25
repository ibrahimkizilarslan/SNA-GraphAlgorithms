using SNA.GraphAlgorithms.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Models
{
    /// <summary>
    /// Undirected weighted graph yapısı
    /// Adjacency list ile node'ların komşularını ve edge'lerini tutar
    /// </summary>
    public class Graph
    {
        // Düğüm listesi (koleksiyon olarak public)
        public List<Node> Nodes { get; } = new List<Node>();

        // Kenar listesi (koleksiyon olarak public)
        public List<Edge> Edges { get; } = new List<Edge>();

        // Hızlı erişim için Id -> Node map
        public Dictionary<int, Node> NodeById { get; } = new Dictionary<int, Node>();

        // Adjacency List: Her node için bağlı olduğu edge'lerin listesi
        private Dictionary<Node, List<Edge>> adjacencyList = new Dictionary<Node, List<Edge>>();

        /// <summary>
        /// Adjacency list'e erişim (read-only)
        /// </summary>
        public IReadOnlyDictionary<Node, List<Edge>> AdjacencyList => adjacencyList;

        /// <summary>
        /// Graph'a yeni bir node ekler
        /// </summary>
        /// <param name="node">Eklenecek node</param>
        /// <exception cref="InvalidOperationException">Aynı ID'ye sahip node zaten varsa</exception>
        public void AddNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            // Duplicate node kontrolü
            if (NodeById.ContainsKey(node.Id))
                throw new InvalidOperationException($"Node with Id {node.Id} already exists.");

            Nodes.Add(node);
            NodeById[node.Id] = node;
            adjacencyList[node] = new List<Edge>();
        }

        /// <summary>
        /// İki node arasında edge oluşturur
        /// Weight'i otomatik olarak WeightCalculator ile hesaplar
        /// </summary>
        /// <param name="fromId">Kaynak node ID</param>
        /// <param name="toId">Hedef node ID</param>
        /// <param name="isDirected">Yönlü edge mi? (Default: false)</param>
        /// <exception cref="InvalidOperationException">Node'lar yoksa veya self-loop ise</exception>
        public void AddEdge(int fromId, int toId, bool isDirected = false)
        {
            // Node'ların varlık kontrolü
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                throw new InvalidOperationException("Both nodes must exist before adding an edge.");

            // Self-loop kontrolü
            if (fromId == toId)
                throw new InvalidOperationException($"Self-loop is not allowed. Cannot add edge from node {fromId} to itself.");

            var fromNode = NodeById[fromId];
            var toNode = NodeById[toId];

            // Weight'i WeightCalculator ile hesapla
            double weight = WeightCalculator.Calculate(fromNode, toNode);

            // Edge oluştur
            var edge = new Edge
            {
                FromNodeId = fromId,
                ToNodeId = toId,
                Weight = weight,
                IsDirected = isDirected
            };

            Edges.Add(edge);

            // Adjacency list'e ekle
            adjacencyList[fromNode].Add(edge);

            // Komşuluk listelerini güncelle
            if (!fromNode.Neighbors.Contains(toId))
                fromNode.Neighbors.Add(toId);

            // Undirected ise ters yönü de ekle
            if (!isDirected)
            {
                var reverseEdge = new Edge
                {
                    FromNodeId = toId,
                    ToNodeId = fromId,
                    Weight = weight,
                    IsDirected = false
                };

                Edges.Add(reverseEdge);
                adjacencyList[toNode].Add(reverseEdge);

                if (!toNode.Neighbors.Contains(fromId))
                    toNode.Neighbors.Add(fromId);
            }
        }

        /// <summary>
        /// Manuel weight ile edge ekler (WeightCalculator kullanmadan)
        /// </summary>
        public void AddEdge(int fromId, int toId, double weight, bool isDirected = false)
        {
            // Node'ların varlık kontrolü
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                throw new InvalidOperationException("Both nodes must exist before adding an edge.");

            // Self-loop kontrolü
            if (fromId == toId)
                throw new InvalidOperationException($"Self-loop is not allowed. Cannot add edge from node {fromId} to itself.");

            var fromNode = NodeById[fromId];
            var toNode = NodeById[toId];

            var edge = new Edge
            {
                FromNodeId = fromId,
                ToNodeId = toId,
                Weight = weight,
                IsDirected = isDirected
            };

            Edges.Add(edge);
            adjacencyList[fromNode].Add(edge);

            if (!fromNode.Neighbors.Contains(toId))
                fromNode.Neighbors.Add(toId);

            if (!isDirected)
            {
                var reverseEdge = new Edge
                {
                    FromNodeId = toId,
                    ToNodeId = fromId,
                    Weight = weight,
                    IsDirected = false
                };

                Edges.Add(reverseEdge);
                adjacencyList[toNode].Add(reverseEdge);

                if (!toNode.Neighbors.Contains(fromId))
                    toNode.Neighbors.Add(fromId);
            }
        }

        /// <summary>
        /// Bir node'un komşularına giden edge'leri döndürür
        /// </summary>
        public List<Edge> GetEdges(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return adjacencyList.ContainsKey(node) ? adjacencyList[node] : new List<Edge>();
        }

        /// <summary>
        /// ID'ye göre node döndürür
        /// </summary>
        public Node? GetNode(int id)
        {
            NodeById.TryGetValue(id, out var node);
            return node;
        }

        /// <summary>
        /// İki node arasında edge olup olmadığını kontrol eder
        /// </summary>
        public bool EdgeExists(int fromId, int toId)
        {
            if (!NodeById.ContainsKey(fromId))
                return false;

            var fromNode = NodeById[fromId];
            return adjacencyList.ContainsKey(fromNode) && 
                   adjacencyList[fromNode].Any(e => e.ToNodeId == toId);
        }

        /// <summary>
        /// Bir node'u ve ilişkili tüm edge'leri siler
        /// </summary>
        public bool RemoveNode(int nodeId)
        {
            if (!NodeById.ContainsKey(nodeId))
                return false;

            var node = NodeById[nodeId];

            // Bu node'a bağlı tüm edge'leri sil
            Edges.RemoveAll(e => e.FromNodeId == nodeId || e.ToNodeId == nodeId);

            // Diğer node'ların komşu listelerinden bu node'u çıkar
            foreach (var otherNode in Nodes)
            {
                otherNode.Neighbors.Remove(nodeId);
                if (adjacencyList.ContainsKey(otherNode))
                {
                    adjacencyList[otherNode].RemoveAll(e => e.ToNodeId == nodeId);
                }
            }

            // Node'u kaldır
            adjacencyList.Remove(node);
            NodeById.Remove(nodeId);
            Nodes.Remove(node);

            return true;
        }

        /// <summary>
        /// İki node arasındaki edge'i siler
        /// </summary>
        public bool RemoveEdge(int fromId, int toId)
        {
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                return false;

            var fromNode = NodeById[fromId];
            var toNode = NodeById[toId];

            // Edge'leri kaldır (her iki yön)
            int removed = Edges.RemoveAll(e => 
                (e.FromNodeId == fromId && e.ToNodeId == toId) ||
                (e.FromNodeId == toId && e.ToNodeId == fromId));

            if (removed == 0)
                return false;

            // Adjacency list'ten kaldır
            if (adjacencyList.ContainsKey(fromNode))
                adjacencyList[fromNode].RemoveAll(e => e.ToNodeId == toId);
            if (adjacencyList.ContainsKey(toNode))
                adjacencyList[toNode].RemoveAll(e => e.ToNodeId == fromId);

            // Komşu listelerini güncelle
            fromNode.Neighbors.Remove(toId);
            toNode.Neighbors.Remove(fromId);

            return true;
        }

        /// <summary>
        /// Graf boş mu kontrol eder
        /// </summary>
        public bool IsEmpty()
        {
            return Nodes.Count == 0;
        }

        /// <summary>
        /// Tüm node ve edge'leri temizler
        /// </summary>
        public void Clear()
        {
            Nodes.Clear();
            Edges.Clear();
            NodeById.Clear();
            adjacencyList.Clear();
        }

        /// <summary>
        /// Graf istatistiklerini döndürür
        /// </summary>
        public (int NodeCount, int EdgeCount, double Density, double AvgDegree) GetStatistics()
        {
            int nodeCount = Nodes.Count;
            int edgeCount = Edges.Count / 2; // Undirected

            double density = 0;
            double avgDegree = 0;

            if (nodeCount > 1)
            {
                double maxEdges = (double)nodeCount * (nodeCount - 1) / 2;
                density = edgeCount / maxEdges;
                avgDegree = Nodes.Average(n => n.Neighbors.Count);
            }

            return (nodeCount, edgeCount, density, avgDegree);
        }
    }
}

