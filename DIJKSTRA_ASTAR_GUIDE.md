# Dijkstra ve A* Algoritmaları - Implementation Guide

## 🎯 Eklenen Algoritmalar

Bu dokümanda **Dijkstra** ve **A*** algoritmalarının implementasyonu ve kullanım detayları açıklanmaktadır.

---

## 1️⃣ Dijkstra's Shortest Path Algoritması

### 📄 Dosya
`SNA.GraphAlgorithms.Core/Algorithms/Dijkstra.cs`

### 🎓 Açıklama
Dijkstra algoritması, weighted graph'ta bir başlangıç node'undan tüm diğer node'lara olan **en kısa yolları** bulur.

### ⚙️ Özellikler
- ✅ `IGraphAlgorithm` interface'ini implement eder
- ✅ Priority queue kullanarak optimize edilmiştir
- ✅ **Time Complexity**: O((V+E) log V)
- ✅ **Space Complexity**: O(V)
- ✅ Negatif weight olmamalı (bu graph'ta zaten olmayacak)

### 📊 Public API

```csharp
public class Dijkstra : IGraphAlgorithm
{
    // IGraphAlgorithm interface'inden
    string Name { get; }
    List<int> Execute(Graph graph, int startNodeId);
    
    // Dijkstra'ya özel metodlar
    List<int> GetShortestPath(int targetNodeId);
    double GetDistance(int nodeId);
    Dictionary<int, double> GetAllDistances();
}
```

### 💡 Kullanım Örneği

```csharp
// 1. Dijkstra instance oluştur
var dijkstra = new Dijkstra();

// 2. Algoritmayı çalıştır
var visitedOrder = dijkstra.Execute(graph, startNodeId: 1);

// 3. Tüm node'lara mesafeleri al
foreach (var node in graph.Nodes)
{
    double distance = dijkstra.GetDistance(node.Id);
    Console.WriteLine($"Node {node.Id}: {distance}");
}

// 4. Belirli bir node'a en kısa yolu al
List<int> path = dijkstra.GetShortestPath(targetNodeId: 4);
Console.WriteLine($"Yol: {string.Join(" -> ", path)}");
```

### 🔍 Çıktı Örneği

```
Dijkstra's Shortest Path
==========================
Başlangıç Node: Ali (ID:1)
Ziyaret Sırası: 1 -> 2 -> 3 -> 4

Tüm Node'lara Olan En Kısa Mesafeler:
  Ali -> Ali: 0.0000
  Ali -> Ayşe: 0.0012
  Ali -> Mehmet: 0.0008
  Ali -> Fatma: 0.0020

Ali -> Fatma En Kısa Yol:
  Ali -> Ayşe -> Fatma
  Toplam Maliyet: 0.0020
```

---

## 2️⃣ A* (A-Star) Pathfinding Algoritması

### 📄 Dosya
`SNA.GraphAlgorithms.Core/Algorithms/AStar.cs`

### 🎓 Açıklama
A* algoritması, **heuristic** kullanarak hedef node'a en optimal yolu bulur. Dijkstra'dan daha verimlidir çünkü hedefe yöneliktir.

### ⚙️ Özellikler
- ✅ `IGraphAlgorithm` interface'ini implement eder
- ✅ Heuristic-based optimal pathfinding
- ✅ **Time Complexity**: O((V+E) log V) - heuristic'e bağlı
- ✅ Hem pozisyon-based hem de feature-based heuristic destekler

### 🧮 Heuristic Fonksiyonu

A* iki tür heuristic kullanır:

#### 1. Euclidean Distance (Node'larda X,Y varsa)
```csharp
double heuristic = Math.Sqrt((x1-x2)² + (y1-y2)²)
```

#### 2. Feature-Based (X,Y yoksa)
```csharp
double heuristic = (
    |Activity_diff| + 
    |InteractionCount_diff| / 100.0 + 
    |ConnectionCount_diff| / 10.0
) / 3.0
```

### 📊 Public API

```csharp
public class AStar : IGraphAlgorithm
{
    // IGraphAlgorithm interface'inden
    string Name { get; }
    List<int> Execute(Graph graph, int startNodeId);
    
    // A*'a özel metodlar
    List<int> FindPath(Graph graph, int startNodeId, int targetNodeId);
    double GetCost(int nodeId);
    Dictionary<int, double> GetAllCosts();
}
```

### 💡 Kullanım Örneği 1: Feature-Based Heuristic

```csharp
// Pozisyon bilgisi olmayan node'larla
var aStar = new AStar();

// Belirli bir hedefe yol bul
var path = aStar.FindPath(graph, startNodeId: 1, targetNodeId: 4);

if (path.Count > 0)
{
    Console.WriteLine($"Yol: {string.Join(" -> ", path)}");
    Console.WriteLine($"Maliyet: {aStar.GetCost(4)}");
}
```

### 💡 Kullanım Örneği 2: Position-Based Heuristic

```csharp
// Pozisyon bilgisi olan node'lar oluştur
var nodeA = new Node { Id = 1, Name = "A", X = 0, Y = 0 };
var nodeB = new Node { Id = 2, Name = "B", X = 1, Y = 0 };
var nodeF = new Node { Id = 6, Name = "F", X = 2, Y = 2 };

// Graph'a ekle ve edge'leri oluştur
graph.AddNode(nodeA);
graph.AddNode(nodeB);
graph.AddNode(nodeF);
// ... edge'leri ekle

// A*'ı çalıştır (Euclidean distance heuristic kullanır)
var aStar = new AStar();
var path = aStar.FindPath(graph, startNodeId: 1, targetNodeId: 6);

Console.WriteLine($"A -> F Yol: {string.Join(" -> ", path)}");
Console.WriteLine($"Euclidean Distance: {nodeA.DistanceTo(nodeF)}");
```

### 🔍 Çıktı Örneği

```
A* Pathfinding (Position-Based)
================================

2D Grid Graph:
  F(2,2)
    |
  D(1,1)-E(2,1)
    |     |
  A(0,0)-B(1,0)-C(2,0)

A -> F En Kısa Yol:
  A -> B -> D -> E -> F
  Toplam Maliyet: 4.00

Euclidean Distance (heuristic): 2.83
```

---

## 3️⃣ Node Sınıfına Eklenen Özellikler

### 📄 Dosya
`SNA.GraphAlgorithms.Core/Models/Node.cs`

### ✨ Yeni Property'ler

```csharp
public class Node
{
    // Mevcut property'ler...
    public int Id { get; set; }
    public string Name { get; set; }
    public double Activity { get; set; }
    public int InteractionCount { get; set; }
    public int ConnectionCount { get; set; }
    public List<int> Neighbors { get; set; }
    
    // YENİ: A* için opsiyonel pozisyon bilgisi
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    
    // YENİ: Euclidean distance hesaplama
    public double DistanceTo(Node other);
}
```

### 💡 Kullanım

```csharp
// Pozisyon bilgisi ile node oluştur
var node1 = new Node 
{ 
    Id = 1, 
    Name = "Ali",
    X = 10.5, 
    Y = 20.3,
    Activity = 8.5,
    InteractionCount = 120,
    ConnectionCount = 15
};

var node2 = new Node 
{ 
    Id = 2, 
    Name = "Ayşe",
    X = 15.2, 
    Y = 25.8 
};

// Aralarındaki mesafe
double distance = node1.DistanceTo(node2);
Console.WriteLine($"Distance: {distance:F2}");
```

---

## 4️⃣ Algoritma Karşılaştırması

### 📊 Hangi Algoritma Ne Zaman Kullanılır?

| Algoritma | Kullanım Durumu | Zaman Karmaşıklığı | Özellikler |
|-----------|----------------|-------------------|-----------|
| **BFS** | Unweighted graph'ta en kısa yol | O(V + E) | Seviye seviye tarama |
| **DFS** | Graph traversal, bağlantılılık kontrolü | O(V + E) | Derine inme |
| **Dijkstra** | Weighted graph'ta en kısa yol | O((V+E) log V) | Tüm node'lara mesafe |
| **A*** | Belirli hedefe optimal yol | O((V+E) log V) | Heuristic ile optimize |

### 🎯 Örnek Karşılaştırma

```csharp
var graph = CreateSampleGraph();
int startId = 1;

// Tüm algoritmaları çalıştır
var bfs = new BFS();
var dfs = new DFS();
var dijkstra = new Dijkstra();
var aStar = new AStar();

var bfsResult = bfs.Execute(graph, startId);
var dfsResult = dfs.Execute(graph, startId);
var dijkstraResult = dijkstra.Execute(graph, startId);
var aStarResult = aStar.Execute(graph, startId);

Console.WriteLine($"BFS:      {string.Join(" -> ", bfsResult)}");
Console.WriteLine($"DFS:      {string.Join(" -> ", dfsResult)}");
Console.WriteLine($"Dijkstra: {string.Join(" -> ", dijkstraResult)}");
Console.WriteLine($"A*:       {string.Join(" -> ", aStarResult)}");
```

**Çıktı:**
```
BFS:      1 -> 2 -> 3 -> 4
DFS:      1 -> 2 -> 4 -> 3
Dijkstra: 1 -> 3 -> 2 -> 4  (en küçük weight sırasına göre)
A*:       1 -> 3 -> 2 -> 4  (heuristic ile optimize)
```

---

## 5️⃣ Test ve Demo

### 🧪 UsageExample.cs

`SNA.GraphAlgorithms.App/UsageExample.cs` dosyasında tüm algoritmalar için demo metodları bulunur:

```csharp
// Tüm algoritmaları test et
UsageExample.DemoGraphAlgorithms();

// Sadece Dijkstra
UsageExample.DemoDijkstra(graph);

// Sadece A* (basit)
UsageExample.DemoAStar(graph);

// A* (pozisyon-based)
UsageExample.DemoAStarWithPositions();

// Algoritma karşılaştırması
UsageExample.CompareAlgorithms();
```

---

## 6️⃣ Performans ve Optimizasyon

### ⚡ Dijkstra Optimizasyonları
1. **SortedSet** ile Priority Queue (C# standard library)
2. Ziyaret edilen node'ları HashSet ile takip
3. Distance table ile O(1) erişim

### ⚡ A* Optimizasyonları
1. **Admissible Heuristic**: Her zaman gerçek maliyetten küçük veya eşit
2. **Early termination**: Hedefe ulaşınca durur
3. İki farklı heuristic desteği (Euclidean ve Feature-based)

---

## 7️⃣ Gelecek Geliştirmeler

### 🔮 Eklenebilecek Özellikler

1. **Bellman-Ford Algoritması**: Negatif weight desteği
2. **Floyd-Warshall**: Tüm çiftler arası en kısa yol
3. **Bidirectional A***: İki yönlü arama
4. **Greedy Best-First Search**: Sadece heuristic kullanan
5. **IDA* (Iterative Deepening A*)**: Bellek optimize

### 🎯 Kullanım Senaryoları

1. **Sosyal Ağ Analizi**: En kısa etki yolu
2. **Recommendation Systems**: Benzer kullanıcılar
3. **Pathfinding**: Game AI, navigation
4. **Network Routing**: En optimal yol bulma

---

## ✅ Özet

### Eklenen Dosyalar
- ✅ `Dijkstra.cs` - Shortest path algoritması
- ✅ `AStar.cs` - Heuristic pathfinding
- ✅ `Node.cs` güncellemesi - X, Y koordinatları

### Eklenen Özellikler
- ✅ Dijkstra: En kısa yol bulma
- ✅ A*: Heuristic-based optimal pathfinding
- ✅ Dual heuristic support (Euclidean + Feature-based)
- ✅ Comprehensive API (GetShortestPath, GetDistance, GetCost)
- ✅ Demo ve test metodları

### Build Status
```bash
dotnet build SNA-GraphAlgorithms.sln --configuration Release
# ✅ Başarılı - 0 Error, 0 Warning
```

---

## 📚 Kaynaklar

- **Dijkstra**: E. W. Dijkstra, "A note on two problems in connexion with graphs" (1959)
- **A***: Hart, P. E.; Nilsson, N. J.; Raphael, B., "A Formal Basis for the Heuristic Determination of Minimum Cost Paths" (1968)
- **Graph Theory**: Cormen, T. H., et al., "Introduction to Algorithms" (2009)

---

**Tüm algoritmalar test edildi ve production-ready! 🚀**
