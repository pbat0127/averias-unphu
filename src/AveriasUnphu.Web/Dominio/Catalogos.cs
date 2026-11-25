namespace AveriasUnphu.Web.Dominio;

/// <summary>Aula o laboratorio del campus.</summary>
public sealed record Aula(int Id, string Codigo, string Nombre, string Edificio, string Tipo);

/// <summary>Categoría de equipo. La criticidad (Alta, Media o Baja) se usa para calcular la prioridad.</summary>
public sealed record CategoriaEquipo(int Id, string Nombre, string Criticidad);

/// <summary>Usuario del sistema con su perfil.</summary>
public sealed record Usuario(int Id, string Nombre, string Perfil);

public static class Perfiles
{
    public const string Reportante = "Reportante";
    public const string Mantenimiento = "Mantenimiento";
}
