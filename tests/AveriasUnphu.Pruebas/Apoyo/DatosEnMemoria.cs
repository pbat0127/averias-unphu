using AveriasUnphu.Web.Datos;
using AveriasUnphu.Web.Dominio;
using Microsoft.Extensions.DependencyInjection;

namespace AveriasUnphu.Pruebas.Apoyo;

/// <summary>
/// Reemplaza a SQL Server en las pruebas. Trae los mismos catálogos que db/02_catalogos.sql
/// (resumidos) para que las pruebas no dependan de tener una base de datos instalada.
/// </summary>
public sealed class DatosEnMemoria
{
    public CatalogosEnMemoria Catalogos { get; } = new();

    public void Registrar(IServiceCollection servicios)
    {
        servicios.AddSingleton<IRepositorioCatalogos>(Catalogos);
    }
}

public sealed class CatalogosEnMemoria : IRepositorioCatalogos
{
    public static readonly Aula A101 = new(1, "A-101", "Aula 101", "Edificio A", "Aula");
    public static readonly Aula A201 = new(2, "A-201", "Aula 201", "Edificio A", "Aula");
    public static readonly Aula LabInf1 = new(3, "LAB-INF-1", "Laboratorio de Informática 1", "Edificio C", "Laboratorio");

    public static readonly CategoriaEquipo Proyector = new(1, "Proyector", "Alta");
    public static readonly CategoriaEquipo Aire = new(2, "Aire acondicionado", "Media");
    public static readonly CategoriaEquipo Pizarra = new(3, "Pizarra", "Baja");

    public static readonly Usuario Docente = new(1, "Docente de prueba", Perfiles.Reportante);
    public static readonly Usuario Tecnico = new(2, "Técnico de mantenimiento", Perfiles.Mantenimiento);

    private readonly List<Aula> _aulas = [A101, A201, LabInf1];
    private readonly List<CategoriaEquipo> _categorias = [Proyector, Aire, Pizarra];
    private readonly List<Usuario> _usuarios = [Docente, Tecnico];

    public Task<IReadOnlyList<Aula>> ListarAulasAsync() => Task.FromResult<IReadOnlyList<Aula>>(_aulas);
    public Task<IReadOnlyList<CategoriaEquipo>> ListarCategoriasAsync() => Task.FromResult<IReadOnlyList<CategoriaEquipo>>(_categorias);

    public Task<IReadOnlyList<Usuario>> ListarUsuariosAsync(string? perfil) =>
        Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.Where(u => perfil is null || u.Perfil == perfil).ToList());

    public Task<Aula?> ObtenerAulaAsync(int id) => Task.FromResult(_aulas.FirstOrDefault(a => a.Id == id));
    public Task<CategoriaEquipo?> ObtenerCategoriaAsync(int id) => Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id));
    public Task<Usuario?> ObtenerUsuarioAsync(int id) => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));
}
