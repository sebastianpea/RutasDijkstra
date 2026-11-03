using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RutasDijkstra
{
    public partial class FormPrincipal : Form
    {
        private Grafo grafo = Grafo.CrearRedCoahuila();

        private readonly ComboBox cmbOrigen = Combo();
        private readonly ComboBox cmbDestino = Combo();
        private readonly ComboBox cmbA = Combo();
        private readonly ComboBox cmbB = Combo();
        private readonly NumericUpDown nudPeso = new() { Minimum = 1, Maximum = 2000, Value = 100, Width = 80 };
        private readonly Label lblResultado = new() { AutoSize = true, MaximumSize = new Size(290, 0), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 12, 3, 3) };
        private readonly PanelGrafo panelGrafo = new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
        private readonly DataGridView dgvMatriz = Tabla();
        private readonly DataGridView dgvTraza = Tabla();
        private readonly TextBox txtLista = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 10.5F), BackColor = Color.White };

        public FormPrincipal()
        {
            Text = "Tarea 4.3 - Ruta más corta entre ciudades con el algoritmo de Dijkstra";
            Size = new Size(1200, 760);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            var pestanas = new TabControl { Dock = DockStyle.Fill };
            pestanas.TabPages.Add(Pagina("1. Red de ciudades", CrearPestanaRed()));
            pestanas.TabPages.Add(Pagina("2. Matriz de adyacencia", dgvMatriz));
            pestanas.TabPages.Add(Pagina("3. Lista de adyacencia", txtLista));
            pestanas.TabPages.Add(Pagina("4. Traza de Dijkstra", dgvTraza));
            Controls.Add(pestanas);

            CargarCiudades();
            cmbOrigen.SelectedIndex = 4;    // Piedras Negras
            cmbDestino.SelectedIndex = 7;   // Torreón
            Calcular();
        }

        private Control CrearPestanaRed()
        {
            var botonCalcular = new Button { Text = "Calcular ruta más corta", Width = 260, Height = 34 };
            botonCalcular.Click += (s, e) => Calcular();
            var botonGuardar = new Button { Text = "Agregar / actualizar arista", Width = 260 };
            botonGuardar.Click += (s, e) => EditarArista(eliminar: false);
            var botonEliminar = new Button { Text = "Eliminar arista", Width = 260 };
            botonEliminar.Click += (s, e) => EditarArista(eliminar: true);
            var botonRestaurar = new Button { Text = "Restaurar red original", Width = 260 };
            botonRestaurar.Click += (s, e) => { grafo = Grafo.CrearRedCoahuila(); Calcular(); };

            var controles = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            controles.Controls.AddRange(new Control[]
            {
            Etiqueta("Origen:"), cmbOrigen, Etiqueta("Destino:"), cmbDestino, botonCalcular, lblResultado,
            Etiqueta("──── Editar la red ────"), Etiqueta("Ciudad A:"), cmbA, Etiqueta("Ciudad B:"), cmbB,
            Etiqueta("Distancia (km):"), nudPeso, botonGuardar, botonEliminar, botonRestaurar
            });

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabla.Controls.Add(controles, 0, 0);
            tabla.Controls.Add(panelGrafo, 1, 0);
            return tabla;
        }

        private void CargarCiudades()
        {
            foreach (var c in new[] { cmbOrigen, cmbDestino, cmbA, cmbB })
            {
                c.Items.Clear();
                c.Items.AddRange(grafo.Nombres);
            }
            cmbA.SelectedIndex = 0;
            cmbB.SelectedIndex = 2;
        }

        private void EditarArista(bool eliminar)
        {
            try
            {
                if (eliminar) grafo.Desconectar(cmbA.SelectedIndex, cmbB.SelectedIndex);
                else grafo.ConectarOActualizar(cmbA.SelectedIndex, cmbB.SelectedIndex, (int)nudPeso.Value);
                Calcular();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Arista no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Ejecuta Dijkstra y actualiza el dibujo, el resultado, la matriz, la lista y la traza.</summary>
        private void Calcular()
        {
            int origen = cmbOrigen.SelectedIndex, destino = cmbDestino.SelectedIndex;
            ResultadoDijkstra r = grafo.Dijkstra(origen);
            List<int> camino = r.Camino(destino);

            lblResultado.Text = camino.Count == 0
                ? $"No existe ruta de {grafo.Nombres[origen]} a {grafo.Nombres[destino]}."
                : $"Ruta más corta ({r.Distancias[destino]} km):{Environment.NewLine}{string.Join(" → ", camino.Select(v => grafo.Nombres[v]))}";

            panelGrafo.Mostrar(grafo, camino);
            LlenarMatriz();
            LlenarLista();
            LlenarTraza(r);
        }

        private void LlenarMatriz()
        {
            dgvMatriz.Columns.Clear();
            dgvMatriz.Rows.Clear();
            dgvMatriz.Columns.Add("ciudad", "");
            foreach (string n in grafo.Nombres) dgvMatriz.Columns.Add(n, n);
            for (int i = 0; i < grafo.Vertices; i++)
            {
                var fila = new object[grafo.Vertices + 1];
                fila[0] = grafo.Nombres[i];
                for (int j = 0; j < grafo.Vertices; j++) fila[j + 1] = i == j ? "0" : grafo.Peso(i, j) == 0 ? "∞" : grafo.Peso(i, j).ToString();
                dgvMatriz.Rows.Add(fila);
            }
        }

        private void LlenarLista()
        {
            var lineas = new List<string>();
            for (int v = 0; v < grafo.Vertices; v++)
                lineas.Add($"{grafo.Nombres[v],-16} → " + string.Join(", ", grafo.Vecinos(v).Select(a => $"{grafo.Nombres[a.Destino]} ({a.Peso})")));
            txtLista.Lines = lineas.ToArray();
        }

        private void LlenarTraza(ResultadoDijkstra r)
        {
            dgvTraza.Columns.Clear();
            dgvTraza.Rows.Clear();
            dgvTraza.Columns.Add("it", "Iteración");
            dgvTraza.Columns.Add("sel", "Vértice seleccionado");
            dgvTraza.Columns.Add("rel", "Distancias mejoradas");
            foreach (string n in grafo.Nombres) dgvTraza.Columns.Add(n, n);
            dgvTraza.Columns["rel"]!.FillWeight = 220;
            dgvTraza.Columns["sel"]!.FillWeight = 140;

            foreach (PasoDijkstra p in r.Traza)
            {
                var fila = new object[grafo.Vertices + 3];
                fila[0] = p.Iteracion;
                fila[1] = $"{grafo.Nombres[p.Seleccionado]} ({p.DistanciaSeleccionado})";
                fila[2] = p.Relajaciones.Count == 0 ? "ninguna" : string.Join("; ", p.Relajaciones);
                for (int v = 0; v < grafo.Vertices; v++)
                {
                    string d = p.Distancias[v] == Grafo.Infinito ? "∞" : p.Distancias[v].ToString();
                    fila[v + 3] = p.Visitados[v] ? d + " ✓" : d;
                }
                dgvTraza.Rows.Add(fila);
            }
        }

        // ------------------------------------------------------------ Auxiliares
        private static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };

        private static DataGridView Tabla() => new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window
        };

        private static TabPage Pagina(string titulo, Control contenido)
        {
            var p = new TabPage(titulo) { Padding = new Padding(6) };
            p.Controls.Add(contenido);
            return p;
        }

        private static Label Etiqueta(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(3, 9, 3, 3) };
    }
}
