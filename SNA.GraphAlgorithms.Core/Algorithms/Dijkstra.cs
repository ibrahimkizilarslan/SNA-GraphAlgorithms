using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /
    /// Dijkstra's Shortest Path algorithm
    /// Finds the shortest path from a starting node to all other nodes in a weighted graph
    
    public class Dijkstra : IGraphAlgorithm
    {
        public string Name => "Dijkstra's Shortest Path";

        // Distance information from last execution
        private Dictionary<int, double> distances = new Dictionary<int, double>();
        private Dictionary<int, int?> previousNodes = new Dictionary<int, int?>();

        
        /// Runs Dijkstra's algorithm
        
        /// <param name="graph">Graph to operate on</param>
        /// <param name="startNodeId">Start node ID</param>
        /// <returns>List of visited node IDs (in Shortest Path Tree order)</returns>
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

            // Assign infinite distance to all nodes
            foreach (var node in graph.Nodes)
            {
                distances[node.Id] = double.PositiveInfinity;
                previousNodes[node.Id] = null;
            }

            // Start node distance is 0
            distances[startNodeId] = 0;

            // Priority queue (sorted by distance)
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
                // Get node with shortest distance
                var (currentDistance, currentId) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                // Skip if already visited
                if (visited.Contains(currentId))
                    continue;

                // Visit node
                visited.Add(currentId);
                visitedOrder.Add(currentId);

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null)
                    continue;

                // Check neighbors
                var edges = graph.GetEdges(currentNode);
                foreach (var edge in edges)
                {
                    int neighborId = edge.ToNodeId;

                    if (visited.Contains(neighborId))
                        continue;

                    // Calculate new distance
                    double newDistance = distances[currentId] + edge.Weight;

                    // Update if a shorter path is found
                    if (newDistance < distances[neighborId])
                    {
                        // Remove old entry (if exists)
                        priorityQueue.Remove((distances[neighborId], neighborId));

                        // Update
                        distances[neighborId] = newDistance;
                        previousNodes[neighborId] = currentId;

                        // Add new entry
                        priorityQueue.Add((newDistance, neighborId));
                    }
                }
            }

            return visitedOrder;
        }

        
        /// Returns the shortest path to a specific target node
        /// Can be used after Execute() is called
        
        /// <param name="targetNodeId">Target node ID</param>
        /// <returns>Path from start to target (List of node IDs)</returns>
        public List<int> GetShortestPath(int targetNodeId)
        {
            if (!previousNodes.ContainsKey(targetNodeId))
                throw new InvalidOperationException("Execute() must be called before GetShortestPath().");

            var path = new List<int>();
            int? currentId = targetNodeId;

            // If target node is unreachable
            if (distances[targetNodeId] == double.PositiveInfinity)
                return path; // Empty list

            // Backtrack the path
            while (currentId.HasValue)
            {
                path.Add(currentId.Value);
                currentId = previousNodes[currentId.Value];
            }

            // Reverse the path (start to target)
            path.Reverse();
            return path;
        }

        
        /// Returns shortest distance to a specific node
        /// Can be used after Execute() is called
        
        public double GetDistance(int nodeId)
        {
            if (!distances.ContainsKey(nodeId))
                throw new InvalidOperationException("Execute() must be called before GetDistance().");

            return distances[nodeId];
        }

        
        /// Returns all distances (for debugging/results)
        
        public Dictionary<int, double> GetAllDistances()
        {
            return new Dictionary<int, double>(distances);
        }
    }
}
