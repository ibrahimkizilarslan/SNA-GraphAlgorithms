using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace SNA.GraphAlgorithms.Infrastructure.FileServices
{
    
    /// Graf verilerini farklı formatlarda dışa aktarır
    /// JSON, CSV ve Adjacency Matrix formatlarını destekler
    
    public class GraphExporter
    {
        
        /// Grafı JSON formatında dışa aktarır
        
        /// <param name="graph">Dışa aktarılacak graf</param>
        /// <param name="filePath">Hedef dosya yolu</param>
        public void ExportToJson(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var graphData = new
            {
                Nodes = graph.Nodes.Select(n => new
                {
                    n.Id,
                    n.Name,
                    n.Activity,
                    n.InteractionCount,
                    n.ConnectionCount,
                    n.X,
                    n.Y,
                    n.Neighbors
                }).ToList(),
                Edges = graph.Edges
                    .Where(e => e.FromNodeId < e.ToNodeId) // Duplicate'leri önle (undirected)
                    .Select(e => new
                    {
                        e.FromNodeId,
                        e.ToNodeId,
                        e.Weight,
                        e.IsDirected
                    }).ToList(),
                Metadata = new
                {
                    NodeCount = graph.Nodes.Count,
                    EdgeCount = graph.Edges.Count / 2, // Undirected
                    ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(graphData, options);
            File.WriteAllText(filePath, jsonString);
        }

        
        /// Graf düğümlerini CSV formatında dışa aktarır
        
        public void ExportNodesToCsv(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Activity,InteractionCount,ConnectionCount,X,Y,Degree");

            foreach (var node in graph.Nodes)
            {
                sb.AppendLine($"{node.Id},{EscapeCsv(node.Name)},{node.Activity},{node.InteractionCount},{node.ConnectionCount},{node.X},{node.Y},{node.Neighbors.Count}");
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        
        /// Graf kenarlarını CSV formatında dışa aktarır
        
        public void ExportEdgesToCsv(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var sb = new StringBuilder();
            sb.AppendLine("FromNodeId,ToNodeId,Weight,FromNodeName,ToNodeName");

            // Duplicate edge'leri önle (sadece FromNodeId < ToNodeId olanları yaz)
            var writtenEdges = new HashSet<string>();
            foreach (var edge in graph.Edges)
            {
                string key = $"{Math.Min(edge.FromNodeId, edge.ToNodeId)}-{Math.Max(edge.FromNodeId, edge.ToNodeId)}";
                if (!writtenEdges.Contains(key))
                {
                    var fromNode = graph.GetNode(edge.FromNodeId);
                    var toNode = graph.GetNode(edge.ToNodeId);
                    sb.AppendLine($"{edge.FromNodeId},{edge.ToNodeId},{edge.Weight:F6},{EscapeCsv(fromNode?.Name ?? "")},{EscapeCsv(toNode?.Name ?? "")}");
                    writtenEdges.Add(key);
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        
        /// Komşuluk listesini (Adjacency List) text formatında dışa aktarır
        
        public void ExportAdjacencyList(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var sb = new StringBuilder();
            sb.AppendLine("=== ADJACENCY LIST ===");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Nodes: {graph.Nodes.Count}, Edges: {graph.Edges.Count / 2}");
            sb.AppendLine();

            foreach (var node in graph.Nodes.OrderBy(n => n.Id))
            {
                var edges = graph.GetEdges(node);
                var neighborInfo = edges.Select(e =>
                {
                    var neighbor = graph.GetNode(e.ToNodeId);
                    return $"{neighbor?.Name ?? e.ToNodeId.ToString()}(w:{e.Weight:F4})";
                });

                sb.AppendLine($"{node.Name} (ID:{node.Id}) -> [{string.Join(", ", neighborInfo)}]");
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        /
        /// Komşuluk matrisini (Adjacency Matrix) dışa aktarır
        
        public void ExportAdjacencyMatrix(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var matrix = GetAdjacencyMatrix(graph);
            var nodeIds = graph.Nodes.OrderBy(n => n.Id).Select(n => n.Id).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("=== ADJACENCY MATRIX ===");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Nodes: {graph.Nodes.Count}");
            sb.AppendLine();

            // Header (node IDs)
            sb.Append("     ");
            foreach (int id in nodeIds)
            {
                sb.Append($"{id,8}");
            }
            sb.AppendLine();

            // Matrix rows
            for (int i = 0; i < nodeIds.Count; i++)
            {
                sb.Append($"{nodeIds[i],4} ");
                for (int j = 0; j < nodeIds.Count; j++)
                {
                    double weight = matrix[i, j];
                    if (weight == 0)
                        sb.Append($"{"0",8}");
                    else
                        sb.Append($"{weight,8:F4}");
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        
        /// Komşuluk matrisini CSV formatında dışa aktarır
        
        public void ExportAdjacencyMatrixCsv(Graph graph, string filePath)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var matrix = GetAdjacencyMatrix(graph);
            var nodeIds = graph.Nodes.OrderBy(n => n.Id).Select(n => n.Id).ToList();

            var sb = new StringBuilder();

            // Header
            sb.Append(",");
            sb.AppendLine(string.Join(",", nodeIds));

            // Rows
            for (int i = 0; i < nodeIds.Count; i++)
            {
                sb.Append($"{nodeIds[i]},");
                var row = new List<string>();
                for (int j = 0; j < nodeIds.Count; j++)
                {
                    row.Add(matrix[i, j].ToString("F4"));
                }
                sb.AppendLine(string.Join(",", row));
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        
        /// Tüm verileri tek bir klasöre dışa aktarır
        
        public void ExportAll(Graph graph, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            ExportToJson(graph, Path.Combine(directoryPath, $"graph_{timestamp}.json"));
            ExportNodesToCsv(graph, Path.Combine(directoryPath, $"nodes_{timestamp}.csv"));
            ExportEdgesToCsv(graph, Path.Combine(directoryPath, $"edges_{timestamp}.csv"));
            ExportAdjacencyList(graph, Path.Combine(directoryPath, $"adjacency_list_{timestamp}.txt"));
            ExportAdjacencyMatrix(graph, Path.Combine(directoryPath, $"adjacency_matrix_{timestamp}.txt"));
            ExportAdjacencyMatrixCsv(graph, Path.Combine(directoryPath, $"adjacency_matrix_{timestamp}.csv"));
        }

        
        /// Grafın komşuluk matrisini hesaplar
        
        public double[,] GetAdjacencyMatrix(Graph graph)
        {
            var nodeIds = graph.Nodes.OrderBy(n => n.Id).Select(n => n.Id).ToList();
            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < nodeIds.Count; i++)
            {
                idToIndex[nodeIds[i]] = i;
            }

            int n = nodeIds.Count;
            var matrix = new double[n, n];

            foreach (var edge in graph.Edges)
            {
                if (idToIndex.ContainsKey(edge.FromNodeId) && idToIndex.ContainsKey(edge.ToNodeId))
                {
                    int i = idToIndex[edge.FromNodeId];
                    int j = idToIndex[edge.ToNodeId];
                    matrix[i, j] = edge.Weight;
                }
            }

            return matrix;
        }

        
        /// CSV için string escape işlemi
        
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
