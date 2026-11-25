// Formulario de reporte de averías.
// Tarea 2.3: carga los catálogos y valida los campos.
// Tarea 2.5: se conecta con la validación de duplicados del backend:
//   - al elegir aula y categoría, avisa si ya hay una solicitud activa (y desactiva el envío);
//   - al enviar, registra la avería o muestra la vista de bloqueo con la solicitud vigente.
(() => {
  const formulario = document.getElementById('formulario-reporte');
  const selectUsuario = document.getElementById('usuario');
  const selectAula = document.getElementById('aula');
  const selectCategoria = document.getElementById('categoria');
  const descripcion = document.getElementById('descripcion');
  const contador = document.getElementById('contador');
  const mensaje = document.getElementById('mensaje');
  const botonEnviar = document.getElementById('boton-enviar');
  let consultaActual = 0;

  function mostrarMensaje(tipo, titulo, texto) {
    mensaje.className = `mensaje mensaje-${tipo}`;
    mensaje.innerHTML = '';
    const h2 = document.createElement('h2');
    h2.textContent = titulo;
    const p = document.createElement('p');
    p.textContent = texto;
    mensaje.append(h2, p);
    mensaje.hidden = false;
  }

  function ocultarMensaje() {
    mensaje.hidden = true;
  }

  function llenarSelect(select, elementos, textoVacio, crearOpcion) {
    select.innerHTML = '';
    select.append(new Option(textoVacio, ''));
    elementos.forEach((e) => select.append(crearOpcion(e)));
  }

  async function cargarCatalogos() {
    try {
      const [usuarios, aulas, categorias] = await Promise.all([
        Api.obtener('/api/catalogos/usuarios?perfil=Reportante'),
        Api.obtener('/api/catalogos/aulas'),
        Api.obtener('/api/catalogos/categorias'),
      ]);

      llenarSelect(selectUsuario, usuarios, 'Seleccione…', (u) => new Option(u.nombre, u.id));

      // Aulas y laboratorios agrupados por tipo
      selectAula.innerHTML = '';
      selectAula.append(new Option('Seleccione…', ''));
      ['Aula', 'Laboratorio'].forEach((tipo) => {
        const grupo = document.createElement('optgroup');
        grupo.label = tipo === 'Aula' ? 'Aulas' : 'Laboratorios';
        aulas.filter((a) => a.tipo === tipo)
          .forEach((a) => grupo.append(new Option(`${a.codigo} · ${a.nombre}`, a.id)));
        if (grupo.children.length) selectAula.append(grupo);
      });

      llenarSelect(selectCategoria, categorias, 'Seleccione…', (c) => new Option(c.nombre, c.id));
    } catch (error) {
      mostrarMensaje('error', 'No se pudieron cargar los catálogos', error.message);
    }
  }

  function leerDatos() {
    const impacto = formulario.querySelector('input[name="impacto"]:checked');
    return {
      usuarioId: Number(selectUsuario.value) || 0,
      aulaId: Number(selectAula.value) || 0,
      categoriaId: Number(selectCategoria.value) || 0,
      impacto: impacto ? impacto.value : '',
      descripcion: descripcion.value.trim(),
    };
  }

  function marcar(idCampo, invalido) {
    document.getElementById(idCampo).classList.toggle('invalido', invalido);
    return !invalido;
  }

  function validar(datos) {
    const resultados = [
      marcar('campo-usuario', !datos.usuarioId),
      marcar('campo-aula', !datos.aulaId),
      marcar('campo-categoria', !datos.categoriaId),
      marcar('campo-impacto', !datos.impacto),
      marcar('campo-descripcion', !datos.descripcion),
    ];
    return resultados.every(Boolean);
  }

  function permitirEnvio(permitido) {
    botonEnviar.disabled = !permitido;
  }

  // Pregunta al backend si ya hay una solicitud activa para el aula y la categoría elegidas.
  async function revisarDuplicado() {
    const aulaId = Number(selectAula.value);
    const categoriaId = Number(selectCategoria.value);
    const consulta = ++consultaActual;
    VistaBloqueo.ocultar();
    permitirEnvio(true);
    if (!aulaId || !categoriaId) return;

    try {
      const r = await Api.obtener(`/api/solicitudes/validar?aulaId=${aulaId}&categoriaId=${categoriaId}`);
      if (consulta !== consultaActual) return; // el usuario ya cambió la selección
      if (r.duplicada) {
        VistaBloqueo.alertar(r.vigente);
        permitirEnvio(false);
      }
    } catch {
      // Si la consulta falla, el backend vuelve a validar al enviar.
    }
  }

  async function enviarReporte(datos) {
    permitirEnvio(false);
    try {
      const r = await Api.enviar('/api/solicitudes', 'POST', datos);
      if (r.estado === 201) {
        const s = r.datos;
        formulario.reset();
        mostrarMensaje('exito', `Solicitud #${s.id} registrada`,
          `${s.aula} · ${s.categoria}. Estado: ${s.estado}. Prioridad asignada: ${s.prioridad}.`);
      } else if (r.estado === 409) {
        formulario.hidden = true;
        VistaBloqueo.bloquear(r.datos.vigente);
      } else if (r.estado === 400) {
        mostrarMensaje('error', 'Revise los datos del reporte', r.datos.errores.join(' '));
      } else {
        mostrarMensaje('error', 'No se pudo registrar la avería', `El servidor respondió con el error ${r.estado}.`);
      }
    } catch (error) {
      mostrarMensaje('error', 'No se pudo registrar la avería', error.message);
    } finally {
      permitirEnvio(true);
    }
  }

  formulario.addEventListener('submit', async (evento) => {
    evento.preventDefault();
    ocultarMensaje();
    const datos = leerDatos();
    if (!validar(datos)) return;
    await enviarReporte(datos);
  });

  formulario.addEventListener('reset', () => {
    ocultarMensaje();
    formulario.querySelectorAll('.invalido').forEach((c) => c.classList.remove('invalido'));
    VistaBloqueo.ocultar();
    permitirEnvio(true);
    setTimeout(() => { contador.textContent = '0'; });
  });

  selectAula.addEventListener('change', revisarDuplicado);
  selectCategoria.addEventListener('change', revisarDuplicado);

  // Desde la vista de bloqueo: volver al formulario vacío.
  VistaBloqueo.alPedirOtraAveria(() => {
    formulario.reset();
    formulario.hidden = false;
    selectAula.focus();
  });

  descripcion.addEventListener('input', () => {
    contador.textContent = descripcion.value.length;
  });

  cargarCatalogos();
})();
