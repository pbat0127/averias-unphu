const mysql = require('mysql2/promise');

let pool;

// Crea la conexión la primera vez que se necesita.
// Así las pruebas que no usan la base de datos no necesitan MySQL.
function getPool() {
  if (!pool) {
    pool = mysql.createPool({
      host: process.env.DB_HOST || 'localhost',
      port: Number(process.env.DB_PORT || 3306),
      user: process.env.DB_USER || 'root',
      password: process.env.DB_PASSWORD || '',
      database: process.env.DB_NAME || 'averias',
      charset: 'utf8mb4',
      waitForConnections: true,
      connectionLimit: 5
    });
  }
  return pool;
}

module.exports = { getPool };
