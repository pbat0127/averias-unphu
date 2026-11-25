// Alerta y vista de bloqueo ante un reporte duplicado (tarea 2.4).
// Muestra los datos de la solicitud vigente para que el usuario sepa que la avería ya está reportada.
//
//   VistaBloqueo.alertar(vigente)   → aviso sobre el formulario: se puede cambiar el aula o la categoría.
//   VistaBloqueo.bloquear(vigente)  → oculta el formulario y muestra la solicitud vigente.
//   VistaBloqueo.ocultar()          → quita el aviso o el bloqueo.
//
// "vigente" es la solicitud activa que devuelve la API:
//   { id, aula, categoria, estado, prioridad, fechaReporte, reportadoPor, descripcion }
const VistaBloqueo = (() => {
  const panel = document.getElementById('bloqueo');

  function fila(lista, titulo, contenido) {
    const dt = document.createElement('dt');
    dt.textContent = titulo;
    const dd = document.createElement('dd');
    if (contenido instanceof Node) dd.append(contenido); else dd.textContent = contenido;
    lista.append(dt, dd);
  }

  function pintar(vigente, modo) {
    panel.innerHTML = '';

    const titulo = document.createElement('h2');
    titulo.textContent = modo === 'bloqueo'
      ? 'Esta avería ya está reportada'
      : 'Atención: ya hay una solicitud activa para esta aula y categoría';

    const texto = document.createElement('p');
    texto.textContent = modo === 'bloqueo'
      ? `Ya existe la solicitud #${vigente.id} para ${vigente.aula} · ${vigente.categoria}, y todavía no se ha resuelto. `
        + 'No hace falta reportarla otra vez: el equipo de mantenimiento ya la tiene en su bandeja.'
      : `La solicitud #${vigente.id} para ${vigente.aula} · ${vigente.categoria} sigue abierta, así que no se puede `
        + 'registrar otra. Si la avería es de otro equipo, cambie el aula o la categoría.';

    const datos = document.createElement('dl');
    datos.className = 'datos-solicitud';
    fila(datos, 'Solicitud', `#${vigente.id}`);
    fila(datos, 'Estado', Formato.etiqueta('estado', vigente.estado));
    fila(datos, 'Prioridad', Formato.etiqueta('prioridad', vigente.prioridad));
    fila(datos, 'Reportada', `${Formato.fecha(vigente.fechaReporte)} por ${vigente.reportadoPor}`);
    fila(datos, 'Descripción', vigente.descripcion);

    panel.append(titulo, texto, datos);

    if (modo === 'bloqueo') {
      const acciones = document.createElement('div');
      acciones.className = 'acciones';
      const otra = document.createElement('button');
      otra.type = 'button';
      otra.className = 'boton-principal';
      otra.id = 'boton-otra-averia';
      otra.textContent = 'Reportar otra avería';
      otra.addEventListener('click', () => panel.dispatchEvent(new CustomEvent('otra-averia')));
      acciones.append(otra);
      panel.append(acciones);
    }

    panel.dataset.modo = modo;
    panel.hidden = false;
  }

  return {
    alertar: (vigente) => pintar(vigente, 'alerta'),
    bloquear: (vigente) => pintar(vigente, 'bloqueo'),
    ocultar: () => { panel.hidden = true; panel.innerHTML = ''; delete panel.dataset.modo; },
    alPedirOtraAveria: (accion) => panel.addEventListener('otra-averia', accion),
  };
})();
