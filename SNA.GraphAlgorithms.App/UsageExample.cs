using SNA.GraphAlgorithms.Core.Algorithms;
using SNA.GraphAlgorithms.Core.Models;
using SNA.GraphAlgorithms.Core.Services;
using SNA.GraphAlgorithms.Infrastructure.FileServices;
using System;

namespace SNA.GraphAlgorithms.App
{
    
    /// Usage example for the refactored architecture
    
    public static class UsageExample
    {
        public static void DemoGraphAlgorithms()
        {
            Console.WriteLine("=== Graph Algorithm Demo ===\n");

            // 1. Manuel Graph oluşturma
            var graph = CreateSampleGraph();

            // 2. BFS Algorithm
            IGraphAlgorithm bfs = new BFS();
            Console.WriteLine($"\n--- {bfs.Name} ---");
            var bfsResult = bfs.Execute(graph, 1);
            Console.WriteLine($"Visit Order: {string.Join(" -> ", bfsResult)}");

            // 3. DFS Algorithm
            IGraphAlgorithm dfs = new DFS();
            Console.WriteLine($"\n--- {dfs.Name} ---");
            var dfsResult = dfs.Execute(graph, 1);
            Console.WriteLine($"Visit Order: {string.Join(" -> ", dfsResult)}");

            // 4. Dijkstra Algorithm
            DemoDijkstra(graph);

            // 5. A* Algorithm (Basic)
            DemoAStar(graph);

            // 6. A* Algorithm (Position-based)
            DemoAStarWithPositions();

            // 7. Load from CSV example
            DemoLoadFromCsv();

            // 8. Welsh-Powell Coloring
            DemoWelshPowell(graph);

            // 9. Connected Components
            DemoConnectedComponents(graph);

            // 10. Degree Centrality
            DemoDegreeCentrality(graph);

            // 11. Graf Export
            DemoGraphExport(graph);
        }

        
        /// Welsh-Powell graph coloring example
        
        private static void DemoWelshPowell(Graph graph)
        {
            Console.WriteLine("\n\n=== Welsh-Powell Graph Coloring ===");

            var welshPowell = new WelshPowell();
            welshPowell.Execute(graph, 0);

            Console.WriteLine($"Chromatic Number (Colors Used): {welshPowell.GetChromaticNumber()}");

            var groups = welshPowell.GetColorGroups();
            foreach (var group in groups.OrderBy(g => g.Key))
            {
                var nodeNames = group.Value.Select(id => graph.GetNode(id)?.Name ?? id.ToString());
                Console.WriteLine($"Color {group.Key}: {string.Join(", ", nodeNames)}");
            }
        }

        
        /// Connected components example
        
        private static void DemoConnectedComponents(Graph graph)
        {
            Console.WriteLine("\n\n=== Connected Components Analysis ===");

            var cc = new ConnectedComponents();
            cc.Execute(graph, 0);

            Console.WriteLine($"Total Components: {cc.GetComponentCount()}");
            Console.WriteLine($"Is Graph Connected: {(cc.IsGraphConnected() ? "Yes" : "No")}");

            var components = cc.GetAllComponents();
            for (int i = 0; i < components.Count; i++)
            {
                var nodeNames = components[i].Select(id => graph.GetNode(id)?.Name ?? id.ToString());
                Console.WriteLine($"Component {i + 1}: {string.Join(", ", nodeNames)}");
            }
        }

        
        /// Degree Centrality example
        
        private static void DemoDegreeCentrality(Graph graph)
        {
            Console.WriteLine("\n\n=== Degree Centrality (Node Influence) ===");

            var dc = new DegreeCentrality();
            dc.Execute(graph, 0);

            Console.WriteLine($"Graph Density: {dc.GetGraphDensity(graph):F4}");
            Console.WriteLine($"Average Centrality: {dc.GetAverageCentrality():F4}");

            Console.WriteLine("\nTop 5 Influential Nodes:");
            var topNodes = dc.GetTopNodes(5);
            int rank = 1;
            foreach (var (nodeId, centrality, degree) in topNodes)
            {
                var node = graph.GetNode(nodeId);
                Console.WriteLine($"  #{rank}: {node?.Name} - Degree: {degree}, Centrality: {centrality:F4}");
                rank++;
            }
        }

        
        /// Graph export example
        
