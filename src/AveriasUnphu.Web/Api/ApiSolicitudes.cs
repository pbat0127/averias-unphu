using AveriasUnphu.Web.Dominio;
using AveriasUnphu.Web.Servicios;

namespace AveriasUnphu.Web.Api;

/// <summary>Respuesta de la validación de duplicados.</summary>
public sealed record RespuestaValidacion(bool Duplicada, Solicitud? Vigente);

/// <summary>Respuesta cuando el reporte es duplicado (HTTP 409).</summary>
public sealed record RespuestaDuplicado(string Mensaje, Solicitud Vigente);

/// <summary>Respuesta cuando faltan datos (HTTP 400).</summary>
public sealed record RespuestaErrores(IReadOnlyList<string> Errores);

/// <summary>Cuerpo del cambio de estado.</summary>
public sealed record CambioEstado(string? Estado);

/// <summary>Rutas de la API de solicitudes.</summary>
public static class ApiSolicitudes
{
    public static IEndpointRouteBuilder MapApiSolicitudes(this IEndpointRouteBuilder rutas)
    {
        var grupo = rutas.MapGroup("/api/solicitudes");

        // GET /api/solicitudes/validar?aulaId=1&categoriaId=2
        // Dice si ya hay una solicitud activa para el aula y la categoría, sin guardar nada.
        grupo.MapGet("/validar", async (int aulaId, int categoriaId, ServicioSolicitudes servicio) =>
        {
            var vigente = await servicio.BuscarActivaAsync(aulaId, categoriaId);
            return Results.Ok(new RespuestaValidacion(vigente is not null, vigente));
        });

        // POST /api/solicitudes
        // Valida duplicados ANTES de guardar.
        //   201 Created  → se registró la avería
        //   409 Conflict → duplicado: devuelve la solicitud vigente
        //   400          → faltan datos
        grupo.MapPost("", async (NuevaSolicitud datos, ServicioSolicitudes servicio) =>
        {
            var resultado = await servicio.RegistrarAsync(datos);
            return resultado.Tipo switch
            {
                TipoResultado.Registrada => Results.Created(
                    $"/api/solicitudes/{resultado.Solicitud!.Id}", resultado.Solicitud),
                TipoResultado.Duplicada => Results.Conflict(new RespuestaDuplicado(
                    "Ya existe una solicitud activa para esta aula y categoría.", resultado.Vigente!)),
                _ => Results.BadRequest(new RespuestaErrores(resultado.Errores)),
            };
        });

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
        grupo.MapGet("/{id:int}", async (int id, ServicioSolicitudes servicio) =>
            await servicio.ObtenerAsync(id) is { } solicitud ? Results.Ok(solicitud) : Results.NotFound());

        return rutas;
    }
}
