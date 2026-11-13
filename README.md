# Sistema de Averías UNPHU

Aplicación web para reportar y dar seguimiento a las averías de las aulas y espacios del campus.

> Repositorio de ejemplo preparado por el Grupo 01 para la exposición de la Unidad 3 (INF-386-03).

## Qué hace
- Registrar una avería (aula, tipo, descripción) y verla en la lista.
- Asignar un técnico y cambiar el estado: Reportada → Asignada → En reparación → Cerrada.
- Filtrar la lista por estado.
- Adjuntar una foto opcional (JPG o PNG) al reportar la avería.

## Tecnologías
- Node.js 22 LTS (ver `.nvmrc`)
- Express 5
- MySQL 8.0

## Requisitos previos
- Node.js 22 o superior, con npm
- MySQL 8.0 instalado y en ejecución
- Git

## Instalación
1. Clone el repositorio:
   ```
   git clone https://github.com/<usuario>/averias-unphu.git
   cd averias-unphu
   ```
2. Instale las dependencias con las versiones exactas de `package-lock.json`:
   ```
   npm ci
   ```

## Configuración
Copie la plantilla y complete sus datos locales:
```
cp .env.example .env
```

| Variable | Ejemplo | Para qué sirve |
|---|---|---|
| `PORT` | `3000` | Puerto del servidor web |
| `DB_HOST` | `localhost` | Servidor de MySQL |
| `DB_PORT` | `3306` | Puerto de MySQL |
| `DB_USER` | `root` | Usuario de MySQL |
| `DB_PASSWORD` | *(su contraseña)* | Contraseña de MySQL |
| `DB_NAME` | `averias` | Nombre de la base de datos |

El archivo `.env` tiene contraseñas, por eso **no se sube** al repositorio (está en `.gitignore`).

## Base de datos
Ejecute los scripts **en este orden**:
```
mysql -u root -p < db/01_schema.sql
mysql -u root -p < db/02_catalogos.sql
mysql -u root -p < db/03_datos_prueba.sql
mysql -u root -p < db/04_foto_averia.sql
```
- En **PowerShell** use: `Get-Content db/01_schema.sql | mysql -u root -p` (repita con cada script).
- También puede abrir cada script en **MySQL Workbench** y ejecutarlo.
- El script `01` borra las tablas si ya existen: úselo solo en su computadora.

## Ejecución
```
npm run dev
```
Abra <http://localhost:3000>. Verá la lista con las averías de prueba.

## Pruebas
```
npm test
```
Resultado esperado: **18 pruebas aprobadas**. Las pruebas no necesitan la base de datos.

También se ejecutan solas en GitHub Actions con cada `push` y cada pull request.

## Estructura
```
db/      scripts SQL numerados, en el orden en que se ejecutan
docs/    bitácora, decisiones, cambios y matriz de trazabilidad
src/     código de la aplicación
tests/   pruebas automáticas
uploads/  fotos subidas (no se versionan)
```

## Documentación
- [Bitácora](docs/bitacora.md)
- [Decisiones técnicas (ADR)](docs/decisiones/)
- [Matriz de trazabilidad](docs/trazabilidad.md)
- [Solicitudes de cambio](docs/cambios.md)
- [Registro de cambios por versión](CHANGELOG.md)

## Autores
Grupo 01 — Integrante 1 · Integrante 2 · Integrante 3
