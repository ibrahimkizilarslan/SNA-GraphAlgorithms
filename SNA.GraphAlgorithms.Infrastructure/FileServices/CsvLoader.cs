using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SNA.GraphAlgorithms.Infrastructure.FileServices
{
    
    /// Loads data from CSV file and creates Node list
    /// SRP: Responsibility for CSV reading and Node creation
    
    public class CsvLoader
    {
        
        /// Reads nodes from CSV file
        /// Expected CSV format: Id,Name,Activity,InteractionCount,ConnectionCount
        
        /// <param name="filePath">CSV file path</param>
        /// <returns>Created node list</returns>
        public List<Node> LoadNodes(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV file not found: {filePath}");

            var nodes = new List<Node>();

            using (var reader = new StreamReader(filePath))
            {
                // Read first line (header)
                string? headerLine = reader.ReadLine();
                if (headerLine == null)
                    throw new InvalidDataException("CSV file is empty.");

                // Read data lines
                int lineNumber = 1;
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var node = ParseNodeFromCsvLine(line);
                        nodes.Add(node);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException($"Error parsing line {lineNumber}: {line}", ex);
                    }
                }
            }

            return nodes;
        }

        
        /// Creates Node from CSV line
        
        private Node ParseNodeFromCsvLine(string line)
        {
            string[] columns = line.Split(',');

            if (columns.Length < 5)
                throw new FormatException($"Invalid CSV format. Expected at least 5 columns, got {columns.Length}");

            var node = new Node
            {
                Id = int.Parse(columns[0].Trim(), CultureInfo.InvariantCulture),
                Name = columns[1].Trim(),
                Activity = double.Parse(columns[2].Trim(), CultureInfo.InvariantCulture),
                InteractionCount = int.Parse(columns[3].Trim(), CultureInfo.InvariantCulture),
                ConnectionCount = int.Parse(columns[4].Trim(), CultureInfo.InvariantCulture)
            };

            return node;
        }

        
        /// Reads nodes from CSV and creates Graph
        /// Graph automatically calculates edges using WeightCalculator
        
        /// <param name="filePath">CSV file path</param>
        /// <param name="createFullyConnected">Connect all nodes to each other (fully connected graph)</param>
        /// <returns>Created Graph</returns>
        public Graph LoadGraph(string filePath, bool createFullyConnected = false)
        {
            var nodes = LoadNodes(filePath);
            var graph = new Graph();

            // Add nodes to graph
            foreach (var node in nodes)
            {
                graph.AddNode(node);
            }

            // Optional: Create fully connected graph
            if (createFullyConnected)
            {
                CreateFullyConnectedGraph(graph, nodes);
            }

            return graph;
        }

        
        /// Connects all nodes to each other (fully connected graph)
        /// Weights are automatically calculated by the Graph
        
        private void CreateFullyConnectedGraph(Graph graph, List<Node> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    // Graph.AddEdge automatically uses WeightCalculator
                    graph.AddEdge(nodes[i].Id, nodes[j].Id, isDirected: false);
                }
            }
        }
    }
}

