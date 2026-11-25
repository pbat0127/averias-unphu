using System.Data.Common;
using AveriasUnphu.Web.Dominio;

namespace AveriasUnphu.Web.Datos;

/// <summary>Lectura de los catálogos: aulas, categorías de equipo y usuarios.</summary>
public interface IRepositorioCatalogos
{
    Task<IReadOnlyList<Aula>> ListarAulasAsync();
    Task<IReadOnlyList<CategoriaEquipo>> ListarCategoriasAsync();
    Task<IReadOnlyList<Usuario>> ListarUsuariosAsync(string? perfil);
    Task<Aula?> ObtenerAulaAsync(int id);
    Task<CategoriaEquipo?> ObtenerCategoriaAsync(int id);
    Task<Usuario?> ObtenerUsuarioAsync(int id);
}

public sealed class RepositorioCatalogosSql(ConexionSql conexionSql) : IRepositorioCatalogos
{
    private const string SqlAulas =
        "SELECT id, codigo, nombre, edificio, tipo FROM dbo.aulas WHERE activo = 1";
    private const string SqlCategorias =
        "SELECT id, nombre, criticidad FROM dbo.categorias_equipo WHERE activo = 1";
    private const string SqlUsuarios =
        "SELECT id, nombre, perfil FROM dbo.usuarios WHERE activo = 1";

    public async Task<IReadOnlyList<Aula>> ListarAulasAsync()
    {
        await using var conexion = await conexionSql.AbrirAsync();
        return await ComandoSql.Crear(conexion, SqlAulas + " ORDER BY tipo, codigo").ListarAsync(MapearAula);
    }

    public async Task<IReadOnlyList<CategoriaEquipo>> ListarCategoriasAsync()
    {
        await using var conexion = await conexionSql.AbrirAsync();
        return await ComandoSql.Crear(conexion, SqlCategorias + " ORDER BY nombre").ListarAsync(MapearCategoria);
    }

    public async Task<IReadOnlyList<Usuario>> ListarUsuariosAsync(string? perfil)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        return await ComandoSql
            .Crear(conexion, SqlUsuarios + " AND (@perfil IS NULL OR perfil = @perfil) ORDER BY nombre")
            .Con("@perfil", perfil)
            .ListarAsync(MapearUsuario);
    }

    public async Task<Aula?> ObtenerAulaAsync(int id)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var lista = await ComandoSql.Crear(conexion, SqlAulas + " AND id = @id").Con("@id", id).ListarAsync(MapearAula);
        return lista.SingleOrDefault();
    }

    public async Task<CategoriaEquipo?> ObtenerCategoriaAsync(int id)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var lista = await ComandoSql.Crear(conexion, SqlCategorias + " AND id = @id").Con("@id", id).ListarAsync(MapearCategoria);
        return lista.SingleOrDefault();
    }

    public async Task<Usuario?> ObtenerUsuarioAsync(int id)
    {
        await using var conexion = await conexionSql.AbrirAsync();
        var lista = await ComandoSql.Crear(conexion, SqlUsuarios + " AND id = @id").Con("@id", id).ListarAsync(MapearUsuario);
        return lista.SingleOrDefault();
    }

    private static Aula MapearAula(DbDataReader l) =>
        new(l.Entero("id"), l.Texto("codigo"), l.Texto("nombre"), l.Texto("edificio"), l.Texto("tipo"));

    private static CategoriaEquipo MapearCategoria(DbDataReader l) =>
        new(l.Entero("id"), l.Texto("nombre"), l.Texto("criticidad"));

    private static Usuario MapearUsuario(DbDataReader l) =>
        new(l.Entero("id"), l.Texto("nombre"), l.Texto("perfil"));
}
