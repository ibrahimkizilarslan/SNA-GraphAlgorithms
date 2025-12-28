using SNA.GraphAlgorithms.Core.Models;
using System.Collections.Generic;

namespace SNA.GraphAlgorithms.Core.Algorithms
{
    
    /// Graph üzerinde çalışan algoritmaların ortak interface'i
    
    public interface IGraphAlgorithm
    {
        
        /// Algoritma adı
        
        string Name { get; }

        
        /// Graph üzerinde algoritmayı çalıştır
        
        /// <param name="graph">Üzerinde çalışılacak graph</param>
        /// <param name="startNodeId">Başlangıç düğümü ID</param>
        /// <returns>Ziyaret edilen düğümlerin ID listesi (ziyaret sırasına göre)</returns>
        List<int> Execute(Graph graph, int startNodeId);
    }
}
