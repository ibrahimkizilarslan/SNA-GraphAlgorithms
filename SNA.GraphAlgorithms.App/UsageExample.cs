using SNA.GraphAlgorithms.Core.Algorithms;
using SNA.GraphAlgorithms.Core.Models;
using SNA.GraphAlgorithms.Core.Services;
using SNA.GraphAlgorithms.Infrastructure.FileServices;
using System;

namespace SNA.GraphAlgorithms.App
{
    /// <summary>
    /// Refactor edilmiş yapının kullanım örneği
    /// </summary>
    public static class UsageExample
    {
        public static void DemoGraphAlgorithms()
        {
            Console.WriteLine("=== Graph Algorithm Demo ===\n");

            // 1. Manuel Graph oluşturma
            var graph = CreateSampleGraph();

            // 2. BFS Algoritması
            IGraphAlgorithm bfs = new BFS();
            Console.WriteLine($"\n--- {bfs.Name} ---");
            var bfsResult = bfs.Execute(graph, 1);
            Console.WriteLine($"Ziyaret Sırası: {string.Join(" -> ", bfsResult)}");

            // 3. DFS Algoritması
            IGraphAlgorithm dfs = new DFS();
            Console.WriteLine($"\n--- {dfs.Name} ---");
            var dfsResult = dfs.Execute(graph, 1);
            Console.WriteLine($"Ziyaret Sırası: {string.Join(" -> ", dfsResult)}");

            // 4. CSV'den Graph yükleme örneği
            DemoLoadFromCsv();
        }

        /// <summary>
        /// Manuel graph oluşturma örneği
        /// </summary>
        private static Graph CreateSampleGraph()
        {
            var graph = new Graph();

            // Node'ları oluştur
            var node1 = new Node { Id = 1, Name = "Ali", Activity = 8.5, InteractionCount = 120, ConnectionCount = 15 };
            var node2 = new Node { Id = 2, Name = "Ayşe", Activity = 7.2, InteractionCount = 95, ConnectionCount = 12 };
            var node3 = new Node { Id = 3, Name = "Mehmet", Activity = 9.0, InteractionCount = 150, ConnectionCount = 18 };
            var node4 = new Node { Id = 4, Name = "Fatma", Activity = 6.8, InteractionCount = 80, ConnectionCount = 10 };

            // Graph'a ekle
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);
            graph.AddNode(node4);

            // Edge'leri ekle (WeightCalculator otomatik hesaplar)
            graph.AddEdge(1, 2); // Ali <-> Ayşe
            graph.AddEdge(1, 3); // Ali <-> Mehmet
            graph.AddEdge(2, 4); // Ayşe <-> Fatma
            graph.AddEdge(3, 4); // Mehmet <-> Fatma

            Console.WriteLine("\n=== Oluşturulan Graph ===");
            Console.WriteLine($"Node Sayısı: {graph.Nodes.Count}");
            Console.WriteLine($"Edge Sayısı: {graph.Edges.Count / 2}"); // Undirected, bu yüzden 2'ye böl
            
            Console.WriteLine("\nNode Detayları:");
            foreach (var node in graph.Nodes)
            {
                Console.WriteLine($"  {node.Name} (ID:{node.Id}) - Activity:{node.Activity}, Interactions:{node.InteractionCount}, Connections:{node.ConnectionCount}");
            }

            Console.WriteLine("\nEdge Detayları (Weight'ler otomatik hesaplandı):");
            var printedEdges = new HashSet<string>();
            foreach (var edge in graph.Edges)
            {
                string key = $"{Math.Min(edge.FromNodeId, edge.ToNodeId)}-{Math.Max(edge.FromNodeId, edge.ToNodeId)}";
                if (!printedEdges.Contains(key))
                {
                    var fromNode = graph.GetNode(edge.FromNodeId);
                    var toNode = graph.GetNode(edge.ToNodeId);
                    Console.WriteLine($"  {fromNode?.Name} <-> {toNode?.Name}: Weight = {edge.Weight:F4}");
                    printedEdges.Add(key);
                }
            }

            return graph;
        }

        /// <summary>
        /// CSV'den graph yükleme örneği
        /// </summary>
        private static void DemoLoadFromCsv()
        {
            Console.WriteLine("\n\n=== CSV'den Graph Yükleme ===");
            
            // Not: Bu örnek için bir CSV dosyası gerekli
            // CSV formatı: Id,Name,Activity,InteractionCount,ConnectionCount
            // Örnek:
            // 1,Ali,8.5,120,15
            // 2,Ayşe,7.2,95,12
            
            string csvPath = "sample_data.csv";
            
            if (System.IO.File.Exists(csvPath))
            {
                var csvLoader = new CsvLoader();
                
                // Sadece node'ları yükle
                var nodes = csvLoader.LoadNodes(csvPath);
                Console.WriteLine($"Yüklenen Node Sayısı: {nodes.Count}");
                
                // Graph oluştur (tam bağlı)
                var graph = csvLoader.LoadGraph(csvPath, createFullyConnected: true);
                Console.WriteLine($"Graph - Node: {graph.Nodes.Count}, Edge: {graph.Edges.Count / 2}");
            }
            else
            {
                Console.WriteLine($"CSV dosyası bulunamadı: {csvPath}");
                Console.WriteLine("Örnek CSV formatı:");
                Console.WriteLine("Id,Name,Activity,InteractionCount,ConnectionCount");
                Console.WriteLine("1,Ali,8.5,120,15");
                Console.WriteLine("2,Ayşe,7.2,95,12");
            }
        }

        /// <summary>
        /// Weight hesaplama örneği
        /// </summary>
        public static void DemoWeightCalculation()
        {
            Console.WriteLine("\n\n=== Weight Hesaplama Demo ===");
            
            var node1 = new Node { Id = 1, Name = "Ali", Activity = 8.5, InteractionCount = 120, ConnectionCount = 15 };
            var node2 = new Node { Id = 2, Name = "Ayşe", Activity = 7.2, InteractionCount = 95, ConnectionCount = 12 };
            
            double weight = WeightCalculator.Calculate(node1, node2);
            
            Console.WriteLine($"\nNode 1: {node1.Name}");
            Console.WriteLine($"  Activity: {node1.Activity}, Interactions: {node1.InteractionCount}, Connections: {node1.ConnectionCount}");
            
            Console.WriteLine($"\nNode 2: {node2.Name}");
            Console.WriteLine($"  Activity: {node2.Activity}, Interactions: {node2.InteractionCount}, Connections: {node2.ConnectionCount}");
            
            Console.WriteLine($"\nHesaplanan Weight: {weight:F6}");
            Console.WriteLine("\nFormül: 1 / (1 + (Activity_diff)² + (Interaction_diff)² + (Connection_diff)²)");
        }
    }
}
