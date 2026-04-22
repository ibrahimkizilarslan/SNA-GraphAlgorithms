using SNA.GraphAlgorithms.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Models
{
    
    /// Undirected weighted graph structure
    /// Uses adjacency list to store neighbors and edge data
    
    public class Graph
    {
        // Node list (public collection)
        public List<Node> Nodes { get; } = new List<Node>();

        // Edge list (public collection)
        public List<Edge> Edges { get; } = new List<Edge>();

        // Fast access Map: Id -> Node
        public Dictionary<int, Node> NodeById { get; } = new Dictionary<int, Node>();

        // Adjacency List: List of connected edges for each node
        private Dictionary<Node, List<Edge>> adjacencyList = new Dictionary<Node, List<Edge>>();

        
        /// Access to adjacency list (read-only)
        
        public IReadOnlyDictionary<Node, List<Edge>> AdjacencyList => adjacencyList;

        
        /// Adds a new node to the graph
        
        /// <param name="node">Node to add</param>
        /// <exception cref="InvalidOperationException">If a node with the same ID already exists</exception>
        public void AddNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            // Check for duplicate nodes
            if (NodeById.ContainsKey(node.Id))
                throw new InvalidOperationException($"Node with Id {node.Id} already exists.");

            Nodes.Add(node);
            NodeById[node.Id] = node;
            adjacencyList[node] = new List<Edge>();
        }

        
        /// Creates an edge between two nodes
        /// Weight is automatically calculated using WeightCalculator
        
        /// <param name="fromId">Source node ID</param>
        /// <param name="toId">Target node ID</param>
        /// <param name="isDirected">Is the edge directed? (Default: false)</param>
        /// <exception cref="InvalidOperationException">If nodes do not exist or self-looping is attempted</exception>
        public void AddEdge(int fromId, int toId, bool isDirected = false)
        {
            // Ensure nodes exist
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                throw new InvalidOperationException("Both nodes must exist before adding an edge.");

            // Prevent self-loops
            if (fromId == toId)
                throw new InvalidOperationException($"Self-loop is not allowed. Cannot add edge from node {fromId} to itself.");

            var fromNode = NodeById[fromId];
            var toNode = NodeById[toId];

            // Calculate weight using WeightCalculator
            double weight = WeightCalculator.Calculate(fromNode, toNode);

            // Create edge
            var edge = new Edge
            {
                FromNodeId = fromId,
                ToNodeId = toId,
                Weight = weight,
                IsDirected = isDirected
            };

            Edges.Add(edge);

            // Add to adjacency list
            adjacencyList[fromNode].Add(edge);

            // Update neighbor lists
            if (!fromNode.Neighbors.Contains(toId))
                fromNode.Neighbors.Add(toId);

            // If undirected, add reverse edge too
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

        
        /// Adds an edge with manual weight (bypass WeightCalculator)
        
        public void AddEdge(int fromId, int toId, double weight, bool isDirected = false)
        {
            // Ensure nodes exist control
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                throw new InvalidOperationException("Both nodes must exist before adding an edge.");

            // Self-loop control
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

        
        /// Returns edges going to neighbors of a node
        
        public List<Edge> GetEdges(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return adjacencyList.ContainsKey(node) ? adjacencyList[node] : new List<Edge>();
        }

        
        /// Returns node by ID
        
        public Node? GetNode(int id)
        {
            NodeById.TryGetValue(id, out var node);
            return node;
        }

       
        /// Checks if an edge exists between two nodes
        
        public bool EdgeExists(int fromId, int toId)
        {
            if (!NodeById.ContainsKey(fromId))
                return false;

            var fromNode = NodeById[fromId];
            return adjacencyList.ContainsKey(fromNode) && 
                   adjacencyList[fromNode].Any(e => e.ToNodeId == toId);
        }

        
        /// Removes a node and all associated edges
        
        public bool RemoveNode(int nodeId)
        {
            if (!NodeById.ContainsKey(nodeId))
                return false;

            var node = NodeById[nodeId];

            // Remove all edges connected to this node
            foreach (var otherNode in Nodes)
            {
                otherNode.Neighbors.Remove(nodeId);
                if (adjacencyList.ContainsKey(otherNode))
                {
                    adjacencyList[otherNode].RemoveAll(e => e.ToNodeId == nodeId);
                }
            }

            // Remove the node
            adjacencyList.Remove(node);
            NodeById.Remove(nodeId);
            Nodes.Remove(node);

            return true;
        }

        
        /// Removes edge between two nodes
        
        public bool RemoveEdge(int fromId, int toId)
        {
            if (!NodeById.ContainsKey(fromId) || !NodeById.ContainsKey(toId))
                return false;

            var fromNode = NodeById[fromId];
            var toNode = NodeById[toId];

            // Remove edges (both directions)
            int removed = Edges.RemoveAll(e => 
                (e.FromNodeId == fromId && e.ToNodeId == toId) ||
                (e.FromNodeId == toId && e.ToNodeId == fromId));

            if (removed == 0)
                return false;

            // Remove from adjacency list
            if (adjacencyList.ContainsKey(fromNode))
                adjacencyList[fromNode].RemoveAll(e => e.ToNodeId == toId);
            if (adjacencyList.ContainsKey(toNode))
                adjacencyList[toNode].RemoveAll(e => e.ToNodeId == fromId);

            // Update neighbor lists
            fromNode.Neighbors.Remove(toId);
            toNode.Neighbors.Remove(fromId);

            return true;
        }

        
        /// Checks if the graph is empty
        
        public bool IsEmpty()
        {
            return Nodes.Count == 0;
        }

        
        /// Clears all nodes and edges from the graph
        
        public void Clear()
        {
            Nodes.Clear();
            Edges.Clear();
            NodeById.Clear();
            adjacencyList.Clear();
        }

        
        /// Returns graph statistics (SNA metrics)
        
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

