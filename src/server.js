require('dotenv').config({ quiet: true });
const app = require('./app');

const port = Number(process.env.PORT || 3000);
app.listen(port, () => {
  console.log(`Sistema de averías en http://localhost:${port}`);
});
