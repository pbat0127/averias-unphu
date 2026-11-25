using System.Net.Http.Json;
using AveriasUnphu.Pruebas.Apoyo;
using AveriasUnphu.Web.Dominio;

namespace AveriasUnphu.Pruebas;

/// <summary>Tarea 1.2: la API entrega los catálogos que carga el formulario.</summary>
public class PruebasCatalogos
{
    [Fact]
    public async Task Lista_las_aulas_y_laboratorios()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var aulas = await servidor.Cliente.GetFromJsonAsync<List<Aula>>("/api/catalogos/aulas");

        Assert.NotNull(aulas);
        Assert.Contains("A-101", aulas!.Select(a => a.Codigo));
        Assert.Contains("Laboratorio", aulas.Select(a => a.Tipo));
    }

    [Fact]
    public async Task Lista_las_categorias_con_su_criticidad()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var categorias = await servidor.Cliente.GetFromJsonAsync<List<CategoriaEquipo>>("/api/catalogos/categorias");

        var proyector = Assert.Single(categorias!.Where(c => c.Nombre == "Proyector"));
        Assert.Equal("Alta", proyector.Criticidad);
    }

    [Fact]
    public async Task Filtra_los_usuarios_por_perfil()
    {
        await using var servidor = await ServidorDePrueba.IniciarAsync();

        var reportantes = await servidor.Cliente.GetFromJsonAsync<List<Usuario>>("/api/catalogos/usuarios?perfil=Reportante");

        Assert.NotEmpty(reportantes!);
        Assert.All(reportantes!, u => Assert.Equal(Perfiles.Reportante, u.Perfil));
    }
}
