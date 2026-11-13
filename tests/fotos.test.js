const { describe, test } = require('node:test');
const assert = require('node:assert');
const { validarFoto } = require('../src/validacion');

const DOS_MB = 2 * 1024 * 1024;
const archivo = (mimetype, size) => ({ mimetype, size });

describe('CP-08 Adjuntar foto a la avería', () => {
  test('la foto es opcional', () => {
    assert.strictEqual(validarFoto(undefined).ok, true);
  });

  test('acepta una foto JPG', () => {
    assert.strictEqual(validarFoto(archivo('image/jpeg', 300 * 1024)).ok, true);
  });

  test('acepta una foto PNG', () => {
    assert.strictEqual(validarFoto(archivo('image/png', 300 * 1024)).ok, true);
  });

  test('rechaza un archivo PDF', () => {
    assert.strictEqual(validarFoto(archivo('application/pdf', 100 * 1024)).ok, false);
  });

  test('rechaza una foto de más de 2 MB', () => {
    assert.strictEqual(validarFoto(archivo('image/jpeg', DOS_MB + 1)).ok, false);
  });

  test('acepta una foto de exactamente 2 MB', () => {
    assert.strictEqual(validarFoto(archivo('image/jpeg', DOS_MB)).ok, true);
  });
});
