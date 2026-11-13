const express = require('express');

const app = express();

// Ruta para comprobar que el servidor responde (no usa la base de datos).
app.get('/salud', (req, res) => {
  res.json({ ok: true });
});

app.get('/', (req, res) => {
  res.send('Sistema de averías UNPHU: en construcción.');
});

module.exports = app;