        private static void DemoGraphExport(Graph graph)
        {
            Console.WriteLine("\n\n=== Graph Exporting ===");

            var exporter = new GraphExporter();

            string exportDir = "exports";
            if (!System.IO.Directory.Exists(exportDir))
                System.IO.Directory.CreateDirectory(exportDir);

            try
            {
                exporter.ExportToJson(graph, System.IO.Path.Combine(exportDir, "graph.json"));
                Console.WriteLine("✓ JSON exported: exports/graph.json");

                exporter.ExportNodesToCsv(graph, System.IO.Path.Combine(exportDir, "nodes.csv"));
                Console.WriteLine("✓ Nodes CSV exported: exports/nodes.csv");

                exporter.ExportEdgesToCsv(graph, System.IO.Path.Combine(exportDir, "edges.csv"));
                Console.WriteLine("✓ Edges CSV exported: exports/edges.csv");

                exporter.ExportAdjacencyList(graph, System.IO.Path.Combine(exportDir, "adjacency_list.txt"));
                Console.WriteLine("✓ Adjacency List exported: exports/adjacency_list.txt");

                exporter.ExportAdjacencyMatrix(graph, System.IO.Path.Combine(exportDir, "adjacency_matrix.txt"));
                Console.WriteLine("✓ Adjacency Matrix exported: exports/adjacency_matrix.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
            }
        }
        

        
        /// Manual graph construction example
        
        private static Graph CreateSampleGraph()
        {
            var graph = new Graph();

            // Create nodes
            var node1 = new Node { Id = 1, Name = "User A", Activity = 8.5, InteractionCount = 120, ConnectionCount = 15 };
            var node2 = new Node { Id = 2, Name = "User B", Activity = 7.2, InteractionCount = 95, ConnectionCount = 12 };
            var node3 = new Node { Id = 3, Name = "User C", Activity = 9.0, InteractionCount = 150, ConnectionCount = 18 };
            var node4 = new Node { Id = 4, Name = "User D", Activity = 6.8, InteractionCount = 80, ConnectionCount = 10 };

            // Add to graph
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);
            graph.AddNode(node4);

            // Add edges (WeightCalculator calculates automatically)
            graph.AddEdge(1, 2); // User A <-> User B
            graph.AddEdge(1, 3); // User A <-> User C
            graph.AddEdge(2, 4); // User B <-> User D
            graph.AddEdge(3, 4); // User C <-> User D

            Console.WriteLine("\n=== Generated Graph ===");
            Console.WriteLine($"Node Count: {graph.Nodes.Count}");
            Console.WriteLine($"Edge Count: {graph.Edges.Count / 2}"); // Undirected so divide by 2
            
            Console.WriteLine("\nNode Details:");
            foreach (var node in graph.Nodes)
            {
                Console.WriteLine($"  {node.Name} (ID:{node.Id}) - Activity:{node.Activity}, Interactions:{node.InteractionCount}, Connections:{node.ConnectionCount}");
            }

            Console.WriteLine("\nEdge Details (Weights calculated automatically):");
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

        
        /// Dijkstra algoritması demo
        
        private static void DemoDijkstra(Graph graph)
        {
            Console.WriteLine("\n\n=== Dijkstra's Shortest Path ===");
            
            var dijkstra = new Dijkstra();
            int startNodeId = 1;
            
            // Algoritmayı çalıştır
            var visitedOrder = dijkstra.Execute(graph, startNodeId);
            
            Console.WriteLine($"\nStart Node: {graph.GetNode(startNodeId)?.Name} (ID:{startNodeId})");
            Console.WriteLine($"Visit Order: {string.Join(" -> ", visitedOrder)}");
            
            // Show distances to all nodes
            Console.WriteLine("\nShortest Distances to All Nodes:");
            foreach (var node in graph.Nodes)
            {
                double distance = dijkstra.GetDistance(node.Id);
                string distStr = distance == double.PositiveInfinity ? "Unreachable" : distance.ToString("F4");
                Console.WriteLine($"  {graph.GetNode(startNodeId)?.Name} -> {node.Name}: {distStr}");
            }
            
            // Specific path to a node
            int targetId = 4;
            var path = dijkstra.GetShortestPath(targetId);
            Console.WriteLine($"\n{graph.GetNode(startNodeId)?.Name} -> {graph.GetNode(targetId)?.Name} Shortest Path:");
            Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
            Console.WriteLine($"  Total Cost: {dijkstra.GetDistance(targetId):F4}");
        }

        
        /// A* algoritması demo (basit)
        
        private static void DemoAStar(Graph graph)
        {
            Console.WriteLine("\n\n=== A* Pathfinding (Basic) ===");
            
            var aStar = new AStar();
            int startNodeId = 1;
            int targetNodeId = 4;
            
            // Find path to a target
            var path = aStar.FindPath(graph, startNodeId, targetNodeId);
            
            Console.WriteLine($"\nStart: {graph.GetNode(startNodeId)?.Name} (ID:{startNodeId})");
            Console.WriteLine($"Target: {graph.GetNode(targetNodeId)?.Name} (ID:{targetNodeId})");
            
            if (path.Count > 0)
            {
                Console.WriteLine($"\nPath Found:");
                Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
                Console.WriteLine($"  Total Cost: {aStar.GetCost(targetNodeId):F4}");
            }
            else
            {
                Console.WriteLine("\nPath not found!");
            }
        }

        
        /// A* algoritması demo (pozisyon-based heuristic ile)
        
        private static void DemoAStarWithPositions()
        {
            Console.WriteLine("\n\n=== A* Pathfinding (Position-Based) ===");
            
            // Create graph with position data
            var graph = new Graph();
            
            // Locate nodes on a grid
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
            
            // Connections (manual weights based on distance)
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
            
            // Shortest path from A to F
            var aStar = new AStar();
            var path = aStar.FindPath(graph, startNodeId: 1, targetNodeId: 6);
            
            Console.WriteLine($"\nShortest Path {nodeA.Name} -> {nodeF.Name}:");
            Console.WriteLine($"  {string.Join(" -> ", path.ConvertAll(id => graph.GetNode(id)?.Name ?? id.ToString()))}");
            Console.WriteLine($"  Total Cost: {aStar.GetCost(6):F2}");
            
            // Euclidean distance heuristic kullanıldı
            Console.WriteLine($"\nEuclidean Distance (heuristic): {nodeA.DistanceTo(nodeF):F2}");
        }

        
        /// CSV'den graph yükleme örneği
        
        private static void DemoLoadFromCsv()
        {
            Console.WriteLine("\n\n=== Loading Graph from CSV ===");
            
            // Note: A CSV file is required for this example
            // CSV format: Id,Name,Activity,InteractionCount,ConnectionCount
            // Example:
            // 1,UserA,8.5,120,15
            // 2,UserB,7.2,95,12
            
            string csvPath = "sample_data.csv";
            
            if (System.IO.File.Exists(csvPath))
            {
                var csvLoader = new CsvLoader();
                
                // Load nodes only
                var nodes = csvLoader.LoadNodes(csvPath);
                Console.WriteLine($"Loaded Node Count: {nodes.Count}");
                
                // Create graph (fully connected)
                var graph = csvLoader.LoadGraph(csvPath, createFullyConnected: true);
                Console.WriteLine($"Graph - Nodes: {graph.Nodes.Count}, Edges: {graph.Edges.Count / 2}");
            }
            else
            {
                Console.WriteLine($"CSV file not found: {csvPath}");
                Console.WriteLine("Sample CSV format:");
                Console.WriteLine("Id,Name,Activity,InteractionCount,ConnectionCount");
                Console.WriteLine("1,UserA,8.5,120,15");
                Console.WriteLine("2,UserB,7.2,95,12");
            }
        }

        
        /// Weight hesaplama örneği
        
        public static void DemoWeightCalculation()
        {
            Console.WriteLine("\n\n=== Weight Hesaplama Demo ===");
            
            var node1 = new Node { Id = 1, Name = "User A", Activity = 8.5, InteractionCount = 120, ConnectionCount = 15 };
            var node2 = new Node { Id = 2, Name = "User B", Activity = 7.2, InteractionCount = 95, ConnectionCount = 12 };
            
            double weight = WeightCalculator.Calculate(node1, node2);
            
            Console.WriteLine($"\nNode 1: {node1.Name}");
            Console.WriteLine($"  Activity: {node1.Activity}, Interactions: {node1.InteractionCount}, Connections: {node1.ConnectionCount}");
            
            Console.WriteLine($"\nNode 2: {node2.Name}");
            Console.WriteLine($"  Activity: {node2.Activity}, Interactions: {node2.InteractionCount}, Connections: {node2.ConnectionCount}");
            
            Console.WriteLine($"\nCalculated Weight: {weight:F6}");
            Console.WriteLine("\nFormula: 1 / (1 + (Activity_diff)² + (Interaction_diff)² + (Connection_diff)²)");
        }

        
        /// Compare all algorithms
        
        public static void CompareAlgorithms()
        {
            Console.WriteLine("\n\n=== Algorithm Comparison ===\n");
            
            var graph = CreateSampleGraph();
            int startId = 1;
            
            Console.WriteLine($"Start Node: {graph.GetNode(startId)?.Name}\n");
            
            // BFS
            var bfs = new BFS();
            var bfsResult = bfs.Execute(graph, startId);
            Console.WriteLine($"BFS Visit Order: {string.Join(" -> ", bfsResult)}");
            
            // DFS
            var dfs = new DFS();
            var dfsResult = dfs.Execute(graph, startId);
            Console.WriteLine($"DFS Visit Order: {string.Join(" -> ", dfsResult)}");
            
            // Dijkstra
            var dijkstra = new Dijkstra();
            var dijkstraResult = dijkstra.Execute(graph, startId);
            Console.WriteLine($"Dijkstra Visit Order: {string.Join(" -> ", dijkstraResult)}");
            
            // A*
            var aStar = new AStar();
            var aStarResult = aStar.Execute(graph, startId);
            Console.WriteLine($"A* Visit Order: {string.Join(" -> ", aStarResult)}");
        }
    }
}
