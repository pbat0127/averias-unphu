using AveriasUnphu.Web.Datos;
using AveriasUnphu.Web.Dominio;
using AveriasUnphu.Web.Servicios;

namespace AveriasUnphu.Web.Api;

/// <summary>Respuesta con mensajes de error (HTTP 400, 404 o 409).</summary>
public sealed record RespuestaErrores(IReadOnlyList<string> Errores);

/// <summary>Cuerpo del cambio de estado.</summary>
public sealed record CambioEstado(string? Estado);

/// <summary>Rutas de la API de solicitudes.</summary>
public static class ApiSolicitudes
{
    public static IEndpointRouteBuilder MapApiSolicitudes(this IEndpointRouteBuilder rutas)
    {
        var grupo = rutas.MapGroup("/api/solicitudes");

        // GET /api/solicitudes?estado=activas&aulaId=1&categoriaId=2
        // Bandeja de seguimiento: ordenadas por prioridad (Alta, Media, Baja) y por antigüedad.
        grupo.MapGet("", async (string? estado, int? aulaId, int? categoriaId, ServicioSeguimiento servicio) =>
            await servicio.ListarAsync(new FiltroSolicitudes(estado, aulaId, categoriaId)) is { } lista
                ? Results.Ok(lista)
                : Results.BadRequest(new RespuestaErrores([$"Estado desconocido: \"{estado}\"."])));

        // PATCH /api/solicitudes/5/estado   { "estado": "En proceso" }
        //   200 → estado actualizado · 400 → cambio no permitido · 404 → no existe · 409 → cambió mientras tanto
        grupo.MapPatch("/{id:int}/estado", async (int id, CambioEstado cambio, ServicioSeguimiento servicio) =>
        {
            var r = await servicio.CambiarEstadoAsync(id, cambio.Estado);
            return r.Tipo switch
            {
                TipoCambioEstado.Actualizada => Results.Ok(r.Solicitud),
                TipoCambioEstado.NoEncontrada => Results.NotFound(new RespuestaErrores([r.Error!])),
                TipoCambioEstado.Conflicto => Results.Conflict(new RespuestaErrores([r.Error!])),
                _ => Results.BadRequest(new RespuestaErrores([r.Error!])),
            };
        });

        // GET /api/solicitudes/5
        grupo.MapGet("/{id:int}", async (int id, IRepositorioSolicitudes solicitudes) =>
            await solicitudes.ObtenerAsync(id) is { } solicitud ? Results.Ok(solicitud) : Results.NotFound());

        return rutas;
    }
}
