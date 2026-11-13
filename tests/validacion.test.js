const { describe, test } = require('node:test');
const assert = require('node:assert');
const { validarAveria } = require('../src/validacion');

const valida = { aula_id: '1', tipo_id: '2', descripcion: 'El proyector no enciende', reportado_por: 'Integrante 1' };

describe('CP-01 Registrar avería', () => {
  test('acepta una avería con todos los datos', () => {
    assert.strictEqual(validarAveria(valida).ok, true);
  });

  test('rechaza una avería sin aula', () => {
    const r = validarAveria({ ...valida, aula_id: '' });
    assert.strictEqual(r.ok, false);
    assert.ok(r.errores.includes('Seleccione el aula.'));
  });

  test('rechaza un aula que no es un número', () => {
    assert.strictEqual(validarAveria({ ...valida, aula_id: 'A-101' }).ok, false);
  });

  test('rechaza una avería sin tipo', () => {
    const r = validarAveria({ ...valida, tipo_id: undefined });
    assert.ok(r.errores.includes('Seleccione el tipo de avería.'));
  });

  test('rechaza una descripción vacía', () => {
    assert.strictEqual(validarAveria({ ...valida, descripcion: '' }).ok, false);
  });

  test('rechaza una descripción con solo espacios', () => {
    assert.strictEqual(validarAveria({ ...valida, descripcion: '    ' }).ok, false);
  });

  test('rechaza una descripción de más de 500 caracteres', () => {
    assert.strictEqual(validarAveria({ ...valida, descripcion: 'x'.repeat(501) }).ok, false);
  });

  test('quita los espacios sobrantes de la descripción', () => {
    const r = validarAveria({ ...valida, descripcion: '  Pizarra rota  ' });
    assert.strictEqual(r.datos.descripcion, 'Pizarra rota');
  });

  test('conserva tildes y eñes', () => {
    const texto = 'El aire del aula de Diseño no enfría; está dañado el compresor';
    assert.strictEqual(validarAveria({ ...valida, descripcion: texto }).datos.descripcion, texto);
  });
});
