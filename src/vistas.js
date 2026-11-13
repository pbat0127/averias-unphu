// Plantillas HTML sencillas, sin motor de vistas.
function esc(valor) {
  return String(valor ?? '')
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

function layout(titulo, contenido) {
  return `<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${esc(titulo)} · Averías UNPHU</title>
<style>
  body { font-family: system-ui, sans-serif; max-width: 1040px; margin: 2rem auto; padding: 0 1rem; color: #1f2328; }
  a { color: #0969da; }
  table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
  th, td { text-align: left; padding: .5rem; border-bottom: 1px solid #d0d7de; vertical-align: top; }
  label { display: block; margin-top: 1rem; font-weight: 600; }
  input, select, textarea { width: 100%; padding: .5rem; margin-top: .25rem; box-sizing: border-box; }
  td form { margin: 0 0 .25rem; } td select, td button { width: auto; margin: 0; }
  button { margin-top: 1rem; padding: .5rem 1rem; }
  .errores { background: #ffebe9; border: 1px solid #ff8182; padding: .5rem 1rem; }
  .estado { padding: .1rem .5rem; border-radius: 1rem; background: #ddf4ff; font-size: .85rem; white-space: nowrap; }
</style>
</head>
<body>
<h1><a href="/">Averías UNPHU</a></h1>
${contenido}
</body>
</html>`;
}

function lista(averias, { tecnicos = [], estados = [], filtro = null, error = null } = {}) {
  const enlaceFiltro = (valor, texto) => valor === filtro
    ? `<strong>${esc(texto)}</strong>`
    : `<a href="${valor ? `/?estado=${encodeURIComponent(valor)}` : '/'}">${esc(texto)}</a>`;
  const filtros = [enlaceFiltro(null, 'Todas'), ...estados.map(e => enlaceFiltro(e, e))].join(' · ');
  const opcionesTecnico = tecnicos.map(t => `<option value="${esc(t.id)}">${esc(t.nombre)}</option>`).join('');

  // Un botón por cada estado al que se puede pasar. Para "Asignada" hay que elegir técnico.
  const acciones = a => a.siguientes.map(sig => `
      <form method="post" action="/averias/${esc(a.id)}/estado">
        <input type="hidden" name="estado" value="${esc(sig)}">
        ${sig === 'Asignada' ? `<select name="tecnico_id" required><option value="">Técnico…</option>${opcionesTecnico}</select>` : ''}
        <button type="submit">Pasar a ${esc(sig)}</button>
      </form>`).join('') || '—';

  const filas = averias.map(a => `
  <tr>
    <td>${esc(a.id)}</td>
    <td>${esc(a.aula)}</td>
    <td>${esc(a.tipo)}</td>
    <td>${esc(a.descripcion)}</td>
    <td>${a.foto_url ? `<a href="${esc(a.foto_url)}" target="_blank">Ver foto</a>` : '—'}</td>
    <td><span class="estado">${esc(a.estado)}</span></td>
    <td>${esc(a.tecnico ?? '—')}</td>
    <td>${acciones(a)}</td>
  </tr>`).join('');

  return layout('Averías', `
<p><a href="/averias/nueva">+ Reportar una avería</a></p>
<p>Filtrar: ${filtros}</p>
${error ? `<div class="errores">${esc(error)}</div>` : ''}
<table>
  <tr><th>#</th><th>Aula</th><th>Tipo</th><th>Descripción</th><th>Foto</th><th>Estado</th><th>Técnico</th><th>Acción</th></tr>
  ${filas || '<tr><td colspan="8">No hay averías con ese filtro.</td></tr>'}
</table>`);
}

function formulario({ aulas, tipos, errores = [], valores = {} }) {
  const opciones = (items, seleccionado) => items.map(i =>
    `<option value="${esc(i.id)}"${String(i.id) === String(seleccionado) ? ' selected' : ''}>${esc(i.nombre)}</option>`
  ).join('');
  return layout('Reportar avería', `
<h2>Reportar una avería</h2>
${errores.length ? `<div class="errores"><ul>${errores.map(e => `<li>${esc(e)}</li>`).join('')}</ul></div>` : ''}
<form method="post" action="/averias" enctype="multipart/form-data">
  <label>Aula
    <select name="aula_id"><option value="">Seleccione…</option>${opciones(aulas, valores.aula_id)}</select>
  </label>
  <label>Tipo de avería
    <select name="tipo_id"><option value="">Seleccione…</option>${opciones(tipos, valores.tipo_id)}</select>
  </label>
  <label>Descripción
    <textarea name="descripcion" rows="4" maxlength="500">${esc(valores.descripcion)}</textarea>
  </label>
  <label>Reportado por (opcional)
    <input name="reportado_por" value="${esc(valores.reportado_por)}">
  </label>
  <label>Foto (opcional, JPG o PNG)
    <input type="file" name="foto" accept="image/jpeg,image/png">
  </label>
  <button type="submit">Guardar</button>
</form>`);
}

module.exports = { esc, layout, lista, formulario };
