const express = require('express');
const { getPool } = require('./db');
const { validarAveria } = require('./validacion');
const vistas = require('./vistas');

const app = express();
app.use(express.urlencoded({ extended: false }));

app.get('/salud', (req, res) => {
  res.json({ ok: true });
});

async function catalogos() {
  const db = getPool();
  const [aulas] = await db.query("SELECT id, CONCAT(edificio, ' · ', nombre) AS nombre FROM aula ORDER BY edificio, nombre");
  const [tipos] = await db.query('SELECT id, nombre FROM tipo_averia ORDER BY nombre');
  return { aulas, tipos };
}

// Lista de averías (RF-01).
app.get('/', async (req, res) => {
  const [averias] = await getPool().query(`
    SELECT a.id, CONCAT(au.edificio, ' · ', au.nombre) AS aula, t.nombre AS tipo,
           a.descripcion, a.estado
    FROM averia a
    JOIN aula au ON au.id = a.aula_id
    JOIN tipo_averia t ON t.id = a.tipo_id
    ORDER BY a.creada_en DESC, a.id DESC`);
  res.send(vistas.lista(averias));
});

app.get('/averias/nueva', async (req, res) => {
  res.send(vistas.formulario(await catalogos()));
});

// Registrar una avería (RF-01).
app.post('/averias', async (req, res) => {
  const { ok, errores, datos } = validarAveria(req.body);
  if (!ok) {
    return res.status(400).send(vistas.formulario({ ...(await catalogos()), errores, valores: req.body }));
  }
  await getPool().query(
    'INSERT INTO averia (aula_id, tipo_id, descripcion, reportado_por) VALUES (?, ?, ?, ?)',
    [datos.aula_id, datos.tipo_id, datos.descripcion, datos.reportado_por]
  );
  res.redirect('/');
});

// Errores inesperados: casi siempre la base de datos no está creada o el .env está mal.
app.use((err, req, res, next) => {
  console.error(err);
  res.status(500).send(vistas.layout('Error', '<p>Ocurrió un error. Revise que la base de datos esté creada y que el archivo .env tenga los datos correctos.</p>'));
});

module.exports = app;
