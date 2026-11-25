using AveriasUnphu.Pruebas.Apoyo;
using AveriasUnphu.Web.Dominio;
using AveriasUnphu.Web.Servicios;
using static AveriasUnphu.Pruebas.Apoyo.CatalogosEnMemoria;

namespace AveriasUnphu.Pruebas;

/// <summary>
/// Tarea 2.2: validación de duplicados antes de guardar una avería.
/// Casos funcionales P-01 a P-04 y reglas RD-01 a RD-05 (docs/reglas-de-negocio.md).
/// </summary>
public class PruebasRegistroDuplicados
{
    private readonly DatosEnMemoria _datos = new();
    private readonly ServicioSolicitudes _servicio;

    public PruebasRegistroDuplicados() =>
        _servicio = new ServicioSolicitudes(_datos.Solicitudes, _datos.Catalogos);

    private static NuevaSolicitud Reporte(Aula aula, CategoriaEquipo categoria,
        string impacto = Impactos.ImpideLaClase, string descripcion = "El equipo no funciona.") =>
        new(aula.Id, categoria.Id, Docente.Id, descripcion, impacto);

    [Fact]
    public async Task P01_registro_inicial_se_guarda_como_pendiente()
    {
        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        Assert.Equal(TipoResultado.Registrada, resultado.Tipo);
        Assert.Equal(Estados.Pendiente, resultado.Solicitud!.Estado);
        Assert.Equal("A-101 · Aula 101", resultado.Solicitud.Aula);
        Assert.Equal(1, _datos.Solicitudes.Cantidad);
    }

    [Fact]
    public async Task P02_bloquea_el_duplicado_y_devuelve_la_solicitud_vigente()
    {
        var primera = await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        var segunda = await _servicio.RegistrarAsync(Reporte(A101, Proyector, descripcion: "Otra vez el proyector."));

        Assert.Equal(TipoResultado.Duplicada, segunda.Tipo);
        Assert.Equal(primera.Solicitud!.Id, segunda.Vigente!.Id);
        Assert.Equal(1, _datos.Solicitudes.Cantidad);
    }

    [Fact]
    public async Task P02_tambien_bloquea_si_la_vigente_esta_en_proceso()
    {
        var vigente = _datos.Solicitudes.Agregar(A101, Proyector, Estados.EnProceso);

        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        Assert.Equal(TipoResultado.Duplicada, resultado.Tipo);
        Assert.Equal(vigente.Id, resultado.Vigente!.Id);
    }

    [Fact]
    public async Task P03_misma_aula_con_otra_categoria_se_registra()
    {
        await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Aire));

        Assert.Equal(TipoResultado.Registrada, resultado.Tipo);
        Assert.Equal(2, _datos.Solicitudes.Cantidad);
    }

    [Fact]
    public async Task P04_reapertura_si_la_anterior_esta_resuelta_se_registra_una_nueva()
    {
        var anterior = _datos.Solicitudes.Agregar(A101, Proyector, Estados.Resuelta);

        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        Assert.Equal(TipoResultado.Registrada, resultado.Tipo);
        Assert.NotEqual(anterior.Id, resultado.Solicitud!.Id);
        Assert.Equal(Estados.Pendiente, resultado.Solicitud.Estado);
    }

    [Fact]
    public async Task RD03_misma_categoria_en_otra_aula_se_registra()
    {
        await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        var resultado = await _servicio.RegistrarAsync(Reporte(A201, Proyector));

        Assert.Equal(TipoResultado.Registrada, resultado.Tipo);
    }

    [Fact]
    public async Task RD05_envio_simultaneo_lo_detecta_la_base_de_datos_y_responde_duplicado()
    {
        var vigente = _datos.Solicitudes.Agregar(A101, Proyector, Estados.Pendiente);
        _datos.Solicitudes.SimularEnvioSimultaneo = true;

        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector));

        Assert.Equal(TipoResultado.Duplicada, resultado.Tipo);
        Assert.Equal(vigente.Id, resultado.Vigente!.Id);
        Assert.Equal(1, _datos.Solicitudes.Cantidad);
    }

    [Fact]
    public async Task Rechaza_un_reporte_sin_datos_obligatorios()
    {
        var resultado = await _servicio.RegistrarAsync(new NuevaSolicitud(0, 0, 0, "  ", null));

        Assert.Equal(TipoResultado.Invalida, resultado.Tipo);
        Assert.Equal(5, resultado.Errores.Count);
        Assert.Equal(0, _datos.Solicitudes.Cantidad);
    }

    [Fact]
    public async Task Rechaza_una_descripcion_de_mas_de_500_caracteres()
    {
        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector, descripcion: new string('x', 501)));

        Assert.Equal(TipoResultado.Invalida, resultado.Tipo);
        Assert.Contains("500", resultado.Errores[0]);
    }

    [Fact]
    public async Task Solo_el_perfil_Reportante_puede_reportar()
    {
        var resultado = await _servicio.RegistrarAsync(
            new NuevaSolicitud(A101.Id, Proyector.Id, Tecnico.Id, "El proyector no enciende.", Impactos.ImpideLaClase));

        Assert.Equal(TipoResultado.Invalida, resultado.Tipo);
        Assert.Contains("Reportante", resultado.Errores[0]);
    }

    [Fact]
    public async Task Guarda_la_descripcion_sin_espacios_sobrantes_y_con_tildes()
    {
        var resultado = await _servicio.RegistrarAsync(Reporte(A101, Proyector, descripcion: "  La lámpara del proyector está fundida.  "));

        Assert.Equal("La lámpara del proyector está fundida.", resultado.Solicitud!.Descripcion);
    }
}
