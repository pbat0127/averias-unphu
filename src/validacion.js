const MAX_DESCRIPCION = 500;

// Valida los datos del formulario de registro (RF-01).
// Devuelve { ok, errores, datos } para que la ruta decida qué mostrar.
function validarAveria(entrada = {}) {
  const errores = [];
  const aulaId = Number(entrada.aula_id);
  const tipoId = Number(entrada.tipo_id);
  const descripcion = String(entrada.descripcion ?? '').trim();
  const reportadoPor = String(entrada.reportado_por ?? '').trim();

  if (!Number.isInteger(aulaId) || aulaId <= 0) errores.push('Seleccione el aula.');
  if (!Number.isInteger(tipoId) || tipoId <= 0) errores.push('Seleccione el tipo de avería.');
  if (descripcion === '') errores.push('Describa la avería.');
  if (descripcion.length > MAX_DESCRIPCION) {
    errores.push(`La descripción no puede pasar de ${MAX_DESCRIPCION} caracteres.`);
  }

  return {
    ok: errores.length === 0,
    errores,
    datos: { aula_id: aulaId, tipo_id: tipoId, descripcion, reportado_por: reportadoPor || null }
  };
}

module.exports = { validarAveria, MAX_DESCRIPCION };
