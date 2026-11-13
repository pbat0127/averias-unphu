// Flujo de estados de una avería (RF-02) y asignación de técnico (RF-03).
const ESTADOS = ['Reportada', 'Asignada', 'En reparación', 'Cerrada'];

const SIGUIENTE = {
  'Reportada': ['Asignada'],
  'Asignada': ['En reparación'],
  'En reparación': ['Cerrada'],
  'Cerrada': []
};

function siguientesEstados(actual) {
  return SIGUIENTE[actual] ?? [];
}

// Valida un cambio de estado. Para pasar a "Asignada" hace falta un técnico.
function validarCambio(actual, nuevo, tecnicoId) {
  if (!ESTADOS.includes(nuevo)) {
    return { ok: false, error: `Estado desconocido: ${nuevo}` };
  }
  if (!siguientesEstados(actual).includes(nuevo)) {
    return { ok: false, error: `No se puede pasar de "${actual}" a "${nuevo}".` };
  }
  if (nuevo === 'Asignada' && !(Number(tecnicoId) > 0)) {
    return { ok: false, error: 'Para asignar la avería hay que elegir un técnico.' };
  }
  return { ok: true };
}

module.exports = { ESTADOS, siguientesEstados, validarCambio };
