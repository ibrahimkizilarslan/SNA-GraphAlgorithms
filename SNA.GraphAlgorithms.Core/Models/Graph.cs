using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNA.GraphAlgorithms.Core.Models
{
    public class Graph
    {
        // Düğüm listesi
        public List<Node> Nodes { get; } = new List<Node>();

        // Kenar listesi
        public List<Edge> Edges { get; } = new List<Edge>();

        // Hızlı erişim için Id -> Node map
        public Dictionary<int, Node> NodeById { get; } = new Dictionary<int, Node>();

        public void AddNode(Node node)
        {
            if (NodeById.ContainsKey(node.Id))
                throw new InvalidOperationException($"Node with Id {node.Id} already exists.");

            Nodes.Add(node);
            NodeById[node.Id] = node;
        }

        public void AddEdge(int fromId, int toId, double weight, bool isDirected = false)
        {
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                throw new InvalidOperationException("Both nodes must exist before adding an edge.");

            var edge = new Edge
            {
                FromNodeId = fromId,
                ToNodeId = toId,
                Weight = weight,
                IsDirected = isDirected
            };

            Edges.Add(edge);

            // Komşuluk listelerini güncelle
            NodeById[fromId].Neighbors.Add(toId);
            if (!isDirected)
                NodeById[toId].Neighbors.Add(fromId);
        }

        public Node? GetNode(int id)
        {
            NodeById.TryGetValue(id, out var node);
            return node;
        }
    }
}
