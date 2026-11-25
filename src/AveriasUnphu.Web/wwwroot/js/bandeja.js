// Bandeja de seguimiento del perfil Mantenimiento (tarea 3.3).
// Lista las solicitudes por prioridad, filtra por estado, aula y equipo, y avanza el estado:
// Pendiente → En proceso → Resuelta.
(() => {
  const filtros = document.getElementById('filtros');
  const filtroEstado = document.getElementById('filtro-estado');
  const filtroAula = document.getElementById('filtro-aula');
  const filtroCategoria = document.getElementById('filtro-categoria');
  const cuerpo = document.getElementById('solicitudes');
  const mensaje = document.getElementById('mensaje');

  // Acción disponible según el estado actual
  const ACCIONES = {
    'Pendiente': { estado: 'En proceso', texto: 'Iniciar reparación' },
    'En proceso': { estado: 'Resuelta', texto: 'Marcar resuelta' },
  };

  function mostrarMensaje(tipo, texto) {
    mensaje.className = `mensaje mensaje-${tipo}`;
    mensaje.textContent = texto;
    mensaje.hidden = false;
  }

  async function cargarFiltros() {
    const [aulas, categorias] = await Promise.all([
      Api.obtener('/api/catalogos/aulas'),
      Api.obtener('/api/catalogos/categorias'),
    ]);
    aulas.forEach((a) => filtroAula.append(new Option(`${a.codigo} · ${a.nombre}`, a.id)));
    categorias.forEach((c) => filtroCategoria.append(new Option(c.nombre, c.id)));
  }

  function consulta() {
    const p = new URLSearchParams();
    if (filtroEstado.value) p.set('estado', filtroEstado.value);
    if (filtroAula.value) p.set('aulaId', filtroAula.value);
    if (filtroCategoria.value) p.set('categoriaId', filtroCategoria.value);
    const q = p.toString();
    return q ? `/api/solicitudes?${q}` : '/api/solicitudes';
  }

  function celda(fila, contenido, etiqueta, clase) {
    const td = document.createElement('td');
    td.dataset.etiqueta = etiqueta;
    if (clase) td.className = clase;
    if (contenido instanceof Node) td.append(contenido); else td.textContent = contenido;
    fila.append(td);
    return td;
  }

  function pintar(solicitudes) {
    cuerpo.innerHTML = '';
    if (!solicitudes.length) {
      const fila = cuerpo.insertRow();
      const td = fila.insertCell();
      td.colSpan = 8;
      td.className = 'vacio';
      td.textContent = 'No hay solicitudes con estos filtros.';
      return;
    }

    solicitudes.forEach((s) => {
      const fila = document.createElement('tr');
      fila.dataset.id = s.id;
      celda(fila, `#${s.id}`, 'N.º', 'numero');
      celda(fila, Formato.etiqueta('prioridad', s.prioridad), 'Prioridad');
      celda(fila, s.aula, 'Aula');
      celda(fila, s.categoria, 'Equipo');

      const desc = document.createElement('div');
      const texto = document.createElement('div');
      texto.textContent = s.descripcion;
      const impacto = document.createElement('small');
      impacto.className = 'impacto';
      impacto.textContent = s.impacto;
      desc.append(texto, impacto);
      celda(fila, desc, 'Descripción', 'descripcion');

      const reportada = document.createElement('div');
      reportada.append(Formato.fecha(s.fechaReporte));
      const por = document.createElement('small');
      por.className = 'impacto';
      por.textContent = s.reportadoPor;
      reportada.append(por);
      celda(fila, reportada, 'Reportada');

      celda(fila, Formato.etiqueta('estado', s.estado), 'Estado');

      const accion = ACCIONES[s.estado];
      if (accion) {
        const boton = document.createElement('button');
        boton.type = 'button';
        boton.className = s.estado === 'Pendiente' ? 'boton-principal boton-pequeno' : 'boton-secundario boton-pequeno';
        boton.textContent = accion.texto;
        boton.addEventListener('click', () => cambiarEstado(s, accion.estado, boton));
        celda(fila, boton, 'Acción', 'accion');
      } else {
        const resuelta = document.createElement('small');
        resuelta.className = 'impacto';
        resuelta.textContent = `Resuelta el ${Formato.fecha(s.fechaResolucion)}`;
        celda(fila, resuelta, 'Acción', 'accion');
      }
      cuerpo.append(fila);
    });
  }

  async function actualizarResumen() {
    const activas = await Api.obtener('/api/solicitudes?estado=activas');
    const contar = (f) => activas.filter(f).length;
    document.getElementById('total-alta').textContent = contar((s) => s.prioridad === 'Alta');
    document.getElementById('total-media').textContent = contar((s) => s.prioridad === 'Media');
    document.getElementById('total-baja').textContent = contar((s) => s.prioridad === 'Baja');
    document.getElementById('total-proceso').textContent = contar((s) => s.estado === 'En proceso');
  }

  async function cargar() {
    try {
      const [lista] = await Promise.all([Api.obtener(consulta()), actualizarResumen()]);
      pintar(lista);
    } catch (error) {
      mostrarMensaje('error', error.message);
    }
  }

  async function cambiarEstado(solicitud, nuevoEstado, boton) {
    boton.disabled = true;
    try {
      const r = await Api.enviar(`/api/solicitudes/${solicitud.id}/estado`, 'PATCH', { estado: nuevoEstado });
      if (r.ok) {
        mostrarMensaje('exito', `Solicitud #${solicitud.id} (${solicitud.aula} · ${solicitud.categoria}) pasó a ${r.datos.estado}.`);
      } else {
        mostrarMensaje('error', r.datos?.errores?.join(' ') || `Error ${r.estado} al cambiar el estado.`);
      }
    } catch (error) {
      mostrarMensaje('error', error.message);
    }
    await cargar();
  }

  [filtroEstado, filtroAula, filtroCategoria].forEach((s) => s.addEventListener('change', cargar));
  filtros.addEventListener('reset', () => setTimeout(cargar));
  filtros.addEventListener('submit', (e) => e.preventDefault());

  cargarFiltros().catch((e) => mostrarMensaje('error', e.message)).finally(cargar);
})();
