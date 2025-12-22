# SNA Graph Algorithms 🚀

**Social Network Analysis - Graph Algorithms Implementation**

C# WinForms tabanlı, katmanlı mimari ile geliştirilmiş graf algoritmaları projesi.

---

## 📊 Proje Yapısı

```
SNA-GraphAlgorithms/
├── SNA.GraphAlgorithms.App           # UI Layer (WinForms)
├── SNA.GraphAlgorithms.Core          # Business Logic
│   ├── Algorithms/                   # Graph algoritmalar
│   │   ├── IGraphAlgorithm.cs       # Algoritma interface
│   │   ├── BFS.cs                   # Breadth-First Search
│   │   ├── DFS.cs                   # Depth-First Search
│   │   ├── Dijkstra.cs              # Shortest Path
│   │   └── AStar.cs                 # A* Pathfinding
│   ├── Models/                       # Domain modeller
│   │   ├── Node.cs                  # Düğüm (vertex)
│   │   ├── Edge.cs                  # Kenar (edge)
│   │   └── Graph.cs                 # Graf yapısı
│   └── Services/                     # Business servisler
│       └── WeightCalculator.cs      # Ağırlık hesaplama
└── SNA.GraphAlgorithms.Infrastructure # Data & External Services
    └── FileServices/
        └── CsvLoader.cs              # CSV veri okuma
```

---

## 🎯 Özellikler

### ✅ Implemented Algorithms

| Algoritma | Açıklama | Kompleksite | Kullanım |
|-----------|----------|------------|----------|
| **BFS** | Breadth-First Search | O(V + E) | Seviye seviye tarama |
| **DFS** | Depth-First Search | O(V + E) | Derinlik öncelikli tarama |
| **Dijkstra** | Shortest Path | O((V+E) log V) | En kısa yol bulma |
| **A*** | Heuristic Pathfinding | O((V+E) log V) | Optimal yol bulma |

### ✨ Core Features

- ✅ **Adjacency List** ile optimize graph yapısı
- ✅ **Weighted edges** (ağırlıklı kenarlar)
- ✅ **Undirected graph** desteği
- ✅ **Automatic weight calculation** (WeightCalculator)
- ✅ **CSV import/export** desteği
- ✅ **Position-based heuristics** (A* için)
- ✅ **SOLID principles** ile temiz kod
- ✅ **Interface-based design**

---

## 🚀 Hızlı Başlangıç

### 1. Projeyi Klonla

```bash
git clone https://github.com/yourusername/SNA-GraphAlgorithms.git
cd SNA-GraphAlgorithms
```

### 2. Build

```bash
dotnet build SNA-GraphAlgorithms.sln
```

### 3. Run

```bash
dotnet run --project SNA.GraphAlgorithms.App
```

---

## 💻 Kullanım Örnekleri

### 📝 Basit Graph Oluşturma

```csharp
using SNA.GraphAlgorithms.Core.Models;
using SNA.GraphAlgorithms.Core.Algorithms;

// Graph oluştur
var graph = new Graph();

// Node'ları ekle
graph.AddNode(new Node 
{ 
    Id = 1, 
    Name = "Ali",
    Activity = 8.5,
    InteractionCount = 120,
    ConnectionCount = 15
});

graph.AddNode(new Node 
{ 
    Id = 2, 
    Name = "Ayşe",
    Activity = 7.2,
    InteractionCount = 95,
    ConnectionCount = 12
});

// Edge ekle (weight otomatik hesaplanır)
graph.AddEdge(1, 2);
```

### 🔍 BFS Algoritması

```csharp
IGraphAlgorithm bfs = new BFS();
List<int> visitedNodes = bfs.Execute(graph, startNodeId: 1);

Console.WriteLine($"BFS Sonucu: {string.Join(" -> ", visitedNodes)}");
// Output: BFS Sonucu: 1 -> 2 -> 3 -> 4
```

### 🎯 Dijkstra Shortest Path

```csharp
var dijkstra = new Dijkstra();
dijkstra.Execute(graph, startNodeId: 1);

// Belirli bir node'a en kısa yol
var path = dijkstra.GetShortestPath(targetNodeId: 4);
double distance = dijkstra.GetDistance(4);

Console.WriteLine($"En Kısa Yol: {string.Join(" -> ", path)}");
Console.WriteLine($"Mesafe: {distance:F4}");
```

### 🧭 A* Pathfinding

```csharp
var aStar = new AStar();

// Belirli bir hedefe yol bul
var path = aStar.FindPath(graph, startNodeId: 1, targetNodeId: 6);
double cost = aStar.GetCost(6);

Console.WriteLine($"A* Yol: {string.Join(" -> ", path)}");
Console.WriteLine($"Maliyet: {cost:F4}");
```

### 📂 CSV'den Veri Yükleme

```csharp
using SNA.GraphAlgorithms.Infrastructure.FileServices;

var csvLoader = new CsvLoader();

// CSV'den node'ları yükle
List<Node> nodes = csvLoader.LoadNodes("data.csv");

// Tam bağlı graph oluştur
Graph graph = csvLoader.LoadGraph("data.csv", createFullyConnected: true);

Console.WriteLine($"Yüklenen: {graph.Nodes.Count} node, {graph.Edges.Count} edge");
```

