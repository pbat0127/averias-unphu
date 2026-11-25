// Funciones compartidas para llamar a la API del Sistema de Averías.
const Api = (() => {
  async function pedir(url, opciones = {}) {
    let respuesta;
    try {
      respuesta = await fetch(url, {
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        ...opciones,
      });
    } catch {
      throw new Error('No se pudo conectar con el servidor. Revise que la aplicación esté en ejecución.');
    }
    const texto = await respuesta.text();
    const datos = texto ? JSON.parse(texto) : null;
    return { estado: respuesta.status, ok: respuesta.ok, datos };
  }

  async function obtener(url) {
    const r = await pedir(url);
    if (!r.ok) throw new Error(`Error ${r.estado} al consultar ${url}.`);
    return r.datos;
  }

  return {
    pedir,
    obtener,
    enviar: (url, metodo, cuerpo) => pedir(url, { method: metodo, body: JSON.stringify(cuerpo) }),
  };
})();

// Utilidades de formato
const Formato = {
  fecha(iso) {
    if (!iso) return '—';
    return new Date(iso).toLocaleString('es-DO', { dateStyle: 'medium', timeStyle: 'short' });
  },
  etiqueta(tipo, valor) {
    const span = document.createElement('span');
    span.className = `etiqueta ${tipo}-${String(valor).replace(/\s+/g, '-')}`;
    span.textContent = valor;
    return span;
  },
};
