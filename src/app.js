const path = require('path');
const fs = require('fs');
const crypto = require('crypto');
const express = require('express');
const multer = require('multer');
const { getPool } = require('./db');
const { validarAveria, validarFoto, MAX_FOTO } = require('./validacion');
const { ESTADOS, siguientesEstados, validarCambio } = require('./estados');
const vistas = require('./vistas');

const app = express();
app.use(express.urlencoded({ extended: false }));

// Fotos de las averías (CR-01). Se guardan en la carpeta uploads/.
const CARPETA_FOTOS = path.join(__dirname, '..', 'uploads');
const subida = multer({
  storage: multer.diskStorage({
    destination: CARPETA_FOTOS,
    filename: (req, file, cb) => cb(null, crypto.randomUUID() + (file.mimetype === 'image/png' ? '.png' : '.jpg'))
  }),
  limits: { fileSize: MAX_FOTO }
});
app.use('/uploads', express.static(CARPETA_FOTOS));

// Recibe la foto. Si pasa de 2 MB, no la guarda y deja el error para mostrarlo en el formulario.
function recibirFoto(req, res, next) {
  subida.single('foto')(req, res, err => {
    if (err instanceof multer.MulterError && err.code === 'LIMIT_FILE_SIZE') {
      req.errorFoto = 'La foto no puede pesar más de 2 MB.';
      return next();
    }
    next(err);
  });
}

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
           a.descripcion, a.estado, te.nombre AS tecnico, a.foto_url
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

// Registrar una avería (RF-01), con foto opcional (RF-04).
app.post('/averias', recibirFoto, async (req, res) => {
  const { errores, datos } = validarAveria(req.body);
  const foto = validarFoto(req.file);
  if (!foto.ok) errores.push(foto.error);
  if (req.errorFoto) errores.push(req.errorFoto);
  if (errores.length > 0) {
    if (req.file) fs.unlink(req.file.path, () => {});
    return res.status(400).send(vistas.formulario({ ...(await catalogos()), errores, valores: req.body }));
  }
  await getPool().query(
    'INSERT INTO averia (aula_id, tipo_id, descripcion, foto_url, reportado_por) VALUES (?, ?, ?, ?, ?)',
    [datos.aula_id, datos.tipo_id, datos.descripcion, req.file ? `/uploads/${req.file.filename}` : null, datos.reportado_por]
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
