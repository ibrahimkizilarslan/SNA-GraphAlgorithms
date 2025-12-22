using SNA.GraphAlgorithms.Core.Models;
using System.Collections.Generic;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    /// <summary>
    /// Graph üzerinde çalışan algoritmaların ortak interface'i
    /// </summary>
    public interface IGraphAlgorithm
    {
        /// <summary>
        /// Algoritma adı
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Graph üzerinde algoritmayı çalıştır
        /// </summary>
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <returns>Ziyaret edilen düğümlerin ID listesi (ziyaret sırasına göre)</returns>
        List<int> Execute(Graph graph, int startNodeId);
    }
}
