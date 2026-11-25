using System.Net.Http.Json;
using AveriasUnphu.Pruebas.Apoyo;
using AveriasUnphu.Web.Dominio;
using AveriasUnphu.Web.Servicios;
using static AveriasUnphu.Pruebas.Apoyo.CatalogosEnMemoria;

namespace AveriasUnphu.Pruebas;

/// <summary>Tarea 3.1: prioridad automática según la criticidad del equipo y el impacto en la clase.</summary>
public class PruebasPrioridad
{
    [Theory]
    [InlineData("Alta", "Impide la clase", "Alta")]
    [InlineData("Alta", "Afecta parcialmente", "Alta")]
    [InlineData("Alta", "No afecta", "Media")]
    [InlineData("Media", "Impide la clase", "Alta")]
    [InlineData("Media", "Afecta parcialmente", "Media")]
    [InlineData("Media", "No afecta", "Baja")]
    [InlineData("Baja", "Impide la clase", "Media")]
    [InlineData("Baja", "Afecta parcialmente", "Baja")]
    [InlineData("Baja", "No afecta", "Baja")]
    public void Aplica_la_matriz_de_prioridad(string criticidad, string impacto, string esperada)
    {
        Assert.Equal(esperada, ReglasPrioridad.Calcular(criticidad, impacto));
    }

    [Fact]
    public void Rechaza_una_combinacion_que_no_existe()
    {
        Assert.Throws<ArgumentException>(() => ReglasPrioridad.Calcular("Urgente", Impactos.ImpideLaClase));
    }

    [Theory]
    [InlineData(1, "Impide la clase", "Alta")]   // Proyector (criticidad Alta)
    [InlineData(2, "Afecta parcialmente", "Media")] // Aire acondicionado (criticidad Media)
    [InlineData(3, "No afecta", "Baja")]          // Pizarra (criticidad Baja)
    public async Task Al_registrar_la_averia_se_guarda_la_prioridad_calculada(int categoriaId, string impacto, string esperada)
    {
        var datos = new DatosEnMemoria();
        var servicio = new ServicioSolicitudes(datos.Solicitudes, datos.Catalogos);

        var resultado = await servicio.RegistrarAsync(
            new NuevaSolicitud(A101.Id, categoriaId, Docente.Id, "Avería de prueba.", impacto));

        Assert.Equal(TipoResultado.Registrada, resultado.Tipo);
        Assert.Equal(esperada, resultado.Solicitud!.Prioridad);
    }

    [Fact]
    public async Task La_api_devuelve_la_prioridad_asignada()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var respuesta = await servidor.Cliente.PostAsJsonAsync("/api/solicitudes", new
        {
            aulaId = LabInf1.Id, categoriaId = Proyector.Id, usuarioId = Docente.Id,
            impacto = Impactos.ImpideLaClase, descripcion = "El proyector del laboratorio no enciende.",
        });
        var solicitud = await respuesta.Content.ReadFromJsonAsync<Solicitud>();

        Assert.Equal(Prioridades.Alta, solicitud!.Prioridad);
    }
}
