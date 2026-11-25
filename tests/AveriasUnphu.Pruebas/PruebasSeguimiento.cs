using System.Net;
using System.Net.Http.Json;
using AveriasUnphu.Pruebas.Apoyo;
using AveriasUnphu.Web.Api;
using AveriasUnphu.Web.Dominio;
using static AveriasUnphu.Pruebas.Apoyo.CatalogosEnMemoria;

namespace AveriasUnphu.Pruebas;

/// <summary>Tarea 3.2: listar solicitudes por prioridad y actualizar su estado.</summary>
public class PruebasSeguimiento
{
    private static Task<HttpResponseMessage> CambiarEstado(ServidorDePrueba servidor, int id, string estado) =>
        servidor.Cliente.PatchAsJsonAsync($"/api/solicitudes/{id}/estado", new { estado });

    [Fact]
    public async Task Lista_por_prioridad_y_dentro_de_cada_una_la_mas_antigua_primero()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var datos = servidor.Datos.Solicitudes;
        var baja = datos.Agregar(A101, Pizarra, Estados.Pendiente, Prioridades.Baja, fecha: DateTime.Now.AddHours(-10));
        var mediaNueva = datos.Agregar(A101, Aire, Estados.Pendiente, Prioridades.Media, fecha: DateTime.Now.AddHours(-1));
        var alta = datos.Agregar(A101, Proyector, Estados.Pendiente, Prioridades.Alta, fecha: DateTime.Now.AddHours(-2));
        var mediaVieja = datos.Agregar(A201, Aire, Estados.EnProceso, Prioridades.Media, fecha: DateTime.Now.AddHours(-5));

        var lista = await servidor.Cliente.GetFromJsonAsync<List<Solicitud>>("/api/solicitudes");

        Assert.Equal(new[] { alta.Id, mediaVieja.Id, mediaNueva.Id, baja.Id }, lista!.Select(s => s.Id));
    }

    [Fact]
    public async Task Filtra_por_activas_estado_aula_y_categoria()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var datos = servidor.Datos.Solicitudes;
        var pendiente = datos.Agregar(A101, Proyector, Estados.Pendiente);
        var enProceso = datos.Agregar(A201, Aire, Estados.EnProceso);
        var resuelta = datos.Agregar(LabInf1, Pizarra, Estados.Resuelta);

        async Task<IEnumerable<int>> Ids(string consulta) =>
            (await servidor.Cliente.GetFromJsonAsync<List<Solicitud>>("/api/solicitudes" + consulta))!.Select(s => s.Id).Order();

        Assert.Equal(new[] { pendiente.Id, enProceso.Id }, await Ids("?estado=activas"));
        Assert.Equal(new[] { resuelta.Id }, await Ids("?estado=Resuelta"));
        Assert.Equal(new[] { enProceso.Id }, await Ids($"?aulaId={A201.Id}"));
        Assert.Equal(new[] { pendiente.Id }, await Ids($"?categoriaId={Proyector.Id}"));
        Assert.Equal(new[] { pendiente.Id, enProceso.Id, resuelta.Id }, await Ids(""));
    }

    [Fact]
    public async Task Rechaza_un_filtro_de_estado_desconocido()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var respuesta = await servidor.Cliente.GetAsync("/api/solicitudes?estado=Cerrada");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Avanza_de_Pendiente_a_En_proceso_y_a_Resuelta()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var s = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);

        var r1 = await CambiarEstado(servidor, s.Id, Estados.EnProceso);
        var enProceso = await r1.Content.ReadFromJsonAsync<Solicitud>();
        var r2 = await CambiarEstado(servidor, s.Id, Estados.Resuelta);
        var resuelta = await r2.Content.ReadFromJsonAsync<Solicitud>();

        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.Equal(Estados.EnProceso, enProceso!.Estado);
        Assert.NotNull(enProceso.FechaActualizacion);
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        Assert.Equal(Estados.Resuelta, resuelta!.Estado);
        Assert.NotNull(resuelta.FechaResolucion);
    }

    [Fact]
    public async Task No_permite_saltar_de_Pendiente_a_Resuelta()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var s = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);

        var respuesta = await CambiarEstado(servidor, s.Id, Estados.Resuelta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaErrores>();
        Assert.Contains("En proceso", cuerpo!.Errores[0]);
    }

    [Fact]
    public async Task No_permite_cambiar_una_solicitud_resuelta()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var s = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Resuelta);

        var respuesta = await CambiarEstado(servidor, s.Id, Estados.Pendiente);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Responde_404_si_la_solicitud_no_existe_y_400_si_el_estado_no_existe()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var s = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);

        Assert.Equal(HttpStatusCode.NotFound, (await CambiarEstado(servidor, 999, Estados.EnProceso)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await CambiarEstado(servidor, s.Id, "Cerrada")).StatusCode);
    }
}
