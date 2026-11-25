namespace AveriasUnphu.Web.Dominio;

/// <summary>Estados de una solicitud. Pendiente y En proceso son estados activos.</summary>
public static class Estados
{
    public const string Pendiente = "Pendiente";
    public const string EnProceso = "En proceso";
    public const string Resuelta = "Resuelta";

    public static readonly IReadOnlyList<string> Todos = [Pendiente, EnProceso, Resuelta];
    public static readonly IReadOnlyList<string> Activos = [Pendiente, EnProceso];

    public static bool EsActivo(string estado) => Activos.Contains(estado);

    /// <summary>Siguiente estado permitido: Pendiente → En proceso → Resuelta. Null si ya está Resuelta.</summary>
    public static string? Siguiente(string estado) => estado switch
    {
        Pendiente => EnProceso,
        EnProceso => Resuelta,
        _ => null,
    };
}

/// <summary>Cómo afecta la avería a la clase.</summary>
public static class Impactos
{
    public const string ImpideLaClase = "Impide la clase";
    public const string AfectaParcialmente = "Afecta parcialmente";
    public const string NoAfecta = "No afecta";

    public static readonly IReadOnlyList<string> Todos = [ImpideLaClase, AfectaParcialmente, NoAfecta];
}

public static class Prioridades
{
    public const string Alta = "Alta";
    public const string Media = "Media";
    public const string Baja = "Baja";

    public static readonly IReadOnlyList<string> Todas = [Alta, Media, Baja];
}

/// <summary>Solicitud guardada, con los nombres de aula, categoría y usuario.</summary>
public sealed record Solicitud(
    int Id,
    int AulaId,
    string Aula,
    int CategoriaId,
    string Categoria,
    int UsuarioId,
    string ReportadoPor,
    string Descripcion,
    string Impacto,
    string Prioridad,
    string Estado,
    DateTime FechaReporte,
    DateTime? FechaActualizacion,
    DateTime? FechaResolucion);

/// <summary>Filtros de la bandeja. Estado puede ser un estado, "activas" (Pendiente y En proceso) o null (todas).</summary>
public sealed record FiltroSolicitudes(string? Estado = null, int? AulaId = null, int? CategoriaId = null)
{
    public const string SoloActivas = "activas";
}
