using AveriasUnphu.Web.Datos;
using AveriasUnphu.Web.Dominio;

namespace AveriasUnphu.Web.Servicios;

public enum TipoResultado { Registrada, Duplicada, Invalida }

/// <summary>Resultado de intentar registrar una avería.</summary>
public sealed record ResultadoRegistro(
    TipoResultado Tipo, Solicitud? Solicitud, Solicitud? Vigente, IReadOnlyList<string> Errores)
{
    public static ResultadoRegistro Registrada(Solicitud s) => new(TipoResultado.Registrada, s, null, []);
    public static ResultadoRegistro Duplicada(Solicitud vigente) => new(TipoResultado.Duplicada, null, vigente, []);
    public static ResultadoRegistro Invalida(IReadOnlyList<string> errores) => new(TipoResultado.Invalida, null, null, errores);
}

/// <summary>
/// Reglas para registrar averías. Antes de guardar, valida los datos y aplica las reglas de duplicidad
/// (docs/reglas-de-negocio.md): si ya hay una solicitud activa para la misma aula y categoría, no se guarda.
/// </summary>
public sealed class ServicioSolicitudes(IRepositorioSolicitudes solicitudes, IRepositorioCatalogos catalogos)
{
    /// <summary>Solicitud activa para el aula y la categoría, o null. La usa el formulario para avisar antes de enviar.</summary>
    public Task<Solicitud?> BuscarActivaAsync(int aulaId, int categoriaId) =>
        solicitudes.BuscarActivaAsync(aulaId, categoriaId);

    public Task<Solicitud?> ObtenerAsync(int id) => solicitudes.ObtenerAsync(id);

    public async Task<ResultadoRegistro> RegistrarAsync(NuevaSolicitud datos)
    {
        // 1. Datos obligatorios
        var errores = ValidadorSolicitud.Validar(datos);
        if (errores.Count > 0) return ResultadoRegistro.Invalida(errores);

        // 2. Que el aula, la categoría y el usuario existan
        var aula = await catalogos.ObtenerAulaAsync(datos.AulaId);
        var categoria = await catalogos.ObtenerCategoriaAsync(datos.CategoriaId);
        var usuario = await catalogos.ObtenerUsuarioAsync(datos.UsuarioId);

        if (aula is null) errores.Add("El aula seleccionada no existe.");
        if (categoria is null) errores.Add("La categoría seleccionada no existe.");
        if (usuario is null) errores.Add("El usuario seleccionado no existe.");
        else if (usuario.Perfil != Perfiles.Reportante)
            errores.Add("Solo los usuarios con perfil Reportante pueden reportar averías.");
        if (errores.Count > 0) return ResultadoRegistro.Invalida(errores);

        // 3. Regla RD-01: si ya hay una solicitud activa para la misma aula y categoría, es duplicado.
        //    RD-02, RD-03 y RD-04 (otra categoría, otra aula o la anterior ya Resuelta) no encuentran nada aquí.
        var vigente = await solicitudes.BuscarActivaAsync(aula!.Id, categoria!.Id);
        if (vigente is not null) return ResultadoRegistro.Duplicada(vigente);

        // 4. Guardar
        var nueva = new SolicitudParaGuardar(
            aula.Id, categoria.Id, usuario!.Id,
            datos.Descripcion!.Trim(), datos.Impacto!,
            AsignarPrioridad(categoria, datos.Impacto!));

        int id;
        try
        {
            id = await solicitudes.InsertarAsync(nueva);
        }
        catch (SolicitudDuplicadaException)
        {
            // Regla RD-05: otra persona registró la misma avería justo antes. Se responde como duplicado.
            vigente = await solicitudes.BuscarActivaAsync(aula.Id, categoria.Id);
            if (vigente is null) throw;
            return ResultadoRegistro.Duplicada(vigente);
        }

        var guardada = await solicitudes.ObtenerAsync(id)
            ?? throw new InvalidOperationException($"No se encontró la solicitud {id} recién guardada.");
        return ResultadoRegistro.Registrada(guardada);
    }

    // La prioridad automática (Alta, Media o Baja) se implementa en la tarea 3.1.
    private static string AsignarPrioridad(CategoriaEquipo categoria, string impacto) => Prioridades.Media;
}
