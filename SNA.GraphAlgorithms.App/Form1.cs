using SNA.GraphAlgorithms.Core.Algorithms;
using SNA.GraphAlgorithms.Core.Models;
using SNA.GraphAlgorithms.Core.Services;
using SNA.GraphAlgorithms.Infrastructure.FileServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SNA.GraphAlgorithms.App
{
    public partial class Form1 : Form
    {
        // Graf ve veri yönetimi
        private Graph graph = new Graph();
        private CsvLoader csvLoader = new CsvLoader();
        private GraphExporter graphExporter = new GraphExporter();

        // Otomatik kayıt için dosya yolu
        private readonly string autoSaveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SNA-GraphAlgorithms",
            "autosave_graph.json");

        // Görselleştirme
        private Dictionary<int, PointF> nodePositions = new Dictionary<int, PointF>();
        private Dictionary<int, Color> nodeColors = new Dictionary<int, Color>();
        private int? selectedNodeId = null;
        private int? highlightedNodeId = null;
        private List<int> highlightedPath = new List<int>();
        private List<int> highlightedNodes = new List<int>();

        // UI kontrolleri
        private Panel graphPanel = null!;
        private Panel controlPanel = null!;
        private Panel resultsPanel = null!;
        private ListBox resultListBox = null!;
        private RichTextBox infoTextBox = null!;
        private ComboBox algorithmComboBox = null!;
        private ComboBox startNodeComboBox = null!;
        private ComboBox endNodeComboBox = null!;
        private Button runAlgorithmButton = null!;
        private ToolStripStatusLabel statusLabel = null!;

        // Renk paleti (Welsh-Powell için) - Modern ve canlı renkler
        private readonly Color[] colorPalette = new Color[]
        {
            Color.FromArgb(255, 107, 129),  // Canlı Pembe-Kırmızı
            Color.FromArgb(46, 213, 115),   // Neon Yeşil
            Color.FromArgb(30, 144, 255),   // Dodger Mavi
            Color.FromArgb(255, 215, 0),    // Altın Sarı
            Color.FromArgb(165, 94, 234),   // Parlak Mor
            Color.FromArgb(255, 165, 2),    // Turuncu
            Color.FromArgb(29, 209, 161),   // Neon Turkuaz
            Color.FromArgb(243, 156, 18),   // Kehribar
            Color.FromArgb(0, 206, 201),    // Cyan-Turkuaz
            Color.FromArgb(116, 185, 255)   // Açık Mavi
        };

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            
            // Önceki oturumdan kaydedilmiş graf varsa yükle, yoksa örnek veri
            if (!LoadAutoSavedGraph())
            {
                LoadSampleData();
            }

            // Form kapanırken otomatik kaydet
            this.FormClosing += Form1_FormClosing;
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "SNA Graph Algorithms Dashboard";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 40);

            // Ana menü
            CreateMainMenu();

            // Control Panel (Sol)
            CreateControlPanel();

            // Graph Panel (Orta)
            CreateGraphPanel();

            // Results Panel (Sağ)
            CreateResultsPanel();

            // Status Bar
            CreateStatusBar();
        }

        private void CreateMainMenu()
        {
            var menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(30, 30, 50);
            menuStrip.ForeColor = Color.White;

            // File Menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.ForeColor = Color.White;
            fileMenu.DropDownItems.Add("Load CSV", null, LoadCsvClick);
            fileMenu.DropDownItems.Add("Export to JSON", null, ExportJsonClick);
            fileMenu.DropDownItems.Add("Export to CSV", null, ExportCsvClick);
            fileMenu.DropDownItems.Add("Export All", null, ExportAllClick);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            // Dropdown öğelerini siyah yap
            foreach (ToolStripItem item in fileMenu.DropDownItems)
                item.ForeColor = Color.Black;

            // Graph Menu
            var graphMenu = new ToolStripMenuItem("Graph");
            graphMenu.ForeColor = Color.White;
            graphMenu.DropDownItems.Add("Load Sample Data", null, (s, e) => LoadSampleData());
            graphMenu.DropDownItems.Add("Clear Graph", null, (s, e) => ClearGraph());
            graphMenu.DropDownItems.Add("Add Node", null, AddNodeClick);
            graphMenu.DropDownItems.Add("Add Edge", null, AddEdgeClick);
            // Dropdown öğelerini siyah yap
            foreach (ToolStripItem item in graphMenu.DropDownItems)
                item.ForeColor = Color.Black;

            // Algorithms Menu
            var algoMenu = new ToolStripMenuItem("Algorithms");
            algoMenu.ForeColor = Color.White;
            algoMenu.DropDownItems.Add("Run BFS", null, (s, e) => RunAlgorithm("BFS"));
            algoMenu.DropDownItems.Add("Run DFS", null, (s, e) => RunAlgorithm("DFS"));
            algoMenu.DropDownItems.Add("Run Dijkstra", null, (s, e) => RunAlgorithm("Dijkstra"));
            algoMenu.DropDownItems.Add("Run A*", null, (s, e) => RunAlgorithm("A*"));
            algoMenu.DropDownItems.Add(new ToolStripSeparator());
            algoMenu.DropDownItems.Add("Welsh-Powell Coloring", null, (s, e) => RunAlgorithm("Welsh-Powell"));
            algoMenu.DropDownItems.Add("Connected Components", null, (s, e) => RunAlgorithm("Connected Components"));
            algoMenu.DropDownItems.Add("Degree Centrality", null, (s, e) => RunAlgorithm("Degree Centrality"));
            // Dropdown öğelerini siyah yap
            foreach (ToolStripItem item in algoMenu.DropDownItems)
                item.ForeColor = Color.Black;

            // Help Menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.ForeColor = Color.White;
            helpMenu.DropDownItems.Add("About", null, ShowAbout);
            // Dropdown öğelerini siyah yap
            foreach (ToolStripItem item in helpMenu.DropDownItems)
                item.ForeColor = Color.Black;

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(graphMenu);
            menuStrip.Items.Add(algoMenu);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateControlPanel()
        {
            controlPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(35, 35, 60),
                Padding = new Padding(10)
            };

            int y = 40;

            // Header
            var titleLabel = CreateLabel("⚡ ALGORITHM CONTROL", 10, y, 260, true);
            controlPanel.Controls.Add(titleLabel);
            y += 40;

            // Algorithm selection
            controlPanel.Controls.Add(CreateLabel("Algorithm:", 10, y, 260));
            y += 25;

            algorithmComboBox = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            algorithmComboBox.Items.AddRange(new object[] {
                "BFS (Breadth-First)",
                "DFS (Depth-First)",
                "Dijkstra (Shortest Path)",
                "A* (Heuristic Pathfinding)",
                "Welsh-Powell (Coloring)",
                "Connected Components",
                "Degree Centrality (Top 5)"
            });
            algorithmComboBox.SelectedIndex = 0;
            controlPanel.Controls.Add(algorithmComboBox);
            y += 40;

            // Start node
            controlPanel.Controls.Add(CreateLabel("Start Node:", 10, y, 260));
            y += 25;

            startNodeComboBox = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 75),
                ForeColor = Color.White
            };
            controlPanel.Controls.Add(startNodeComboBox);
            y += 40;

            // End node (for A*)
            controlPanel.Controls.Add(CreateLabel("Target Node (for A*):", 10, y, 260));
            y += 25;

            endNodeComboBox = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 75),
                ForeColor = Color.White
            };
            controlPanel.Controls.Add(endNodeComboBox);
            y += 50;

            // Çalıştır butonu
            runAlgorithmButton = new Button
            {
                Text = "▶ RUN ALGORITHM",
                Location = new Point(10, y),
                Size = new Size(260, 45),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            runAlgorithmButton.FlatAppearance.BorderSize = 0;
            runAlgorithmButton.Click += RunAlgorithmButton_Click;
            controlPanel.Controls.Add(runAlgorithmButton);
            y += 60;

            // Ayırıcı
            var separator = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(10, y),
                Size = new Size(260, 2)
            };
            controlPanel.Controls.Add(separator);
            y += 20;

            // Graph Stats
            controlPanel.Controls.Add(CreateLabel("📊 GRAPH STATISTICS", 10, y, 260, true));
            y += 30;

            infoTextBox = new RichTextBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 200),
                BackColor = Color.FromArgb(45, 45, 75),
                ForeColor = Color.FromArgb(220, 220, 240),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Consolas", 9)
            };
            controlPanel.Controls.Add(infoTextBox);
            y += 220;

            // Quick Buttons
            var btnClear = CreateButton("🗑 Clear", 10, y, 125, Color.FromArgb(255, 107, 129));
            btnClear.Click += (s, e) => ClearHighlights();
            controlPanel.Controls.Add(btnClear);

            var btnRefresh = CreateButton("🔄 Refresh", 145, y, 125, Color.FromArgb(100, 149, 237));
            btnRefresh.Click += (s, e) => RefreshUI();
            controlPanel.Controls.Add(btnRefresh);

            this.Controls.Add(controlPanel);
        }

        private void CreateGraphPanel()
        {
            graphPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            graphPanel.Paint += GraphPanel_Paint;
            graphPanel.MouseClick += GraphPanel_MouseClick;
            graphPanel.MouseMove += GraphPanel_MouseMove;
            graphPanel.Resize += (s, e) => { CalculateNodePositions(); graphPanel.Invalidate(); };

            this.Controls.Add(graphPanel);
        }

        private void CreateResultsPanel()
        {
            resultsPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = Color.FromArgb(35, 35, 60),
                Padding = new Padding(10)
            };

            int y = 40;

            // Results Header
            var resultsTitle = CreateLabel("📋 ALGORITHM RESULTS", 10, y, 300, true);
            resultsPanel.Controls.Add(resultsTitle);
            y += 40;

            // Sonuç listesi
            resultListBox = new ListBox
            {
                Location = new Point(10, y),
                Size = new Size(300, 500),
                BackColor = Color.FromArgb(45, 45, 75),
                ForeColor = Color.FromArgb(220, 220, 240),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10)
            };
            resultListBox.SelectedIndexChanged += ResultListBox_SelectedIndexChanged;
            resultsPanel.Controls.Add(resultListBox);
            y += 520;

            // Result info label
            var resultInfoLabel = CreateLabel("Run an algorithm or click a node", 10, y, 300);
            resultInfoLabel.ForeColor = Color.FromArgb(149, 165, 166);
            resultsPanel.Controls.Add(resultInfoLabel);

            this.Controls.Add(resultsPanel);
        }

        private void CreateStatusBar()
        {
            var statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(25, 25, 45)
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Ready | Use File > Load CSV to import a graph",
                ForeColor = Color.FromArgb(236, 240, 241)
            };

            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
        }

        private Label CreateLabel(string text, int x, int y, int width, bool isBold = false)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 25),
                ForeColor = Color.FromArgb(236, 240, 241),
                Font = new Font("Segoe UI", isBold ? 11 : 9, isBold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private Button CreateButton(string text, int x, int y, int width, Color bgColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 35),
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ========== GRAF ÇİZİMİ ==========

        private void GraphPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (graph.Nodes.Count == 0)
            {
                DrawEmptyMessage(e.Graphics);
                return;
            }

            // Edge'leri çiz
            DrawEdges(e.Graphics);

            // Node'ları çiz
            DrawNodes(e.Graphics);
        }

        private void DrawEmptyMessage(Graphics g)
        {
            string message = "Graph is empty\n\nUse File > Load CSV or Graph > Load Sample Data\nto start analysis";
            var font = new Font("Segoe UI", 14);
            var brush = new SolidBrush(Color.FromArgb(149, 165, 166));
            var size = g.MeasureString(message, font);
            float x = (graphPanel.Width - size.Width) / 2;
            float y = (graphPanel.Height - size.Height) / 2;
            g.DrawString(message, font, brush, x, y);
        }

        private void DrawEdges(Graphics g)
        {
            var drawnEdges = new HashSet<string>();

            foreach (var edge in graph.Edges)
            {
                string key = $"{Math.Min(edge.FromNodeId, edge.ToNodeId)}-{Math.Max(edge.FromNodeId, edge.ToNodeId)}";
                if (drawnEdges.Contains(key))
                    continue;
                drawnEdges.Add(key);

                if (!nodePositions.ContainsKey(edge.FromNodeId) || !nodePositions.ContainsKey(edge.ToNodeId))
                    continue;

                var fromPos = nodePositions[edge.FromNodeId];
                var toPos = nodePositions[edge.ToNodeId];

                // Yol vurgulama
                bool isHighlighted = highlightedPath.Count > 0 &&
                    highlightedPath.Contains(edge.FromNodeId) &&
                    highlightedPath.Contains(edge.ToNodeId);

                Color edgeColor = isHighlighted ? Color.FromArgb(0, 230, 170) : Color.FromArgb(80, 80, 110);
                float width = isHighlighted ? 3f : 1.5f;

                using (var pen = new Pen(edgeColor, width))
                {
                    g.DrawLine(pen, fromPos, toPos);
                }

                // Weight label
                if (edge.Weight < 1)
                {
                    var midPoint = new PointF((fromPos.X + toPos.X) / 2, (fromPos.Y + toPos.Y) / 2);
                    string weightText = edge.Weight.ToString("F3");
                    var font = new Font("Segoe UI", 7);
                    g.DrawString(weightText, font, new SolidBrush(Color.FromArgb(149, 165, 166)), midPoint);
                }
            }
        }

        private void DrawNodes(Graphics g)
        {
            int nodeRadius = 25;

            foreach (var node in graph.Nodes)
            {
                if (!nodePositions.ContainsKey(node.Id))
                    continue;

                var pos = nodePositions[node.Id];
                var rect = new RectangleF(pos.X - nodeRadius, pos.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                // Node rengi
                Color fillColor = GetNodeColor(node.Id);

                // Seçili veya vurgulu durumu
                if (node.Id == selectedNodeId)
                {
                    using (var pen = new Pen(Color.FromArgb(255, 215, 0), 4))
                    {
                        g.DrawEllipse(pen, rect);
                    }
                }
                else if (highlightedNodes.Contains(node.Id))
                {
                    using (var pen = new Pen(Color.FromArgb(0, 230, 170), 3))
                    {
                        g.DrawEllipse(pen, rect);
                    }
                }

                // Node doldur
                using (var brush = new SolidBrush(fillColor))
                {
                    g.FillEllipse(brush, rect);
                }

                // Node kenarı
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawEllipse(pen, rect);
                }

                // Node ID
                var font = new Font("Segoe UI", 10, FontStyle.Bold);
                var textSize = g.MeasureString(node.Id.ToString(), font);
                float textX = pos.X - textSize.Width / 2;
                float textY = pos.Y - textSize.Height / 2;
                g.DrawString(node.Id.ToString(), font, Brushes.White, textX, textY);

                // Node ismi (altında)
                var nameFont = new Font("Segoe UI", 8);
                var nameSize = g.MeasureString(node.Name, nameFont);
                float nameX = pos.X - nameSize.Width / 2;
                float nameY = pos.Y + nodeRadius + 5;
                g.DrawString(node.Name, nameFont, new SolidBrush(Color.FromArgb(189, 195, 199)), nameX, nameY);
            }
        }

        private Color GetNodeColor(int nodeId)
        {
            if (nodeColors.ContainsKey(nodeId))
                return nodeColors[nodeId];

            return Color.FromArgb(99, 110, 230); // Varsayılan mor-mavi
        }

        private void CalculateNodePositions()
        {
            nodePositions.Clear();

            if (graph.Nodes.Count == 0)
                return;

            // Padding artırıldı ve merkez hesaplandı
            int padding = 100;
            int availableWidth = graphPanel.Width - (padding * 2);
            int availableHeight = graphPanel.Height - (padding * 2);
            int centerX = graphPanel.Width / 2;
            int centerY = graphPanel.Height / 2;

            int nodeCount = graph.Nodes.Count;

            if (nodeCount == 1)
            {
                nodePositions[graph.Nodes[0].Id] = new PointF(centerX, centerY);
                return;
            }

            // Dairesel yerleşim - yarıçap küçültüldü
            double angleStep = 2 * Math.PI / nodeCount;
            // Min boyutu kullan ve daha küçük bir çarpan ile böl
            double radius = Math.Min(availableWidth, availableHeight) / 3.0;
            
            // Minimum yarıçap kontrolü
            if (radius < 100) radius = 100;
            // Maksimum yarıçap kontrolü (ekrandan taşmasın)
            double maxRadius = Math.Min(graphPanel.Width, graphPanel.Height) / 2.5;
            if (radius > maxRadius) radius = maxRadius;

            for (int i = 0; i < nodeCount; i++)
            {
                var node = graph.Nodes[i];
                double angle = i * angleStep - Math.PI / 2; // Üstten başla
                float x = (float)(centerX + radius * Math.Cos(angle));
                float y = (float)(centerY + radius * Math.Sin(angle));
                nodePositions[node.Id] = new PointF(x, y);
            }
        }

        // ========== EVENT HANDLERS ==========

        private void GraphPanel_MouseClick(object sender, MouseEventArgs e)
        {
            int nodeRadius = 25;

            foreach (var node in graph.Nodes)
            {
                if (!nodePositions.ContainsKey(node.Id))
                    continue;

                var pos = nodePositions[node.Id];
                double dist = Math.Sqrt(Math.Pow(e.X - pos.X, 2) + Math.Pow(e.Y - pos.Y, 2));

                if (dist <= nodeRadius)
                {
                    selectedNodeId = node.Id;
                    ShowNodeDetails(node);
                    graphPanel.Invalidate();
                    return;
                }
            }

            selectedNodeId = null;
            graphPanel.Invalidate();
        }

        private void GraphPanel_MouseMove(object sender, MouseEventArgs e)
        {
            int nodeRadius = 25;
            int? newHighlight = null;

            foreach (var node in graph.Nodes)
            {
                if (!nodePositions.ContainsKey(node.Id))
                    continue;

                var pos = nodePositions[node.Id];
                double dist = Math.Sqrt(Math.Pow(e.X - pos.X, 2) + Math.Pow(e.Y - pos.Y, 2));

                if (dist <= nodeRadius)
                {
                    newHighlight = node.Id;
                    break;
                }
            }

            if (newHighlight != highlightedNodeId)
            {
                highlightedNodeId = newHighlight;
                graphPanel.Invalidate();
            }
        }

        private void ResultListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Seçili sonucu vurgula
            if (resultListBox.SelectedItem != null)
            {
                string item = resultListBox.SelectedItem.ToString();
                // Düğüm ID'sini çıkarmaya çalış
                if (int.TryParse(item.Split(' ')[0].Replace("ID:", "").Replace(":", ""), out int nodeId))
                {
                    if (graph.NodeById.ContainsKey(nodeId))
                    {
                        selectedNodeId = nodeId;
                        graphPanel.Invalidate();
                    }
                }
            }
        }

        private void RunAlgorithmButton_Click(object sender, EventArgs e)
        {
            string selected = algorithmComboBox.SelectedItem?.ToString() ?? "";

            if (selected.Contains("BFS"))
                RunAlgorithm("BFS");
            else if (selected.Contains("DFS"))
                RunAlgorithm("DFS");
            else if (selected.Contains("Dijkstra"))
                RunAlgorithm("Dijkstra");
            else if (selected.Contains("A*"))
                RunAlgorithm("A*");
            else if (selected.Contains("Welsh"))
                RunAlgorithm("Welsh-Powell");
            else if (selected.Contains("Bağlı"))
                RunAlgorithm("Connected Components");
            else if (selected.Contains("Centrality"))
                RunAlgorithm("Degree Centrality");
        }

        // ========== ALGORİTMA ÇALIŞTIRMA ==========

        private void RunAlgorithm(string algorithmName)
        {
            if (graph.Nodes.Count == 0)
            {
                MessageBox.Show("Graph is empty! Please load data first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ClearHighlights();
                resultListBox.Items.Clear();

                int startId = GetSelectedStartNodeId();

                switch (algorithmName)
                {
                    case "BFS":
                        RunBFS(startId);
                        break;
                    case "DFS":
                        RunDFS(startId);
                        break;
                    case "Dijkstra":
                        RunDijkstra(startId);
                        break;
                    case "A*":
                        RunAStar(startId);
                        break;
                    case "Welsh-Powell":
                        RunWelshPowell();
                        break;
                    case "Connected Components":
                        RunConnectedComponents();
                        break;
                    case "Degree Centrality":
                        RunDegreeCentrality();
                        break;
                }

                graphPanel.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Algoritma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedStartNodeId()
        {
            if (startNodeComboBox.SelectedItem != null)
            {
                string selected = startNodeComboBox.SelectedItem.ToString();
                if (int.TryParse(selected.Split(' ')[0], out int id))
                    return id;
            }
            return graph.Nodes.First().Id;
        }

        private int GetSelectedEndNodeId()
        {
            if (endNodeComboBox.SelectedItem != null)
            {
                string selected = endNodeComboBox.SelectedItem.ToString();
                if (int.TryParse(selected.Split(' ')[0], out int id))
                    return id;
            }
            return graph.Nodes.Last().Id;
        }

        private void RunBFS(int startId)
        {
            var bfs = new BFS();
            var result = bfs.Execute(graph, startId);

            resultListBox.Items.Add("=== BFS Results ===");
            resultListBox.Items.Add($"Start: {graph.GetNode(startId)?.Name} (ID:{startId})");
            resultListBox.Items.Add("");
            resultListBox.Items.Add("Visit Order:");

            foreach (int nodeId in result)
            {
                var node = graph.GetNode(nodeId);
                resultListBox.Items.Add($"  {nodeId}: {node?.Name}");
            }

            highlightedPath = result;
            highlightedNodes = result;
            UpdateStatus($"BFS completed. {result.Count} nodes visited.");
        }

        private void RunDFS(int startId)
        {
            var dfs = new DFS();
            var result = dfs.Execute(graph, startId);

            resultListBox.Items.Add("=== DFS Results ===");
            resultListBox.Items.Add($"Start: {graph.GetNode(startId)?.Name} (ID:{startId})");
            resultListBox.Items.Add("");
            resultListBox.Items.Add("Visit Order:");

            foreach (int nodeId in result)
            {
                var node = graph.GetNode(nodeId);
                resultListBox.Items.Add($"  {nodeId}: {node?.Name}");
            }

            highlightedPath = result;
            highlightedNodes = result;
            UpdateStatus($"DFS completed. {result.Count} nodes visited.");
        }

        private void RunDijkstra(int startId)
        {
            var dijkstra = new Dijkstra();
            dijkstra.Execute(graph, startId);

            resultListBox.Items.Add("=== Dijkstra Results ===");
            resultListBox.Items.Add($"Start: {graph.GetNode(startId)?.Name} (ID:{startId})");
            resultListBox.Items.Add("");
            resultListBox.Items.Add("Distances:");

            foreach (var node in graph.Nodes.OrderBy(n => n.Id))
            {
                double dist = dijkstra.GetDistance(node.Id);
                string distStr = dist == double.PositiveInfinity ? "∞" : dist.ToString("F4");
                resultListBox.Items.Add($"  {node.Name}: {distStr}");
            }

            int endId = GetSelectedEndNodeId();
            var path = dijkstra.GetShortestPath(endId);

            resultListBox.Items.Add("");
            resultListBox.Items.Add($"Shortest Path ({graph.GetNode(startId)?.Name} -> {graph.GetNode(endId)?.Name}):");
            resultListBox.Items.Add($"  {string.Join(" -> ", path.Select(id => graph.GetNode(id)?.Name))}");
            resultListBox.Items.Add($"  Cost: {dijkstra.GetDistance(endId):F4}");

            highlightedPath = path;
            highlightedNodes = path;
            UpdateStatus($"Dijkstra completed. Shortest path length: {path.Count}");
        }

        private void RunAStar(int startId)
        {
            int endId = GetSelectedEndNodeId();
            var aStar = new AStar();
            var path = aStar.FindPath(graph, startId, endId);

            resultListBox.Items.Add("=== A* Results ===");
            resultListBox.Items.Add($"Start: {graph.GetNode(startId)?.Name}");
            resultListBox.Items.Add($"Target: {graph.GetNode(endId)?.Name}");
            resultListBox.Items.Add("");

            if (path.Count > 0)
            {
                resultListBox.Items.Add("Path Found:");
                resultListBox.Items.Add($"  {string.Join(" -> ", path.Select(id => graph.GetNode(id)?.Name))}");
                resultListBox.Items.Add($"  Cost: {aStar.GetCost(endId):F4}");
            }
            else
            {
                resultListBox.Items.Add("Path not found!");
            }

            highlightedPath = path;
            highlightedNodes = path;
            UpdateStatus($"A* completed. Path length: {path.Count}");
        }

        private void RunWelshPowell()
        {
            var welshPowell = new WelshPowell();
            welshPowell.Execute(graph, 0);

            var colors = welshPowell.GetAllColors();
            int chromaticNumber = welshPowell.GetChromaticNumber();
            var groups = welshPowell.GetColorGroups();

            resultListBox.Items.Add("=== Welsh-Powell Coloring ===");
            resultListBox.Items.Add($"Chromatic Number: {chromaticNumber}");
            resultListBox.Items.Add("");

            foreach (var group in groups.OrderBy(g => g.Key))
            {
                resultListBox.Items.Add($"Color {group.Key}:");
                foreach (int nodeId in group.Value)
                {
                    var node = graph.GetNode(nodeId);
                    resultListBox.Items.Add($"  - {node?.Name} (ID:{nodeId})");
                }
            }

            // Apply colors
            nodeColors.Clear();
            foreach (var kvp in colors)
            {
                int colorIndex = (kvp.Value - 1) % colorPalette.Length;
                nodeColors[kvp.Key] = colorPalette[colorIndex];
            }

            UpdateStatus($"Welsh-Powell completed. {chromaticNumber} colors used.");
        }

        private void RunConnectedComponents()
        {
            var cc = new ConnectedComponents();
            cc.Execute(graph, 0);

            var components = cc.GetAllComponents();

            resultListBox.Items.Add("=== Connected Components ===");
            resultListBox.Items.Add($"Total Components: {cc.GetComponentCount()}");
            resultListBox.Items.Add($"Is Graph Connected: {(cc.IsGraphConnected() ? "Yes" : "No")}");
            resultListBox.Items.Add("");

            int colorIndex = 0;
            nodeColors.Clear();

            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                resultListBox.Items.Add($"Component {i + 1} ({component.Count} nodes):");

                foreach (int nodeId in component)
                {
                    var node = graph.GetNode(nodeId);
                    resultListBox.Items.Add($"  - {node?.Name} (ID:{nodeId})");
                    nodeColors[nodeId] = colorPalette[colorIndex % colorPalette.Length];
                }
                colorIndex++;
                resultListBox.Items.Add("");
            }

            UpdateStatus($"Connected Components analysis completed. {components.Count} components found.");
        }

        private void RunDegreeCentrality()
        {
            var dc = new DegreeCentrality();
            dc.Execute(graph, 0);

            var topNodes = dc.GetTopNodes(5);

            resultListBox.Items.Add("=== Degree Centrality ===");
            resultListBox.Items.Add($"Average Centrality: {dc.GetAverageCentrality():F4}");
            resultListBox.Items.Add($"Graph Density: {dc.GetGraphDensity(graph):F4}");
            resultListBox.Items.Add("");
            resultListBox.Items.Add("Top 5 Influential Nodes:");

            highlightedNodes.Clear();
            foreach (var (nodeId, centrality, degree) in topNodes)
            {
                var node = graph.GetNode(nodeId);
                resultListBox.Items.Add($"  #{topNodes.IndexOf((nodeId, centrality, degree)) + 1}: {node?.Name}");
                resultListBox.Items.Add($"      Degree: {degree}, Centrality: {centrality:F4}");
                highlightedNodes.Add(nodeId);
            }

            resultListBox.Items.Add("");
            resultListBox.Items.Add("All Nodes (sorted):");

            foreach (var node in graph.Nodes.OrderByDescending(n => dc.GetCentrality(n.Id)))
            {
                resultListBox.Items.Add($"  {node.Name}: {dc.GetCentrality(node.Id):F4} (degree: {dc.GetDegree(node.Id)})");
            }

            UpdateStatus($"Degree Centrality analysis completed. Top node: {graph.GetNode(topNodes[0].NodeId)?.Name}");
        }

        // ========== YARDIMCI METODLAR ==========

        private void ShowNodeDetails(Node node)
        {
            resultListBox.Items.Clear();
            resultListBox.Items.Add($"=== Node Details ===");
            resultListBox.Items.Add($"ID: {node.Id}");
            resultListBox.Items.Add($"Name: {node.Name}");
            resultListBox.Items.Add($"Activity: {node.Activity}");
            resultListBox.Items.Add($"Interactions: {node.InteractionCount}");
            resultListBox.Items.Add($"Connection Count: {node.ConnectionCount}");
            resultListBox.Items.Add($"Degree: {node.Neighbors.Count}");
            resultListBox.Items.Add("");
            resultListBox.Items.Add("Neighbors:");

            foreach (int neighborId in node.Neighbors)
            {
                var neighbor = graph.GetNode(neighborId);
                var edge = graph.Edges.FirstOrDefault(e => 
                    (e.FromNodeId == node.Id && e.ToNodeId == neighborId));
                resultListBox.Items.Add($"  -> {neighbor?.Name} (w: {edge?.Weight:F4})");
            }
        }

        private void LoadSampleData()
        {
            graph = new Graph();

            // Sample data
            var nodes = new[]
            {
                new Node { Id = 1, Name = "User A", Activity = 8.5, InteractionCount = 120, ConnectionCount = 15 },
                new Node { Id = 2, Name = "User B", Activity = 7.2, InteractionCount = 95, ConnectionCount = 12 },
                new Node { Id = 3, Name = "User C", Activity = 9.0, InteractionCount = 150, ConnectionCount = 18 },
                new Node { Id = 4, Name = "User D", Activity = 6.8, InteractionCount = 80, ConnectionCount = 10 },
                new Node { Id = 5, Name = "User E", Activity = 7.5, InteractionCount = 110, ConnectionCount = 14 },
                new Node { Id = 6, Name = "User F", Activity = 8.2, InteractionCount = 130, ConnectionCount = 16 },
                new Node { Id = 7, Name = "User G", Activity = 6.5, InteractionCount = 75, ConnectionCount = 9 },
                new Node { Id = 8, Name = "User H", Activity = 9.5, InteractionCount = 160, ConnectionCount = 20 }
            };

            foreach (var node in nodes)
                graph.AddNode(node);

            // Edge connections
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            graph.AddEdge(2, 4);
            graph.AddEdge(2, 5);
            graph.AddEdge(3, 4);
            graph.AddEdge(3, 6);
            graph.AddEdge(4, 7);
            graph.AddEdge(5, 6);
            graph.AddEdge(5, 8);
            graph.AddEdge(6, 8);
            graph.AddEdge(7, 8);

            RefreshUI();
            UpdateStatus("Sample data loaded. 8 nodes, 11 edges.");
        }

        private void RefreshUI()
        {
            // Düğüm combobox'larını güncelle
            startNodeComboBox.Items.Clear();
            endNodeComboBox.Items.Clear();

            foreach (var node in graph.Nodes.OrderBy(n => n.Id))
            {
                string item = $"{node.Id} - {node.Name}";
                startNodeComboBox.Items.Add(item);
                endNodeComboBox.Items.Add(item);
            }

            if (startNodeComboBox.Items.Count > 0)
            {
                startNodeComboBox.SelectedIndex = 0;
                endNodeComboBox.SelectedIndex = Math.Min(endNodeComboBox.Items.Count - 1, endNodeComboBox.Items.Count - 1);
            }

            // İstatistikleri güncelle
            UpdateStatistics();

            // Node pozisyonlarını hesapla
            CalculateNodePositions();

            // Graf panelini yeniden çiz
            graphPanel.Invalidate();
        }

        private void UpdateStatistics()
        {
            var stats = graph.GetStatistics();

            infoTextBox.Clear();
            infoTextBox.AppendText($"Nodes: {stats.NodeCount}\n");
            infoTextBox.AppendText($"Edges: {stats.EdgeCount}\n");
            infoTextBox.AppendText($"Density: {stats.Density:F4}\n");
            infoTextBox.AppendText($"Avg. Degree: {stats.AvgDegree:F2}\n");
            infoTextBox.AppendText($"\n--- Nodes ---\n");

            foreach (var node in graph.Nodes.OrderBy(n => n.Id))
            {
                infoTextBox.AppendText($"{node.Id}: {node.Name} (d:{node.Neighbors.Count})\n");
            }
        }

        private void ClearHighlights()
        {
            highlightedPath.Clear();
            highlightedNodes.Clear();
            nodeColors.Clear();
            graphPanel.Invalidate();
        }

        private void ClearGraph()
        {
            graph.Clear();
            ClearHighlights();
            resultListBox.Items.Clear();
            RefreshUI();
            UpdateStatus("Graph cleared.");
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }

        // ========== DOSYA İŞLEMLERİ ==========

        private void LoadCsvClick(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "CSV Files|*.csv|All Files|*.*",
                Title = "Select CSV File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    graph = csvLoader.LoadGraph(dialog.FileName, createFullyConnected: true);
                    RefreshUI();
                    UpdateStatus($"CSV Loaded: {graph.Nodes.Count} nodes, {graph.Edges.Count / 2} edges");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"CSV load error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportJsonClick(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Save as JSON"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    graphExporter.ExportToJson(graph, dialog.FileName);
                    UpdateStatus($"JSON saved: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportCsvClick(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                Title = "Save as CSV"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    graphExporter.ExportNodesToCsv(graph, dialog.FileName);
                    UpdateStatus($"CSV saved: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportAllClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Export Folder"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    graphExporter.ExportAll(graph, dialog.SelectedPath);
                    UpdateStatus($"All files saved: {dialog.SelectedPath}");
                    MessageBox.Show($"Files exported successfully to:\n{dialog.SelectedPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddNodeClick(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new node details:\nFormat: Name,Activity,Interactions,Connections\nExample: NewUser,7.5,100,12",
                "Add Node",
                "NewUser,7.5,100,12");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    var parts = input.Split(',');
                    int newId = graph.Nodes.Count > 0 ? graph.Nodes.Max(n => n.Id) + 1 : 1;

                    var node = new Node
                    {
                        Id = newId,
                        Name = parts[0].Trim(),
                        Activity = double.Parse(parts[1].Trim()),
                        InteractionCount = int.Parse(parts[2].Trim()),
                        ConnectionCount = int.Parse(parts[3].Trim())
                    };

                    graph.AddNode(node);
                    RefreshUI();
                    UpdateStatus($"Node added: {node.Name} (ID:{node.Id})");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Düğüm ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddEdgeClick(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter edge details:\nFormat: FromID,ToID\nExample: 1,2",
                "Add Edge",
                "1,2");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    var parts = input.Split(',');
                    int fromId = int.Parse(parts[0].Trim());
                    int toId = int.Parse(parts[1].Trim());

                    graph.AddEdge(fromId, toId);
                    RefreshUI();
                    UpdateStatus($"Edge added: {fromId} <-> {toId}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kenar ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "SNA Graph Algorithms Dashboard\n\n" +
                "Social Network Analysis - Graph Algorithms Toolkit\n\n" +
                "Implementation includes:\n" +
                "• BFS (Breadth-First Search)\n" +
                "• DFS (Depth-First Search)\n" +
                "• Dijkstra (Shortest Path)\n" +
                "• A* (Heuristic Pathfinding)\n" +
                "• Welsh-Powell (Graph Coloring)\n" +
                "• Connected Components\n" +
                "• Degree Centrality",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ========== OTOMATİK KAYIT/YÜKLEME ==========

        /// <summary>
        /// Form kapanırken grafı otomatik kaydet
        /// </summary>
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            AutoSaveGraph();
        }

        /// <summary>
        /// Önceki oturumdan kaydedilmiş grafı yükler
        /// </summary>
        private bool LoadAutoSavedGraph()
        {
            try
            {
                if (!File.Exists(autoSaveFilePath))
                    return false;

                string jsonContent = File.ReadAllText(autoSaveFilePath);
                var graphData = System.Text.Json.JsonSerializer.Deserialize<AutoSaveData>(jsonContent);

                if (graphData == null || graphData.Nodes == null || graphData.Nodes.Count == 0)
                    return false;

                graph = new Graph();

                // Node'ları yükle
                foreach (var nodeData in graphData.Nodes)
                {
                    var node = new Node
                    {
                        Id = nodeData.Id,
                        Name = nodeData.Name,
                        Activity = nodeData.Activity,
                        InteractionCount = nodeData.InteractionCount,
                        ConnectionCount = nodeData.ConnectionCount,
                        X = nodeData.X,
                        Y = nodeData.Y
                    };
                    graph.AddNode(node);
                }

                // Edge'leri yükle
                if (graphData.Edges != null)
                {
                    foreach (var edgeData in graphData.Edges)
                    {
                        graph.AddEdge(edgeData.FromNodeId, edgeData.ToNodeId, edgeData.Weight, false);
                    }
                }

                RefreshUI();
                UpdateStatus("Graph loaded from previous session.");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-load hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Grafı otomatik olarak dosyaya kaydeder
        /// </summary>
        private void AutoSaveGraph()
        {
            try
            {
                if (graph.Nodes.Count == 0)
                    return;

                // Klasörü oluştur
                string? directory = Path.GetDirectoryName(autoSaveFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var saveData = new AutoSaveData
                {
                    Nodes = graph.Nodes.Select(n => new NodeData
                    {
                        Id = n.Id,
                        Name = n.Name,
                        Activity = n.Activity,
                        InteractionCount = n.InteractionCount,
                        ConnectionCount = n.ConnectionCount,
                        X = n.X,
                        Y = n.Y
                    }).ToList(),
                    Edges = graph.Edges
                        .Where(e => e.FromNodeId < e.ToNodeId)
                        .Select(e => new EdgeData
                        {
                            FromNodeId = e.FromNodeId,
                            ToNodeId = e.ToNodeId,
                            Weight = e.Weight
                        }).ToList(),
                    SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(autoSaveFilePath, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-save hatası: {ex.Message}");
            }
        }

        // Otomatik kayıt için veri sınıfları
        private class AutoSaveData
        {
            public List<NodeData> Nodes { get; set; } = new List<NodeData>();
            public List<EdgeData> Edges { get; set; } = new List<EdgeData>();
            public string SaveDate { get; set; } = "";
        }

        private class NodeData
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public double Activity { get; set; }
            public int InteractionCount { get; set; }
            public int ConnectionCount { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private class EdgeData
        {
            public int FromNodeId { get; set; }
            public int ToNodeId { get; set; }
            public double Weight { get; set; }
        }
    }
}

