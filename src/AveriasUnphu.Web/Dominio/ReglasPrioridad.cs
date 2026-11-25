using static AveriasUnphu.Web.Dominio.Impactos;
using static AveriasUnphu.Web.Dominio.Prioridades;

namespace AveriasUnphu.Web.Dominio;

/// <summary>
/// Asigna la prioridad automáticamente (tarea 3.1) cruzando dos datos:
///   - la criticidad de la categoría de equipo (qué tan necesario es para dar la clase), y
///   - el impacto en la clase que indica quien reporta.
///
///                         Impide la clase   Afecta parcialmente   No afecta
///   Criticidad Alta           Alta               Alta              Media
///   Criticidad Media          Alta               Media             Baja
///   Criticidad Baja           Media              Baja              Baja
/// </summary>
public static class ReglasPrioridad
{
    private static readonly Dictionary<(string Criticidad, string Impacto), string> Matriz = new()
    {
        [(Alta, ImpideLaClase)] = Alta,
        [(Alta, AfectaParcialmente)] = Alta,
        [(Alta, NoAfecta)] = Media,

        [(Media, ImpideLaClase)] = Alta,
        [(Media, AfectaParcialmente)] = Media,
        [(Media, NoAfecta)] = Baja,

        [(Baja, ImpideLaClase)] = Media,
        [(Baja, AfectaParcialmente)] = Baja,
        [(Baja, NoAfecta)] = Baja,
    };

    public static string Calcular(string criticidad, string impacto) =>
        Matriz.TryGetValue((criticidad, impacto), out var prioridad)
            ? prioridad
            : throw new ArgumentException($"Combinación no válida: criticidad \"{criticidad}\", impacto \"{impacto}\".");
}
