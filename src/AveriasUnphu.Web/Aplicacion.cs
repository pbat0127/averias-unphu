using AveriasUnphu.Web.Api;
using AveriasUnphu.Web.Datos;

namespace AveriasUnphu.Web;

/// <summary>
/// Arma la aplicación web: servicios, archivos estáticos (wwwroot) y rutas de la API.
/// </summary>
public static class Aplicacion
{
    /// <param name="args">Argumentos de la línea de comandos.</param>
    /// <param name="registrarDatos">
    /// Permite a las pruebas reemplazar el acceso a SQL Server por repositorios en memoria.
    /// Si es null, se usa SQL Server con la cadena de conexión "AveriasUnphu".
    /// </param>
    public static WebApplication Crear(string[] args, Action<IServiceCollection>? registrarDatos = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (registrarDatos is null)
        {
            builder.Services.AddSingleton<ConexionSql>();
            builder.Services.AddScoped<IRepositorioCatalogos, RepositorioCatalogosSql>();
        }
        else
        {
            registrarDatos(builder.Services);
        }

        builder.Services.AddProblemDetails();

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapGet("/salud", () => Results.Ok(new { estado = "activo" }));
        app.MapApiCatalogos();

        return app;
    }
}
