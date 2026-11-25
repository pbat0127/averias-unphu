using AveriasUnphu.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AveriasUnphu.Pruebas.Apoyo;

/// <summary>
/// Levanta la aplicación real en un puerto libre, con datos en memoria en lugar de SQL Server,
/// y entrega un HttpClient para llamar a la API como lo haría el navegador.
/// </summary>
public sealed class ServidorDePrueba : IAsyncDisposable
{
    private readonly WebApplication _app;

    public HttpClient Cliente { get; }
    public DatosEnMemoria Datos { get; }

    private ServidorDePrueba(WebApplication app, DatosEnMemoria datos)
    {
        _app = app;
        Datos = datos;
        Cliente = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    public static async Task<ServidorDePrueba> IniciarAsync(DatosEnMemoria? datos = null)
    {
        datos ??= new DatosEnMemoria();
        var app = Aplicacion.Crear(
            ["--urls=http://127.0.0.1:0", "--environment=Testing", "--Logging:LogLevel:Default=Warning", "--Logging:LogLevel:Microsoft.AspNetCore.StaticFiles=Error"],
            datos.Registrar);
        await app.StartAsync();
        return new ServidorDePrueba(app, datos);
    }

    public async ValueTask DisposeAsync()
    {
        Cliente.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
