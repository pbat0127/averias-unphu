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

function lista(averias) {
  const filas = averias.map(a => `
  <tr>
    <td>${esc(a.id)}</td>
    <td>${esc(a.aula)}</td>
    <td>${esc(a.tipo)}</td>
    <td>${esc(a.descripcion)}</td>
    <td><span class="estado">${esc(a.estado)}</span></td>
  </tr>`).join('');
  return layout('Averías', `
<p><a href="/averias/nueva">+ Reportar una avería</a></p>
<table>
  <tr><th>#</th><th>Aula</th><th>Tipo</th><th>Descripción</th><th>Estado</th></tr>
  ${filas || '<tr><td colspan="5">No hay averías registradas.</td></tr>'}
</table>`);
}

function formulario({ aulas, tipos, errores = [], valores = {} }) {
  const opciones = (items, seleccionado) => items.map(i =>
    `<option value="${esc(i.id)}"${String(i.id) === String(seleccionado) ? ' selected' : ''}>${esc(i.nombre)}</option>`
  ).join('');
  return layout('Reportar avería', `
<h2>Reportar una avería</h2>
${errores.length ? `<div class="errores"><ul>${errores.map(e => `<li>${esc(e)}</li>`).join('')}</ul></div>` : ''}
<form method="post" action="/averias">
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
  <button type="submit">Guardar</button>
</form>`);
}

module.exports = { esc, layout, lista, formulario };
