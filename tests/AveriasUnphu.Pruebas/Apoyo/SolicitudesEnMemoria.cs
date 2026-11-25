using AveriasUnphu.Web.Datos;
using AveriasUnphu.Web.Dominio;

namespace AveriasUnphu.Pruebas.Apoyo;

/// <summary>Tabla de solicitudes en memoria. Se comporta como SQL Server en las pruebas.</summary>
public sealed class SolicitudesEnMemoria(CatalogosEnMemoria catalogos) : IRepositorioSolicitudes
{
    private readonly List<Solicitud> _filas = [];
    private readonly Lock _candado = new();

    public int Cantidad { get { lock (_candado) return _filas.Count; } }

    /// <summary>Agrega una solicitud directamente, como si ya estuviera en la base de datos.</summary>
    public Solicitud Agregar(Aula aula, CategoriaEquipo categoria, string estado,
        string prioridad = Prioridades.Media, string impacto = Impactos.AfectaParcialmente,
        string descripcion = "Avería de prueba.", DateTime? fecha = null)
    {
        lock (_candado)
        {
            var s = new Solicitud(
                _filas.Count + 1, aula.Id, $"{aula.Codigo} · {aula.Nombre}", categoria.Id, categoria.Nombre,
                CatalogosEnMemoria.Docente.Id, CatalogosEnMemoria.Docente.Nombre,
                descripcion, impacto, prioridad, estado, fecha ?? DateTime.Now.AddMinutes(-_filas.Count - 1),
                estado == Estados.Pendiente ? null : DateTime.Now,
                estado == Estados.Resuelta ? DateTime.Now : null);
            _filas.Add(s);
            return s;
        }
    }

    public Task<Solicitud?> ObtenerAsync(int id)
    {
        lock (_candado) return Task.FromResult(_filas.FirstOrDefault(s => s.Id == id));
    }

    public Task<IReadOnlyList<Solicitud>> ListarAsync(FiltroSolicitudes f)
    {
        static int Orden(string prioridad) => prioridad switch { Prioridades.Alta => 1, Prioridades.Media => 2, _ => 3 };
        lock (_candado)
        {
            IReadOnlyList<Solicitud> lista = _filas
                .Where(s => f.Estado is null
                            || (f.Estado == FiltroSolicitudes.SoloActivas && Estados.EsActivo(s.Estado))
                            || s.Estado == f.Estado)
                .Where(s => f.AulaId is null || s.AulaId == f.AulaId)
                .Where(s => f.CategoriaId is null || s.CategoriaId == f.CategoriaId)
                .OrderBy(s => Orden(s.Prioridad)).ThenBy(s => s.FechaReporte)
                .ToList();
            return Task.FromResult(lista);
        }
    }

    public Task<bool> ActualizarEstadoAsync(int id, string estadoActual, string estadoNuevo)
    {
        lock (_candado)
        {
            var i = _filas.FindIndex(s => s.Id == id && s.Estado == estadoActual);
            if (i < 0) return Task.FromResult(false);
            _filas[i] = _filas[i] with
            {
                Estado = estadoNuevo,
                FechaActualizacion = DateTime.Now,
                FechaResolucion = estadoNuevo == Estados.Resuelta ? DateTime.Now : _filas[i].FechaResolucion,
            };
            return Task.FromResult(true);
        }
    }
}
