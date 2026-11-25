using AveriasUnphu.Web.Datos;
using AveriasUnphu.Web.Dominio;

namespace AveriasUnphu.Web.Servicios;

public enum TipoCambioEstado { Actualizada, NoEncontrada, Invalido, Conflicto }

public sealed record ResultadoCambioEstado(TipoCambioEstado Tipo, Solicitud? Solicitud, string? Error)
{
    public static ResultadoCambioEstado Actualizada(Solicitud s) => new(TipoCambioEstado.Actualizada, s, null);
    public static ResultadoCambioEstado NoEncontrada() => new(TipoCambioEstado.NoEncontrada, null, "La solicitud no existe.");
    public static ResultadoCambioEstado Invalido(string error) => new(TipoCambioEstado.Invalido, null, error);
    public static ResultadoCambioEstado Conflicto(string error) => new(TipoCambioEstado.Conflicto, null, error);
}

/// <summary>
/// Bandeja de seguimiento del perfil Mantenimiento: listar solicitudes por prioridad
/// y avanzar su estado (Pendiente → En proceso → Resuelta).
/// </summary>
public sealed class ServicioSeguimiento(IRepositorioSolicitudes solicitudes)
{
    /// <summary>Lista con filtros. Devuelve null si el filtro de estado no es válido.</summary>
    public async Task<IReadOnlyList<Solicitud>?> ListarAsync(FiltroSolicitudes filtro)
    {
        var estado = string.IsNullOrWhiteSpace(filtro.Estado) ? null : filtro.Estado.Trim();
        if (estado is not null && estado != FiltroSolicitudes.SoloActivas && !Estados.Todos.Contains(estado))
            return null;
        return await solicitudes.ListarAsync(filtro with { Estado = estado });
    }

    public async Task<ResultadoCambioEstado> CambiarEstadoAsync(int id, string? estadoNuevo)
    {
        if (estadoNuevo is null || !Estados.Todos.Contains(estadoNuevo))
            return ResultadoCambioEstado.Invalido($"Estado desconocido: \"{estadoNuevo}\".");

        var solicitud = await solicitudes.ObtenerAsync(id);
        if (solicitud is null) return ResultadoCambioEstado.NoEncontrada();

        var siguiente = Estados.Siguiente(solicitud.Estado);
        if (siguiente is null)
            return ResultadoCambioEstado.Invalido("La solicitud ya está Resuelta. Si la avería vuelve, se reporta una nueva.");
        if (estadoNuevo != siguiente)
            return ResultadoCambioEstado.Invalido(
                $"No se puede pasar de {solicitud.Estado} a {estadoNuevo}. El siguiente estado es {siguiente}.");

        if (!await solicitudes.ActualizarEstadoAsync(id, solicitud.Estado, estadoNuevo))
            return ResultadoCambioEstado.Conflicto("La solicitud cambió mientras tanto. Actualice la bandeja.");

        return ResultadoCambioEstado.Actualizada((await solicitudes.ObtenerAsync(id))!);
    }
}