**CSV Format:**
```csv
Id,Name,Activity,InteractionCount,ConnectionCount
1,Ali,8.5,120,15
2,Ayşe,7.2,95,12
3,Mehmet,9.0,150,18
```

---

## 🧮 Weight Hesaplama Formülü

Edge weight'leri otomatik olarak node özellikleri kullanılarak hesaplanır:

```
weight(i,j) = 1 / (1 + 
    (Activity_i - Activity_j)² +
    (InteractionCount_i - InteractionCount_j)² +
    (ConnectionCount_i - ConnectionCount_j)²
)
```

**Özellikler:**
- Weight: 0-1 arası normalize
- 1 = İki node tamamen benzer
- 0'a yakın = İki node çok farklı

---

## 📐 A* Heuristic

A* algoritması iki farklı heuristic destekler:

### 1. Euclidean Distance (Pozisyon varsa)

```csharp
var node1 = new Node { Id = 1, X = 0, Y = 0 };
var node2 = new Node { Id = 2, X = 3, Y = 4 };

double distance = node1.DistanceTo(node2); // 5.0
```

### 2. Feature-Based (Pozisyon yoksa)

Node özellikleri (Activity, InteractionCount, ConnectionCount) arasındaki farklar kullanılır.

---

## 🏗️ Mimari Prensipler

### SOLID Principles

- **Single Responsibility**: Her sınıf tek sorumluluk
- **Open/Closed**: Yeni algoritmalar kolayca eklenebilir
- **Liskov Substitution**: IGraphAlgorithm polymorphism
- **Interface Segregation**: Minimal interface'ler
- **Dependency Inversion**: Interface'e bağımlılık

### Design Patterns

- **Strategy Pattern**: IGraphAlgorithm
- **Factory Pattern**: Graph oluşturma
- **Repository Pattern**: CsvLoader

---

## 📚 Dokümantasyon

- **[REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)** - Refactoring detayları
- **[DIJKSTRA_ASTAR_GUIDE.md](DIJKSTRA_ASTAR_GUIDE.md)** - Dijkstra ve A* kullanım kılavuzu
- **[UsageExample.cs](SNA.GraphAlgorithms.App/UsageExample.cs)** - Kod örnekleri

---

## 🧪 Test

### Demo Çalıştırma

```csharp
using SNA.GraphAlgorithms.App;

// Tüm algoritmaları test et
UsageExample.DemoGraphAlgorithms();

// Algoritma karşılaştırması
UsageExample.CompareAlgorithms();

// Weight hesaplama demo
UsageExample.DemoWeightCalculation();
```

---

## 🔧 Gereksinimler

- **.NET 8.0** veya üzeri
- **Windows** (WinForms için)
- **Visual Studio 2022** veya **VS Code**

---

## 📦 NuGet Packages

Bu proje harici bir package kullanmamaktadır. Tamamen .NET standard library ile geliştirilmiştir.

---

## 🎓 Algoritma Seçim Rehberi

| Senaryo | Önerilen Algoritma | Neden? |
|---------|-------------------|--------|
| Unweighted graph tarama | **BFS** | Seviye seviye optimal |
| Graph bağlantılılığı | **DFS** | Tüm node'ları ziyaret |
| En kısa yol (tüm node'lar) | **Dijkstra** | Garantili optimal |
| Belirli hedefe yol | **A*** | Heuristic ile hızlı |
| Sosyal ağ mesafesi | **BFS** veya **Dijkstra** | Kullanım durumuna göre |

---

## 🚀 Gelecek Geliştirmeler

### Planlanıyor

- [ ] **Bellman-Ford**: Negatif weight desteği
- [ ] **Floyd-Warshall**: All-pairs shortest path
- [ ] **Prim's Algorithm**: Minimum spanning tree
- [ ] **Kruskal's Algorithm**: MST alternatif
- [ ] **PageRank**: Sosyal ağ önemi
- [ ] **Community Detection**: Kümeleme
- [ ] **Centrality Measures**: Betweenness, Closeness, Degree
- [ ] **Visualization**: Graph çizimi (WinForms)
- [ ] **Unit Tests**: Comprehensive test suite
- [ ] **Benchmark**: Performance testing

---

## 👥 Katkıda Bulunma

Katkılarınızı bekliyoruz! Lütfen pull request göndermeden önce:

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request açın

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

---

## 📞 İletişim

Proje Sahibi: [@ibrahimkzilarslan](https://github.com/ibrahimkzilarslan)

Project Link: [https://github.com/ibrahimkzilarslan/SNA-GraphAlgorithms](https://github.com/ibrahimkzilarslan/SNA-GraphAlgorithms)

---

## 🙏 Teşekkürler

Bu proje aşağıdaki kaynaklardan ilham almıştır:

- **Introduction to Algorithms** - Cormen, Leiserson, Rivest, Stein
- **Graph Theory** - Reinhard Diestel
- **Social Network Analysis** - Stanley Wasserman, Katherine Faust

---

**⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!**

---

## 📊 Proje İstatistikleri

- **Toplam Algoritma**: 4 (BFS, DFS, Dijkstra, A*)
- **Kod Satırı**: ~2000+
- **Test Coverage**: Coming soon
- **Build Status**: ✅ Passing

---

**Made with ❤️ for Graph Algorithm enthusiasts**