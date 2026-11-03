namespace RutasDijkstra;

/// <summary>Arista de la lista de adyacencia: ciudad destino y peso (km).</summary>
public record Arista(int Destino, int Peso);

/// <summary>Una fila de la traza: estado de las distancias después de cada iteración.</summary>
public record PasoDijkstra(int Iteracion, int Seleccionado, int DistanciaSeleccionado, int[] Distancias, int[] Predecesores, bool[] Visitados, List<string> Relajaciones);

/// <summary>Resultado de Dijkstra desde un origen.</summary>
public record ResultadoDijkstra(int[] Distancias, int[] Predecesores, List<PasoDijkstra> Traza)
{
    /// <summary>Reconstruye el camino al destino siguiendo los predecesores hacia atrás.</summary>
    public List<int> Camino(int destino)
    {
        var camino = new List<int>();
        if (Distancias[destino] == Grafo.Infinito) return camino;
        for (int v = destino; v != -1; v = Predecesores[v]) camino.Insert(0, v);
        return camino;
    }
}

/// <summary>
/// Grafo ponderado no dirigido. Se mantienen las dos representaciones:
/// matriz de adyacencia (para mostrarla) y lista de adyacencia (la que usa Dijkstra).
/// </summary>
public class Grafo
{
    public const int Infinito = int.MaxValue;

    private readonly int[,] matriz;
    private readonly List<Arista>[] lista;

    public string[] Nombres { get; }
    public int Vertices => Nombres.Length;

    public Grafo(string[] nombres)
    {
        Nombres = nombres;
        matriz = new int[nombres.Length, nombres.Length];
        lista = new List<Arista>[nombres.Length];
        for (int i = 0; i < nombres.Length; i++) lista[i] = new List<Arista>();
    }

    /// <summary>Agrega o actualiza una arista no dirigida en ambas representaciones.</summary>
    public void ConectarOActualizar(int a, int b, int peso)
    {
        if (a == b) throw new ArgumentException("Una ciudad no puede conectarse consigo misma.");
        if (peso <= 0) throw new ArgumentException("El peso debe ser positivo (Dijkstra no admite pesos negativos).");
        Desconectar(a, b);
        matriz[a, b] = matriz[b, a] = peso;
        lista[a].Add(new Arista(b, peso));
        lista[b].Add(new Arista(a, peso));
    }

    public void Desconectar(int a, int b)
    {
        matriz[a, b] = matriz[b, a] = 0;
        lista[a].RemoveAll(x => x.Destino == b);
        lista[b].RemoveAll(x => x.Destino == a);
    }

    /// <summary>Peso de la arista o 0 si no existe.</summary>
    public int Peso(int a, int b) => matriz[a, b];
    public IReadOnlyList<Arista> Vecinos(int v) => lista[v];

    /// <summary>
    /// Dijkstra en su versión con arreglo: en cada iteración se elige el vértice no visitado
    /// de menor distancia (O(V²)), lo que permite registrar una traza clara.
    /// </summary>
    public ResultadoDijkstra Dijkstra(int origen)
    {
        int n = Vertices;
        var distancia = new int[n];
        var predecesor = new int[n];
        var visitado = new bool[n];
        var traza = new List<PasoDijkstra>();

        Array.Fill(distancia, Infinito);
        Array.Fill(predecesor, -1);
        distancia[origen] = 0;

        for (int iteracion = 1; iteracion <= n; iteracion++)
        {
            // 1) Seleccionar el vértice no visitado con menor distancia tentativa
            int u = -1;
            for (int v = 0; v < n; v++)
                if (!visitado[v] && distancia[v] != Infinito && (u == -1 || distancia[v] < distancia[u])) u = v;
            if (u == -1) break;                     // los restantes son inalcanzables
            visitado[u] = true;

            // 2) Relajar las aristas de u
            var relajaciones = new List<string>();
            foreach (var (v, peso) in lista[u])
            {
                if (visitado[v]) continue;
                int nueva = distancia[u] + peso;
                if (nueva < distancia[v])
                {
                    relajaciones.Add($"{Nombres[v]}: {(distancia[v] == Infinito ? "∞" : distancia[v])} → {nueva}");
                    distancia[v] = nueva;
                    predecesor[v] = u;
                }
            }
            traza.Add(new PasoDijkstra(iteracion, u, distancia[u], (int[])distancia.Clone(), (int[])predecesor.Clone(), (bool[])visitado.Clone(), relajaciones));
        }
        return new ResultadoDijkstra(distancia, predecesor, traza);
    }

    /// <summary>Red de ciudades de Coahuila con distancias carreteras aproximadas (km).</summary>
    public static Grafo CrearRedCoahuila()
    {
        var g = new Grafo(new[] { "Monclova", "Castaños", "Saltillo", "Sabinas", "Piedras Negras", "Acuña", "Cuatro Ciénegas", "Torreón", "Parras" });
        g.ConectarOActualizar(0, 1, 15);    // Monclova - Castaños
        g.ConectarOActualizar(1, 2, 175);   // Castaños - Saltillo
        g.ConectarOActualizar(0, 3, 125);   // Monclova - Sabinas
        g.ConectarOActualizar(3, 4, 130);   // Sabinas - Piedras Negras
        g.ConectarOActualizar(3, 5, 200);   // Sabinas - Acuña
        g.ConectarOActualizar(4, 5, 95);    // Piedras Negras - Acuña
        g.ConectarOActualizar(0, 6, 80);    // Monclova - Cuatro Ciénegas
        g.ConectarOActualizar(6, 7, 230);   // Cuatro Ciénegas - Torreón
        g.ConectarOActualizar(2, 8, 160);   // Saltillo - Parras
        g.ConectarOActualizar(8, 7, 155);   // Parras - Torreón
        g.ConectarOActualizar(2, 7, 260);   // Saltillo - Torreón
        return g;
    }
}