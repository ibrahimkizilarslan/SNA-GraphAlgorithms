using SNA.GraphAlgorithms.Core.Models;
using System;

namespace SNA.GraphAlgorithms.Core.Services
{
    
    /// İki node arasındaki edge weight'ini hesaplayan statik servis
    
    public static class WeightCalculator
    {
        
        /// İki node arasındaki ağırlığı hesaplar
        /// Formül: weight(i,j) = 1 / (1 + (Activity_i - Activity_j)^2 + (Interaction_i - Interaction_j)^2 + (Connection_i - Connection_j)^2)
        
        /// <param name="nodeA">İlk node</param>
        /// <param name="nodeB">İkinci node</param>
        /// <returns>Hesaplanan ağırlık değeri (0-1 arası)</returns>
        public static double Calculate(Node nodeA, Node nodeB)
        {
            if (nodeA == null)
                throw new ArgumentNullException(nameof(nodeA));
            if (nodeB == null)
                throw new ArgumentNullException(nameof(nodeB));

            // Özellik farklarının karesi
            double activityDiff = Math.Pow(nodeA.Activity - nodeB.Activity, 2);
            double interactionDiff = Math.Pow(nodeA.InteractionCount - nodeB.InteractionCount, 2);
            double connectionDiff = Math.Pow(nodeA.ConnectionCount - nodeB.ConnectionCount, 2);

            // Formül: 1 / (1 + toplam_fark)
            double weight = 1.0 / (1.0 + activityDiff + interactionDiff + connectionDiff);

            return weight;
        }
    }
}
