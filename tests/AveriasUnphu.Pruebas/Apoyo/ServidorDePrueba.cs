using AveriasUnphu.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AveriasUnphu.Pruebas.Apoyo;

/// <summary>
/// Levanta la aplicación real en un puerto libre y entrega un HttpClient
/// para llamar a la API como lo haría el navegador.
/// </summary>
public sealed class ServidorDePrueba : IAsyncDisposable
{
    private readonly WebApplication _app;

    public HttpClient Cliente { get; }

    private ServidorDePrueba(WebApplication app)
    {
        _app = app;
        Cliente = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    public static async Task<ServidorDePrueba> IniciarAsync()
    {
        var app = Aplicacion.Crear(
            ["--urls=http://127.0.0.1:0", "--environment=Testing", "--Logging:LogLevel:Default=Warning"],
            _ => { });
        await app.StartAsync();
        return new ServidorDePrueba(app);
    }

    public async ValueTask DisposeAsync()
    {
        Cliente.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
