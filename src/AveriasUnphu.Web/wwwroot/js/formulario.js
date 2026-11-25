// Formulario de reporte de averías (tarea 2.3).
// Carga los catálogos desde la API y valida los campos antes de enviar.
(() => {
  const formulario = document.getElementById('formulario-reporte');
  const selectUsuario = document.getElementById('usuario');
  const selectAula = document.getElementById('aula');
  const selectCategoria = document.getElementById('categoria');
  const descripcion = document.getElementById('descripcion');
  const contador = document.getElementById('contador');
  const mensaje = document.getElementById('mensaje');

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

  // El envío a la API se conecta en la tarea 2.5.
  async function enviarReporte(datos) {
    console.info('Reporte listo para enviar:', datos);
    mostrarMensaje('aviso', 'Formulario completo', 'Los datos son válidos. El envío al servidor se conecta en la tarea 2.5.');
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
    setTimeout(() => { contador.textContent = '0'; });
  });

  descripcion.addEventListener('input', () => {
    contador.textContent = descripcion.value.length;
  });

  cargarCatalogos();
})();
