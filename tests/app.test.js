const { test } = require('node:test');
const assert = require('node:assert');
const app = require('../src/app');

test('GET /salud responde que el servidor está activo', async () => {
  const server = app.listen(0);
  try {
    const { port } = server.address();
    const res = await fetch(`http://127.0.0.1:${port}/salud`);
    assert.strictEqual(res.status, 200);
    assert.deepStrictEqual(await res.json(), { ok: true });
  } finally {
    server.closeAllConnections();
    server.close();
  }
});
