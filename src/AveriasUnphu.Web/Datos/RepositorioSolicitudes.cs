using System.Data.Common;
using AveriasUnphu.Web.Dominio;
using Microsoft.Data.SqlClient;

namespace AveriasUnphu.Web.Datos;

/// <summary>Acceso a la tabla de solicitudes.</summary>
public interface IRepositorioSolicitudes
{
    /// <summary>Devuelve la solicitud activa (Pendiente o En proceso) del aula y categoría, o null si no hay.</summary>
    Task<Solicitud?> BuscarActivaAsync(int aulaId, int categoriaId);

    /// <summary>Guarda la solicitud y devuelve su número.</summary>
    /// <exception cref="SolicitudDuplicadaException">Ya hay una solicitud activa para el aula y la categoría.</exception>
    Task<int> InsertarAsync(SolicitudParaGuardar solicitud);

    Task<Solicitud?> ObtenerAsync(int id);

    /// <summary>Lista las solicitudes por prioridad (Alta, Media, Baja) y, dentro de cada una, de la más antigua a la más nueva.</summary>
    Task<IReadOnlyList<Solicitud>> ListarAsync(FiltroSolicitudes filtro);

    /// <summary>Cambia el estado solo si la solicitud sigue en <paramref name="estadoActual"/>. Devuelve false si no se cambió.</summary>
    Task<bool> ActualizarEstadoAsync(int id, string estadoActual, string estadoNuevo);
}

public sealed class RepositorioSolicitudesSql(ConexionSql conexionSql) : IRepositorioSolicitudes
{
    private const string Columnas = """
        s.id, s.aula_id, a.codigo + N' · ' + a.nombre AS aula,
        s.categoria_id, c.nombre AS categoria,
        s.usuario_id, u.nombre AS reportado_por,
        s.descripcion, s.impacto, s.prioridad, s.estado,
        s.fecha_reporte, s.fecha_actualizacion, s.fecha_resolucion
        """;

    private const string Tablas = """
        FROM dbo.solicitudes AS s
        JOIN dbo.aulas AS a             ON a.id = s.aula_id
        JOIN dbo.categorias_equipo AS c ON c.id = s.categoria_id
        JOIN dbo.usuarios AS u          ON u.id = s.usuario_id
        """;

    // Misma consulta que db/04_consulta_solicitudes_activas.sql (tarea 2.1).
    private const string SqlBuscarActiva = $"""
        SELECT TOP (1) {Columnas}
        {Tablas}
        WHERE s.aula_id = @aula_id
          AND s.categoria_id = @categoria_id
          AND s.estado IN (N'Pendiente', N'En proceso')
        ORDER BY s.fecha_reporte DESC
        """;

    private const string SqlInsertar = """
        INSERT INTO dbo.solicitudes (aula_id, categoria_id, usuario_id, descripcion, impacto, prioridad)
        OUTPUT INSERTED.id
        VALUES (@aula_id, @categoria_id, @usuario_id, @descripcion, @impacto, @prioridad)
        """;

    private const string SqlObtener = $"SELECT {Columnas} {Tablas} WHERE s.id = @id";

    private const string SqlListar = $"""
        SELECT {Columnas}
        {Tablas}
        WHERE (@estado IS NULL
               OR (@estado = N'activas' AND s.estado IN (N'Pendiente', N'En proceso'))
               OR s.estado = @estado)
          AND (@aula_id IS NULL OR s.aula_id = @aula_id)
          AND (@categoria_id IS NULL OR s.categoria_id = @categoria_id)
        ORDER BY CASE s.prioridad WHEN N'Alta' THEN 1 WHEN N'Media' THEN 2 ELSE 3 END,
                 s.fecha_reporte
        """;

    private const string SqlActualizarEstado = """
        UPDATE dbo.solicitudes
        SET estado = @nuevo,
            fecha_actualizacion = SYSDATETIME(),
            fecha_resolucion = CASE WHEN @nuevo = N'Resuelta' THEN SYSDATETIME() ELSE fecha_resolucion END
        WHERE id = @id AND estado = @actual
        """;

    // Errores de SQL Server por violar un índice o restricción única.
    private static readonly int[] ErroresDeLlaveDuplicada = [2601, 2627];

    public async Task<Solicitud?> BuscarActivaAsync(int aulaId, int categoriaId)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var lista = await ComandoSql.Crear(conexion, SqlBuscarActiva)
            .Con("@aula_id", aulaId)
            .Con("@categoria_id", categoriaId)
            .ListarAsync(Mapear);
        return lista.SingleOrDefault();
    }

    public async Task<int> InsertarAsync(SolicitudParaGuardar s)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var comando = ComandoSql.Crear(conexion, SqlInsertar)
            .Con("@aula_id", s.AulaId)
            .Con("@categoria_id", s.CategoriaId)
            .Con("@usuario_id", s.UsuarioId)
            .Con("@descripcion", s.Descripcion)
            .Con("@impacto", s.Impacto)
            .Con("@prioridad", s.Prioridad);
        try
        {
            return Convert.ToInt32(await comando.ExecuteScalarAsync());
        }
        catch (SqlException ex) when (ErroresDeLlaveDuplicada.Contains(ex.Number))
        {
            throw new SolicitudDuplicadaException(ex);
        }
    }

    public async Task<Solicitud?> ObtenerAsync(int id)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var lista = await ComandoSql.Crear(conexion, SqlObtener).Con("@id", id).ListarAsync(Mapear);
        return lista.SingleOrDefault();
    }

    public async Task<IReadOnlyList<Solicitud>> ListarAsync(FiltroSolicitudes filtro)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        return await ComandoSql.Crear(conexion, SqlListar)
            .Con("@estado", filtro.Estado)
            .Con("@aula_id", filtro.AulaId)
            .Con("@categoria_id", filtro.CategoriaId)
            .ListarAsync(Mapear);
    }

    public async Task<bool> ActualizarEstadoAsync(int id, string estadoActual, string estadoNuevo)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var filas = await ComandoSql.Crear(conexion, SqlActualizarEstado)
            .Con("@id", id)
            .Con("@actual", estadoActual)
            .Con("@nuevo", estadoNuevo)
            .ExecuteNonQueryAsync();
        return filas == 1;
    }

    internal static Solicitud Mapear(DbDataReader l) => new(
        l.Entero("id"),
        l.Entero("aula_id"), l.Texto("aula"),
        l.Entero("categoria_id"), l.Texto("categoria"),
        l.Entero("usuario_id"), l.Texto("reportado_por"),
        l.Texto("descripcion"), l.Texto("impacto"), l.Texto("prioridad"), l.Texto("estado"),
        l.Fecha("fecha_reporte"), l.FechaOpcional("fecha_actualizacion"), l.FechaOpcional("fecha_resolucion"));
}
