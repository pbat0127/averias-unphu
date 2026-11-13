const MAX_DESCRIPCION = 500;
const TIPOS_FOTO = ['image/jpeg', 'image/png'];
const MAX_FOTO = 2 * 1024 * 1024; // 2 MB

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

// RF-04 (cambio CR-01): la foto es opcional; si viene, debe ser JPG o PNG y pesar 2 MB o menos.
function validarFoto(archivo) {
  if (!archivo) return { ok: true };
  if (!TIPOS_FOTO.includes(archivo.mimetype)) {
    return { ok: false, error: 'La foto debe ser JPG o PNG.' };
  }
  if (archivo.size > MAX_FOTO) {
    return { ok: false, error: 'La foto no puede pesar más de 2 MB.' };
  }
  return { ok: true };
}

module.exports = { validarAveria, validarFoto, MAX_DESCRIPCION, MAX_FOTO };
