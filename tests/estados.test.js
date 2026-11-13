const { describe, test } = require('node:test');
const assert = require('node:assert');
const { siguientesEstados, validarCambio } = require('../src/estados');

describe('CP-04 Cambiar estado de una avería', () => {
  test('permite pasar de Reportada a Asignada con técnico', () => {
    assert.strictEqual(validarCambio('Reportada', 'Asignada', '1').ok, true);
  });

  test('permite pasar de Asignada a En reparación', () => {
    assert.strictEqual(validarCambio('Asignada', 'En reparación').ok, true);
  });

  test('permite pasar de En reparación a Cerrada', () => {
    assert.strictEqual(validarCambio('En reparación', 'Cerrada').ok, true);
  });

  test('no permite saltar de Reportada a Cerrada', () => {
    assert.strictEqual(validarCambio('Reportada', 'Cerrada').ok, false);
  });

  test('no permite volver de Cerrada a Reportada', () => {
    assert.strictEqual(validarCambio('Cerrada', 'Reportada').ok, false);
  });

  test('rechaza un estado desconocido', () => {
    assert.strictEqual(validarCambio('Reportada', 'Perdida').ok, false);
  });

  test('exige un técnico para pasar a Asignada', () => {
    const r = validarCambio('Reportada', 'Asignada', '');
    assert.strictEqual(r.ok, false);
    assert.match(r.error, /técnico/);
  });

  test('indica el siguiente estado posible', () => {
    assert.deepStrictEqual(siguientesEstados('Reportada'), ['Asignada']);
    assert.deepStrictEqual(siguientesEstados('Cerrada'), []);
  });
});
