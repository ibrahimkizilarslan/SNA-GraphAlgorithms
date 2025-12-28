using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    
    /// A* (A-Star) Pathfinding algoritması
    /// Heuristic kullanarak hedef node'a en optimal yolu bulur
    
    public class AStar : IGraphAlgorithm
    {
        public string Name => "A* Pathfinding";

        private Dictionary<int, double> gScores = new Dictionary<int, double>(); // Start'tan node'a gerçek maliyet
        private Dictionary<int, double> fScores = new Dictionary<int, double>(); // g + heuristic
        private Dictionary<int, int?> previousNodes = new Dictionary<int, int?>();
        private int? targetNodeId = null;

        
        /// A* algoritmasını çalıştırır
        /// Not: Hedef node belirtmeden çalıştırılırsa Dijkstra gibi çalışır
        
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <returns>Ziyaret edilen düğümlerin ID listesi</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (!graph.NodeById.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with Id {startNodeId} not found in graph.", nameof(startNodeId));

            // Hedef belirtilmemişse tüm graph'ı tara
            return ExecuteInternal(graph, startNodeId, targetNodeId: null);
        }

        
        /// A* algoritmasını belirli bir hedefe doğru çalıştırır
        /
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <param name="targetNodeId">Hedef düğümü ID</param>
        /// <returns>Başlangıçtan hedefe olan en kısa yol (node ID listesi)</returns>
        public List<int> FindPath(Graph graph, int startNodeId, int targetNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (!graph.NodeById.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with Id {startNodeId} not found in graph.", nameof(startNodeId));

            if (!graph.NodeById.ContainsKey(targetNodeId))
                throw new ArgumentException($"Target node with Id {targetNodeId} not found in graph.", nameof(targetNodeId));

            this.targetNodeId = targetNodeId;
            var visitedOrder = ExecuteInternal(graph, startNodeId, targetNodeId);

            // Yolu reconstruct et
            return ReconstructPath(targetNodeId);
        }

        
        /// A* algoritmasının asıl implementasyonu
        
        private List<int> ExecuteInternal(Graph graph, int startNodeId, int? targetNodeId)
        {
            // Initialize
            gScores = new Dictionary<int, double>();
            fScores = new Dictionary<int, double>();
            previousNodes = new Dictionary<int, int?>();
            var visited = new HashSet<int>();
            var visitedOrder = new List<int>();

            // Tüm node'lara sonsuz maliyet ata
            foreach (var node in graph.Nodes)
            {
                gScores[node.Id] = double.PositiveInfinity;
                fScores[node.Id] = double.PositiveInfinity;
                previousNodes[node.Id] = null;
            }

            // Başlangıç node'unun maliyeti 0
            gScores[startNodeId] = 0;
            fScores[startNodeId] = targetNodeId.HasValue 
                ? CalculateHeuristic(graph, startNodeId, targetNodeId.Value)
                : 0;

            // Priority queue (f-score'a göre sıralı)
            var openSet = new SortedSet<(double fScore, int nodeId)>(
                Comparer<(double, int)>.Create((a, b) =>
                {
                    int result = a.Item1.CompareTo(b.Item1);
                    return result != 0 ? result : a.Item2.CompareTo(b.Item2);
                })
            );

            openSet.Add((fScores[startNodeId], startNodeId));

            while (openSet.Count > 0)
            {
                // En düşük f-score'lu node'u al
                var (currentFScore, currentId) = openSet.Min;
                openSet.Remove(openSet.Min);

                // Hedefe ulaştıysak dur
                if (targetNodeId.HasValue && currentId == targetNodeId.Value)
                {
                    visitedOrder.Add(currentId);
                    break;
                }

                // Ziyaret et
                visited.Add(currentId);
                visitedOrder.Add(currentId);

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null)
                    continue;

                // Komşuları kontrol et
                var edges = graph.GetEdges(currentNode);
                foreach (var edge in edges)
                {
                    int neighborId = edge.ToNodeId;

                    if (visited.Contains(neighborId))
                        continue;

                    // Geçici g-score hesapla
                    double tentativeGScore = gScores[currentId] + edge.Weight;

                    // Daha iyi bir yol bulunduysa güncelle
                    if (tentativeGScore < gScores[neighborId])
                    {
                        // Eski entry'yi kaldır (varsa)
                        openSet.Remove((fScores[neighborId], neighborId));

                        // Güncelle
                        previousNodes[neighborId] = currentId;
                        gScores[neighborId] = tentativeGScore;
                        
                        double heuristic = targetNodeId.HasValue 
                            ? CalculateHeuristic(graph, neighborId, targetNodeId.Value)
                            : 0;
                        
                        fScores[neighborId] = tentativeGScore + heuristic;

                        // Open set'e ekle
                        openSet.Add((fScores[neighborId], neighborId));
                    }
                }
            }

            return visitedOrder;
        }

        
        /// Heuristic fonksiyonu: İki node arasındaki tahmini maliyet
        /// Euclidean distance kullanır (node'larda X,Y varsa)
        /// Yoksa node özelliklerinin benzerliğini kullanır
        
        private double CalculateHeuristic(Graph graph, int fromId, int toId)
        {
            var fromNode = graph.GetNode(fromId);
            var toNode = graph.GetNode(toId);

            if (fromNode == null || toNode == null)
                return 0;

            // Eğer node'larda pozisyon bilgisi varsa Euclidean distance kullan
            if ((fromNode.X != 0 || fromNode.Y != 0) && (toNode.X != 0 || toNode.Y != 0))
            {
                return fromNode.DistanceTo(toNode);
            }

            // Yoksa node özelliklerinin farkını kullan (admissible heuristic)
            // Bu heuristic her zaman gerçek maliyetten küçük veya eşit olmalı
            double activityDiff = Math.Abs(fromNode.Activity - toNode.Activity);
            double interactionDiff = Math.Abs(fromNode.InteractionCount - toNode.InteractionCount);
            double connectionDiff = Math.Abs(fromNode.ConnectionCount - toNode.ConnectionCount);

            // Normalize et (0-1 arası)
            double totalDiff = (activityDiff + interactionDiff / 100.0 + connectionDiff / 10.0) / 3.0;
            
            return totalDiff;
        }

        
        /// Hedefe olan yolu reconstruct eder
        
        private List<int> ReconstructPath(int targetId)
        {
            if (!previousNodes.ContainsKey(targetId))
                return new List<int>();

            // Eğer hedefe ulaşılamıyorsa
            if (gScores[targetId] == double.PositiveInfinity)
                return new List<int>();

            var path = new List<int>();
            int? currentId = targetId;

            while (currentId.HasValue)
            {
                path.Add(currentId.Value);
                currentId = previousNodes[currentId.Value];
            }

            path.Reverse();
            return path;
        }

        
        /// Belirli bir node'a olan maliyeti döndürür
        
        public double GetCost(int nodeId)
        {
            if (!gScores.ContainsKey(nodeId))
                throw new InvalidOperationException("FindPath() or Execute() must be called first.");

            return gScores[nodeId];
        }

        
        /// Tüm maliyetleri döndürür (debugging için)
        
        public Dictionary<int, double> GetAllCosts()
        {
            return new Dictionary<int, double>(gScores);
        }
    }
}
