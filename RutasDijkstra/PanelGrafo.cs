namespace RutasDijkstra;

/// <summary>Dibuja el grafo con posiciones aproximadas al mapa y resalta el camino más corto.</summary>
public class PanelGrafo : Panel
{
    // Posición relativa (0 a 1) de cada ciudad, en el mismo orden que Grafo.Nombres.
    private static readonly PointF[] Posiciones =
    {
        new(0.55f, 0.48f), new(0.66f, 0.60f), new(0.66f, 0.92f), new(0.60f, 0.28f), new(0.88f, 0.12f),
        new(0.58f, 0.05f), new(0.28f, 0.48f), new(0.05f, 0.86f), new(0.33f, 0.90f)
    };

    private Grafo? grafo;
    private List<int> camino = new();

    public PanelGrafo()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void Mostrar(Grafo g, List<int> caminoResaltado)
    {
        grafo = g;
        camino = caminoResaltado;
        Invalidate();
    }

    private PointF Punto(int v) => new(50 + Posiciones[v].X * (ClientSize.Width - 130), 30 + Posiciones[v].Y * (ClientSize.Height - 70));

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (grafo is null) return;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var aristasCamino = new HashSet<(int, int)>();
        for (int i = 1; i < camino.Count; i++)
        {
            aristasCamino.Add((camino[i - 1], camino[i]));
            aristasCamino.Add((camino[i], camino[i - 1]));
        }

        using var normal = new Pen(Color.Silver, 2f);
        using var resaltado = new Pen(Color.FromArgb(40, 170, 90), 5f);
        using var fuente = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var fondoPeso = new SolidBrush(Color.FromArgb(235, 255, 255, 255));

        // Aristas con su peso
        for (int a = 0; a < grafo.Vertices; a++)
        {
            for (int b = a + 1; b < grafo.Vertices; b++)
            {
                int peso = grafo.Peso(a, b);
                if (peso == 0) continue;
                PointF pa = Punto(a), pb = Punto(b);
                g.DrawLine(aristasCamino.Contains((a, b)) ? resaltado : normal, pa, pb);
                var medio = new PointF((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2);
                string texto = $"{peso} km";
                SizeF tam = g.MeasureString(texto, fuente);
                g.FillRectangle(fondoPeso, medio.X - tam.Width / 2, medio.Y - tam.Height / 2, tam.Width, tam.Height);
                g.DrawString(texto, fuente, Brushes.DimGray, medio.X - tam.Width / 2, medio.Y - tam.Height / 2);
            }
        }

        // Vértices
        for (int v = 0; v < grafo.Vertices; v++)
        {
            PointF p = Punto(v);
            bool enCamino = camino.Contains(v);
            Color color = camino.Count > 0 && v == camino[0] ? Color.Gold
                        : camino.Count > 0 && v == camino[^1] ? Color.Tomato
                        : enCamino ? Color.LightGreen : Color.LightSkyBlue;
            using var pincel = new SolidBrush(color);
            g.FillEllipse(pincel, p.X - 12, p.Y - 12, 24, 24);
            g.DrawEllipse(Pens.DimGray, p.X - 12, p.Y - 12, 24, 24);
            g.DrawString(grafo.Nombres[v], fuente, Brushes.Black, p.X + 14, p.Y - 8);
        }
    }
}