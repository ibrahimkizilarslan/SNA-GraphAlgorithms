using SNA.GraphAlgorithms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Degree Centrality Algoritması
    /// Düğümlerin merkezilik (centrality) değerlerini hesaplar
    /// En etkili düğümleri bulur
    /// </summary>
    public class DegreeCentrality : IGraphAlgorithm
    {
        public string Name => "Degree Centrality";

        // Son çalıştırmadan kalan merkezilik değerleri
        private Dictionary<int, double> centralityScores = new Dictionary<int, double>();
        private Dictionary<int, int> degreeValues = new Dictionary<int, int>();

        /// <summary>
        /// Degree Centrality algoritmasını çalıştırır
        /// </summary>
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Kullanılmıyor (tüm düğümler hesaplanır)</param>
        /// <returns>Merkezilik sırasına göre (azalan) düğüm ID listesi</returns>
        public List<int> Execute(Graph graph, int startNodeId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            // Önceki sonuçları temizle
            centralityScores = new Dictionary<int, double>();
            degreeValues = new Dictionary<int, int>();

            if (graph.Nodes.Count == 0)
                return new List<int>();

            int n = graph.Nodes.Count;

            // Her düğümün degree'sini ve centrality'sini hesapla
            foreach (var node in graph.Nodes)
            {
                int degree = node.Neighbors.Count;
                degreeValues[node.Id] = degree;

                // Normalize edilmiş centrality: degree / (n-1)
                double centrality = n > 1 ? (double)degree / (n - 1) : 0;
                centralityScores[node.Id] = centrality;
            }

            // Merkezilik sırasına göre sırala (azalan)
            return centralityScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Belirli bir düğümün centrality değerini döndürür
        /// </summary>
        public double GetCentrality(int nodeId)
        {
            if (!centralityScores.ContainsKey(nodeId))
                throw new InvalidOperationException($"Node {nodeId} centrality not calculated. Execute() must be called first.");

            return centralityScores[nodeId];
        }

        /// <summary>
        /// Belirli bir düğümün degree değerini döndürür
        /// </summary>
        public int GetDegree(int nodeId)
        {
            if (!degreeValues.ContainsKey(nodeId))
                throw new InvalidOperationException($"Node {nodeId} degree not calculated. Execute() must be called first.");

            return degreeValues[nodeId];
        }

        /// <summary>
        /// Tüm centrality değerlerini döndürür
        /// </summary>
        public Dictionary<int, double> GetAllCentralities()
        {
            return new Dictionary<int, double>(centralityScores);
        }

        /// <summary>
        /// En yüksek centrality'ye sahip N düğümü döndürür
        /// </summary>
        /// <param name="count">Döndürülecek düğüm sayısı</param>
        public List<(int NodeId, double Centrality, int Degree)> GetTopNodes(int count = 5)
        {
            return centralityScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => (kvp.Key, kvp.Value, degreeValues[kvp.Key]))
                .ToList();
        }

        /// <summary>
        /// En düşük centrality'ye sahip N düğümü döndürür (izole düğümler)
        /// </summary>
        public List<(int NodeId, double Centrality, int Degree)> GetBottomNodes(int count = 5)
        {
            return centralityScores
                .OrderBy(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => (kvp.Key, kvp.Value, degreeValues[kvp.Key]))
                .ToList();
        }

        /// <summary>
        /// Ortalama centrality değerini döndürür
        /// </summary>
        public double GetAverageCentrality()
        {
            if (centralityScores.Count == 0)
                return 0;

            return centralityScores.Values.Average();
        }

        /// <summary>
        /// Graf yoğunluğunu (density) hesaplar
        /// Density = 2*E / (V*(V-1))
        /// </summary>
        public double GetGraphDensity(Graph graph)
        {
            if (graph == null || graph.Nodes.Count <= 1)
                return 0;

            int v = graph.Nodes.Count;
            // Edges listesi çift yönlü tutulduğu için 2'ye bölüyoruz
            int e = graph.Edges.Count / 2;
            double maxEdges = (double)v * (v - 1) / 2;

            return e / maxEdges;
        }
    }
}