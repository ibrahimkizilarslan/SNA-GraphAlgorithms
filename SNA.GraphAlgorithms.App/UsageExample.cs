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

            // 4. Dijkstra Algoritması
            DemoDijkstra(graph);

            // 5. A* Algoritması (basit)
            DemoAStar(graph);

            // 6. A* Algoritması (pozisyon-based)
            DemoAStarWithPositions();

            // 7. CSV'den Graph yükleme örneği
            DemoLoadFromCsv();

            // 8. Welsh-Powell Renklendirme
            DemoWelshPowell(graph);

            // 9. Bağlı Bileşenler
            DemoConnectedComponents(graph);

            // 10. Degree Centrality
            DemoDegreeCentrality(graph);

            // 11. Graf Export
            DemoGraphExport(graph);
        }

        /// <summary>
        /// Welsh-Powell graf renklendirme örneği
        /// </summary>
        private static void DemoWelshPowell(Graph graph)
        {
            Console.WriteLine("\n\n=== Welsh-Powell Graf Renklendirme ===");

            var welshPowell = new WelshPowell();
            welshPowell.Execute(graph, 0);

            Console.WriteLine($"Kromatik Sayı (Kullanılan Renk Sayısı): {welshPowell.GetChromaticNumber()}");

            var groups = welshPowell.GetColorGroups();
            foreach (var group in groups.OrderBy(g => g.Key))
            {
                var nodeNames = group.Value.Select(id => graph.GetNode(id)?.Name ?? id.ToString());
                Console.WriteLine($"Renk {group.Key}: {string.Join(", ", nodeNames)}");
            }
        }

        /// <summary>
        /// Bağlı bileşenler örneği
        /// </summary>
        private static void DemoConnectedComponents(Graph graph)
        {
            Console.WriteLine("\n\n=== Bağlı Bileşenler (Connected Components) ===");

            var cc = new ConnectedComponents();
            cc.Execute(graph, 0);

            Console.WriteLine($"Toplam Bileşen Sayısı: {cc.GetComponentCount()}");
            Console.WriteLine($"Graf Bağlı mı: {(cc.IsGraphConnected() ? "Evet" : "Hayır")}");

            var components = cc.GetAllComponents();
            for (int i = 0; i < components.Count; i++)
            {
                var nodeNames = components[i].Select(id => graph.GetNode(id)?.Name ?? id.ToString());
                Console.WriteLine($"Bileşen {i + 1}: {string.Join(", ", nodeNames)}");
            }
        }

        /// <summary>
        /// Degree Centrality örneği
        /// </summary>
        private static void DemoDegreeCentrality(Graph graph)
        {
            Console.WriteLine("\n\n=== Degree Centrality (En Etkili Düğümler) ===");

            var dc = new DegreeCentrality();
            dc.Execute(graph, 0);

            Console.WriteLine($"Graf Yoğunluğu: {dc.GetGraphDensity(graph):F4}");
            Console.WriteLine($"Ortalama Merkezilik: {dc.GetAverageCentrality():F4}");

            Console.WriteLine("\nEn Etkili 5 Düğüm:");
            var topNodes = dc.GetTopNodes(5);
            int rank = 1;
            foreach (var (nodeId, centrality, degree) in topNodes)
            {
                var node = graph.GetNode(nodeId);
                Console.WriteLine($"  #{rank}: {node?.Name} - Degree: {degree}, Centrality: {centrality:F4}");
                rank++;
            }
        }

        /// <summary>
        /// Graf export örneği
        /// </summary>
        private static void DemoGraphExport(Graph graph)
        {
            Console.WriteLine("\n\n=== Graf Dışa Aktarım ===");

            var exporter = new GraphExporter();

            string exportDir = "exports";
            if (!System.IO.Directory.Exists(exportDir))
                System.IO.Directory.CreateDirectory(exportDir);

            try
            {
                exporter.ExportToJson(graph, System.IO.Path.Combine(exportDir, "graph.json"));
                Console.WriteLine("✓ JSON dışa aktarıldı: exports/graph.json");

                exporter.ExportNodesToCsv(graph, System.IO.Path.Combine(exportDir, "nodes.csv"));
                Console.WriteLine("✓ Nodes CSV dışa aktarıldı: exports/nodes.csv");

                exporter.ExportEdgesToCsv(graph, System.IO.Path.Combine(exportDir, "edges.csv"));
                Console.WriteLine("✓ Edges CSV dışa aktarıldı: exports/edges.csv");

                exporter.ExportAdjacencyList(graph, System.IO.Path.Combine(exportDir, "adjacency_list.txt"));
                Console.WriteLine("✓ Komşuluk listesi dışa aktarıldı: exports/adjacency_list.txt");

                exporter.ExportAdjacencyMatrix(graph, System.IO.Path.Combine(exportDir, "adjacency_matrix.txt"));
                Console.WriteLine("✓ Komşuluk matrisi dışa aktarıldı: exports/adjacency_matrix.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dışa aktarım hatası: {ex.Message}");
            }
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
        /// Dijkstra algoritması demo
        /// </summary>
        private static void DemoDijkstra(Graph graph)
        {
            Console.WriteLine("\n\n=== Dijkstra's Shortest Path ===");
            
            var dijkstra = new Dijkstra();
            int startNodeId = 1;
            
            // Algoritmayı çalıştır
            var visitedOrder = dijkstra.Execute(graph, startNodeId);
            
            Console.WriteLine($"\nBaşlangıç Node: {graph.GetNode(startNodeId)?.Name} (ID:{startNodeId})");
            Console.WriteLine($"Ziyaret Sırası: {string.Join(" -> ", visitedOrder)}");
            
            // Tüm node'lara olan mesafeleri göster
            Console.WriteLine("\nTüm Node'lara Olan En Kısa Mesafeler:");
            foreach (var node in graph.Nodes)
            {
                double distance = dijkstra.GetDistance(node.Id);
                string distStr = distance == double.PositiveInfinity ? "Ulaşılamaz" : distance.ToString("F4");
                Console.WriteLine($"  {graph.GetNode(startNodeId)?.Name} -> {node.Name}: {distStr}");
            }
            
            // Belirli bir node'a yol
            int targetId = 4;
            var path = dijkstra.GetShortestPath(targetId);
            Console.WriteLine($"\n{graph.GetNode(startNodeId)?.Name} -> {graph.GetNode(targetId)?.Name} En Kısa Yol:");
            Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
            Console.WriteLine($"  Toplam Maliyet: {dijkstra.GetDistance(targetId):F4}");
        }

        /// <summary>
        /// A* algoritması demo (basit)
        /// </summary>
        private static void DemoAStar(Graph graph)
        {
            Console.WriteLine("\n\n=== A* Pathfinding (Basic) ===");
            
            var aStar = new AStar();
            int startNodeId = 1;
            int targetNodeId = 4;
            
            // Belirli bir hedefe yol bul
            var path = aStar.FindPath(graph, startNodeId, targetNodeId);
            
            Console.WriteLine($"\nBaşlangıç: {graph.GetNode(startNodeId)?.Name} (ID:{startNodeId})");
            Console.WriteLine($"Hedef: {graph.GetNode(targetNodeId)?.Name} (ID:{targetNodeId})");
            
            if (path.Count > 0)
            {
                Console.WriteLine($"\nBulunan Yol:");
                Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
                Console.WriteLine($"  Toplam Maliyet: {aStar.GetCost(targetNodeId):F4}");
            }
            else
            {
                Console.WriteLine("\nYol bulunamadı!");
            }
        }

        /// <summary>
        /// A* algoritması demo (pozisyon-based heuristic ile)
        /// </summary>
        private static void DemoAStarWithPositions()
        {
            Console.WriteLine("\n\n=== A* Pathfinding (Position-Based) ===");
            
            // Pozisyon bilgisi olan graph oluştur
            var graph = new Graph();
            
            // Node'ları grid üzerinde konumlandır
            var nodeA = new Node { Id = 1, Name = "A", X = 0, Y = 0, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            var nodeB = new Node { Id = 2, Name = "B", X = 1, Y = 0, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            var nodeC = new Node { Id = 3, Name = "C", X = 2, Y = 0, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            var nodeD = new Node { Id = 4, Name = "D", X = 1, Y = 1, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            var nodeE = new Node { Id = 5, Name = "E", X = 2, Y = 1, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            var nodeF = new Node { Id = 6, Name = "F", X = 2, Y = 2, Activity = 5, InteractionCount = 100, ConnectionCount = 10 };
            
            graph.AddNode(nodeA);
            graph.AddNode(nodeB);
            graph.AddNode(nodeC);
            graph.AddNode(nodeD);
            graph.AddNode(nodeE);
            graph.AddNode(nodeF);
            
            // Bağlantılar (manuel weight ile - mesafeye göre)
            graph.AddEdge(1, 2, 1.0); // A-B
            graph.AddEdge(2, 3, 1.0); // B-C
            graph.AddEdge(2, 4, 1.0); // B-D
            graph.AddEdge(3, 5, 1.0); // C-E
            graph.AddEdge(4, 5, 1.0); // D-E
            graph.AddEdge(5, 6, 1.0); // E-F
            
            Console.WriteLine("\n2D Grid Graph:");
            Console.WriteLine("  F(2,2)");
            Console.WriteLine("    |");
            Console.WriteLine("  D(1,1)-E(2,1)");
            Console.WriteLine("    |     |");
            Console.WriteLine("  A(0,0)-B(1,0)-C(2,0)");
            
            // A'dan F'ye en kısa yol
            var aStar = new AStar();
            var path = aStar.FindPath(graph, startNodeId: 1, targetNodeId: 6);
            
            Console.WriteLine($"\n{nodeA.Name} -> {nodeF.Name} En Kısa Yol:");
            Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
            Console.WriteLine($"  Toplam Maliyet: {aStar.GetCost(6):F2}");
            
            // Euclidean distance heuristic kullanıldı
            Console.WriteLine($"\nEuclidean Distance (heuristic): {nodeA.DistanceTo(nodeF):F2}");
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

        /// <summary>
        /// Tüm algoritmaları karşılaştırma
        /// </summary>
        public static void CompareAlgorithms()
        {
            Console.WriteLine("\n\n=== Algoritma Karşılaştırması ===\n");
            
            var graph = CreateSampleGraph();
            int startId = 1;
            
            Console.WriteLine($"Başlangıç Node: {graph.GetNode(startId)?.Name}\n");
            
            // BFS
            var bfs = new BFS();
            var bfsResult = bfs.Execute(graph, startId);
            Console.WriteLine($"BFS Ziyaret Sırası: {string.Join(" -> ", bfsResult)}");
            
            // DFS
            var dfs = new DFS();
            var dfsResult = dfs.Execute(graph, startId);
            Console.WriteLine($"DFS Ziyaret Sırası: {string.Join(" -> ", dfsResult)}");
            
            // Dijkstra
            var dijkstra = new Dijkstra();
            var dijkstraResult = dijkstra.Execute(graph, startId);
            Console.WriteLine($"Dijkstra Ziyaret Sırası: {string.Join(" -> ", dijkstraResult)}");
            
            // A*
            var aStar = new AStar();
            var aStarResult = aStar.Execute(graph, startId);
            Console.WriteLine($"A* Ziyaret Sırası: {string.Join(" -> ", aStarResult)}");
        }
    }
}
