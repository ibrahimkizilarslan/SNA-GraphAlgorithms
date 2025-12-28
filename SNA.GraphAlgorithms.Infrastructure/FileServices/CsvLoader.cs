using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SNA.GraphAlgorithms.Infrastructure.FileServices
{
    
    /// CSV dosyasından veri okuyarak Node listesi oluşturur
    /// SRP: Sadece CSV okuma ve Node oluşturma sorumluluğu
    
    public class CsvLoader
    {
        
        /// CSV dosyasından node'ları okur
        /// Expected CSV format: Id,Name,Activity,InteractionCount,ConnectionCount
        
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>Oluşturulan node listesi</returns>
        public List<Node> LoadNodes(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV file not found: {filePath}");

            var nodes = new List<Node>();

            using (var reader = new StreamReader(filePath))
            {
                // İlk satırı oku (header)
                string? headerLine = reader.ReadLine();
                if (headerLine == null)
                    throw new InvalidDataException("CSV file is empty.");

                // Veri satırlarını oku
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

        
        /// CSV satırından Node oluşturur
        
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

        
        /// CSV'den node'ları okur ve Graph oluşturur
        /// Graph, edge'leri otomatik olarak WeightCalculator ile hesaplar
        
        /// <param name="filePath">CSV dosya yolu</param>
        /// <param name="createFullyConnected">Tüm node'ları birbirine bağla (tam bağlı graph)</param>
        /// <returns>Oluşturulan Graph</returns>
        public Graph LoadGraph(string filePath, bool createFullyConnected = false)
        {
            var nodes = LoadNodes(filePath);
            var graph = new Graph();

            // Node'ları graph'a ekle
            foreach (var node in nodes)
            {
                graph.AddNode(node);
            }

            // İsteğe bağlı: Tam bağlı graph oluştur
            if (createFullyConnected)
            {
                CreateFullyConnectedGraph(graph, nodes);
            }

            return graph;
        }

        
        /// Tüm node'ları birbirine bağlar (tam bağlı graph)
        /// Weight'ler otomatik olarak Graph tarafından hesaplanır
        
        private void CreateFullyConnectedGraph(Graph graph, List<Node> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    // Graph.AddEdge otomatik olarak WeightCalculator kullanır
                    graph.AddEdge(nodes[i].Id, nodes[j].Id, isDirected: false);
                }
            }
        }
    }
}

