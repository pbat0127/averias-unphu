using System.Net;
using System.Net.Http.Json;
using AveriasUnphu.Pruebas.Apoyo;
using AveriasUnphu.Web.Api;
using AveriasUnphu.Web.Dominio;
using static AveriasUnphu.Pruebas.Apoyo.CatalogosEnMemoria;

namespace AveriasUnphu.Pruebas;

/// <summary>Tarea 2.2: el endpoint valida duplicados antes de guardar (lo que recibe el navegador).</summary>
public class PruebasApiSolicitudes
{
    private static object Reporte(Aula aula, CategoriaEquipo categoria) => new
    {
        aulaId = aula.Id,
        categoriaId = categoria.Id,
        usuarioId = Docente.Id,
        impacto = Impactos.ImpideLaClase,
        descripcion = "El proyector no enciende.",
    };

    [Fact]
    public async Task Post_registra_la_averia_y_responde_201()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var respuesta = await servidor.Cliente.PostAsJsonAsync("/api/solicitudes", Reporte(A101, Proyector));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var solicitud = await respuesta.Content.ReadFromJsonAsync<Solicitud>();
        Assert.Equal(Estados.Pendiente, solicitud!.Estado);
        Assert.Equal($"/api/solicitudes/{solicitud.Id}", respuesta.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Post_duplicado_responde_409_con_la_solicitud_vigente()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var primera = await servidor.Cliente.PostAsJsonAsync("/api/solicitudes", Reporte(A101, Proyector));
        var vigente = await primera.Content.ReadFromJsonAsync<Solicitud>();

        var respuesta = await servidor.Cliente.PostAsJsonAsync("/api/solicitudes", Reporte(A101, Proyector));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaDuplicado>();
        Assert.Equal(vigente!.Id, cuerpo!.Vigente.Id);
        Assert.Equal("Proyector", cuerpo.Vigente.Categoria);
    }

    [Fact]
    public async Task Post_sin_datos_responde_400_con_los_errores()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var respuesta = await servidor.Cliente.PostAsJsonAsync("/api/solicitudes", new { aulaId = A101.Id });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaErrores>();
        Assert.Contains("Describa la avería.", cuerpo!.Errores);
    }

    [Fact]
    public async Task Validar_indica_si_ya_hay_una_solicitud_activa()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var vigente = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);

        var duplicada = await servidor.Cliente.GetFromJsonAsync<RespuestaValidacion>(
            $"/api/solicitudes/validar?aulaId={A101.Id}&categoriaId={Proyector.Id}");
        var libre = await servidor.Cliente.GetFromJsonAsync<RespuestaValidacion>(
            $"/api/solicitudes/validar?aulaId={A101.Id}&categoriaId={Aire.Id}");

        Assert.True(duplicada!.Duplicada);
        Assert.Equal(vigente.Id, duplicada.Vigente!.Id);
        Assert.False(libre!.Duplicada);
        Assert.Null(libre.Vigente);
    }

    [Fact]
    public async Task Get_por_numero_devuelve_la_solicitud_o_404()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();
        var s = servidor.Datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);

        var encontrada = await servidor.Cliente.GetFromJsonAsync<Solicitud>($"/api/solicitudes/{s.Id}");
        var noExiste = await servidor.Cliente.GetAsync("/api/solicitudes/999");

        Assert.Equal(s.Id, encontrada!.Id);
        Assert.Equal(HttpStatusCode.NotFound, noExiste.StatusCode);
    }
}
