using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace AveriasUnphu.Web.Datos;

/// <summary>
/// Abre conexiones a SQL Server con la cadena "AveriasUnphu" de appsettings.json.
/// </summary>
public sealed class ConexionSql(IConfiguration configuracion)
{
    private readonly string _cadena = configuracion.GetConnectionString("AveriasUnphu")
        ?? throw new InvalidOperationException(
            "Falta la cadena de conexión 'AveriasUnphu' en appsettings.json.");

    public async Task<DbConnection> AbrirAsync()
    {
        var conexion = new SqlConnection(_cadena);
        await conexion.OpenAsync();
        return conexion;
    }
}

/// <summary>Atajos para armar comandos con parámetros (nunca concatenar valores en el SQL).</summary>
internal static class ComandoSql
{
    public static DbCommand Crear(DbConnection conexion, string sql)
    {
        var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return comando;
    }

    public static DbCommand Con(this DbCommand comando, string nombre, object? valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nombre;
        parametro.Value = valor ?? DBNull.Value;
        comando.Parameters.Add(parametro);
        return comando;
    }

    public static async Task<List<T>> ListarAsync<T>(this DbCommand comando, Func<DbDataReader, T> mapear)
    {
        var lista = new List<T>();
        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            lista.Add(mapear(lector));
        return lista;
    }

    public static string Texto(this DbDataReader lector, string columna) =>
        lector.GetString(lector.GetOrdinal(columna));

    public static int Entero(this DbDataReader lector, string columna) =>
        lector.GetInt32(lector.GetOrdinal(columna));

    public static DateTime Fecha(this DbDataReader lector, string columna) =>
        lector.GetDateTime(lector.GetOrdinal(columna));

    public static DateTime? FechaOpcional(this DbDataReader lector, string columna)
    {
        var i = lector.GetOrdinal(columna);
        return lector.IsDBNull(i) ? null : lector.GetDateTime(i);
    }
}
