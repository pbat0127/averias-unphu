namespace AveriasUnphu.Web.Dominio;

/// <summary>Revisa que el reporte tenga todos los datos obligatorios.</summary>
public static class ValidadorSolicitud
{
    public const int MaximoDescripcion = 500;

    public static List<string> Validar(NuevaSolicitud datos)
    {
        var errores = new List<string>();
        var descripcion = datos.Descripcion?.Trim() ?? "";

        if (datos.UsuarioId <= 0) errores.Add("Seleccione quién reporta la avería.");
        if (datos.AulaId <= 0) errores.Add("Seleccione el aula o laboratorio.");
        if (datos.CategoriaId <= 0) errores.Add("Seleccione la categoría del equipo.");
        if (datos.Impacto is null || !Impactos.Todos.Contains(datos.Impacto))
            errores.Add("Indique el impacto en la clase.");
        if (descripcion.Length == 0)
            errores.Add("Describa la avería.");
        else if (descripcion.Length > MaximoDescripcion)
            errores.Add($"La descripción no puede pasar de {MaximoDescripcion} caracteres.");

        return errores;
    }
}
