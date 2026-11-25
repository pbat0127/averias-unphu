using AveriasUnphu.Web.Datos;

namespace AveriasUnphu.Web.Api;

/// <summary>Rutas de la API para leer los catálogos que usa el formulario.</summary>
public static class ApiCatalogos
{
    public static IEndpointRouteBuilder MapApiCatalogos(this IEndpointRouteBuilder rutas)
    {
        var grupo = rutas.MapGroup("/api/catalogos");

        // GET /api/catalogos/aulas
        grupo.MapGet("/aulas", (IRepositorioCatalogos catalogos) => catalogos.ListarAulasAsync());

        // GET /api/catalogos/categorias
        grupo.MapGet("/categorias", (IRepositorioCatalogos catalogos) => catalogos.ListarCategoriasAsync());

        // GET /api/catalogos/usuarios?perfil=Reportante
        grupo.MapGet("/usuarios", (string? perfil, IRepositorioCatalogos catalogos) =>
            catalogos.ListarUsuariosAsync(perfil));

        return rutas;
    }
}
