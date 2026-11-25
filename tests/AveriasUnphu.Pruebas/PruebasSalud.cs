using System.Net;
using AveriasUnphu.Pruebas.Apoyo;

namespace AveriasUnphu.Pruebas;

public class PruebasSalud
{
    [Fact]
    public async Task El_servidor_responde_que_esta_activo()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var respuesta = await servidor.Cliente.GetAsync("/salud");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("activo", await respuesta.Content.ReadAsStringAsync());
    }
}
