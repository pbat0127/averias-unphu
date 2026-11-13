const express = require('express');
const { getPool } = require('./db');
const { validarAveria } = require('./validacion');
const { ESTADOS, siguientesEstados, validarCambio } = require('./estados');
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

// Lista de averías, con filtro por estado (RF-01, RF-02).
app.get('/', async (req, res) => {
  const filtro = ESTADOS.includes(req.query.estado) ? req.query.estado : null;
  const [averias] = await getPool().query(`
    SELECT a.id, CONCAT(au.edificio, ' · ', au.nombre) AS aula, t.nombre AS tipo,
           a.descripcion, a.estado, te.nombre AS tecnico
    FROM averia a
    JOIN aula au ON au.id = a.aula_id
    JOIN tipo_averia t ON t.id = a.tipo_id
    LEFT JOIN tecnico te ON te.id = a.tecnico_id
    ${filtro ? 'WHERE a.estado = ?' : ''}
    ORDER BY a.creada_en DESC, a.id DESC`, filtro ? [filtro] : []);
  const [tecnicos] = await getPool().query('SELECT id, nombre FROM tecnico ORDER BY nombre');
  res.send(vistas.lista(
    averias.map(a => ({ ...a, siguientes: siguientesEstados(a.estado) })),
    { tecnicos, estados: ESTADOS, filtro, error: req.query.error }
  ));
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

// Cambiar el estado y asignar técnico (RF-02, RF-03).
app.post('/averias/:id/estado', async (req, res) => {
  const id = Number(req.params.id);
  const [[averia]] = await getPool().query('SELECT estado FROM averia WHERE id = ?', [id]);
  if (!averia) {
    return res.status(404).send(vistas.layout('No encontrada', '<p>La avería no existe.</p>'));
  }
  const { estado, tecnico_id: tecnicoId } = req.body;
  const cambio = validarCambio(averia.estado, estado, tecnicoId);
  if (!cambio.ok) {
    return res.redirect(`/?error=${encodeURIComponent(cambio.error)}`);
  }
  if (estado === 'Asignada') {
    await getPool().query('UPDATE averia SET estado = ?, tecnico_id = ? WHERE id = ?', [estado, Number(tecnicoId), id]);
  } else {
    await getPool().query('UPDATE averia SET estado = ? WHERE id = ?', [estado, id]);
  }
  res.redirect('/');
});

// Errores inesperados: casi siempre la base de datos no está creada o el .env está mal.
app.use((err, req, res, next) => {
  console.error(err);
  res.status(500).send(vistas.layout('Error', '<p>Ocurrió un error. Revise que la base de datos esté creada y que el archivo .env tenga los datos correctos.</p>'));
});

module.exports = app;
