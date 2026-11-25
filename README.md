# Sistema de Averías UNPHU

Aplicación web para reportar y dar seguimiento a las averías de las aulas y laboratorios del campus.
Proyecto Integrador INF-386-03 · Grupo 01.

## Tecnologías
- ASP.NET Core 10 (C#): API y páginas web
- SQL Server 2019 o superior (también sirve SQL Server Express)
- xUnit para las pruebas automáticas

## Requisitos previos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server o SQL Server Express, con SQL Server Management Studio (SSMS) o `sqlcmd`
- Git

## Instalación
```
git clone https://github.com/pbat0127/averias-unphu.git
cd averias-unphu
dotnet restore
```

## Base de datos
Ejecute los scripts de la carpeta `db` en orden, desde SSMS (abrir y pulsar F5) o con `sqlcmd`:
```
sqlcmd -S localhost -E -i db/01_esquema.sql
sqlcmd -S localhost -E -i db/02_catalogos.sql
sqlcmd -S localhost -E -i db/03_datos_prueba.sql
sqlcmd -S localhost -E -i db/04_consulta_solicitudes_activas.sql
```
| Script | Qué hace |
|---|---|
| `01_esquema.sql` | Crea la base de datos `AveriasUnphu` y sus tablas. |
| `02_catalogos.sql` | Carga aulas, categorías de equipo y usuarios de prueba. |
| `03_datos_prueba.sql` | Carga solicitudes de prueba (borra las que haya). Solo para desarrollo. |
| `04_consulta_solicitudes_activas.sql` | Consulta de solicitudes activas por aula y categoría (base de la validación de duplicados). No modifica datos. |

Con SQL Server Express use `-S localhost\SQLEXPRESS`. El modelo está explicado en [docs/modelo-de-datos.md](docs/modelo-de-datos.md)
y las reglas de duplicidad en [docs/reglas-de-negocio.md](docs/reglas-de-negocio.md).

## Ejecución
```
dotnet run --project src/AveriasUnphu.Web
```
Abra http://localhost:5080.

Si su SQL Server no es `localhost` (por ejemplo, SQL Server Express), cambie la cadena de conexión
`AveriasUnphu` en `src/AveriasUnphu.Web/appsettings.json`, por ejemplo `Server=localhost\\SQLEXPRESS;...`.

### Cómo se usa
1. **Reportar avería:** elija quién reporta, el aula, la categoría del equipo, el impacto en la clase y describa la avería.
2. Si ya hay una solicitud **activa** (Pendiente o En proceso) para esa aula y categoría, el formulario lo avisa
   y no deja enviar. Si el envío llega al servidor igual, se muestra la vista de bloqueo con la solicitud vigente.
3. **Bandeja de mantenimiento** (`/bandeja.html`): lista las solicitudes por prioridad, con filtros por estado,
   aula y equipo. Los botones **Iniciar reparación** y **Marcar resuelta** avanzan el estado.

## API
| Método y ruta | Qué hace |
|---|---|
| `GET /api/catalogos/aulas` | Aulas y laboratorios. |
| `GET /api/catalogos/categorias` | Categorías de equipo con su criticidad. |
| `GET /api/catalogos/usuarios?perfil=Reportante` | Usuarios, filtrados por perfil. |
| `GET /api/solicitudes/validar?aulaId=1&categoriaId=1` | Dice si ya hay una solicitud activa para esa aula y categoría. |
| `POST /api/solicitudes` | Registra una avería. Valida duplicados antes de guardar: **201** si se registró, **409** con la solicitud vigente si es duplicado, **400** si faltan datos. |
| `GET /api/solicitudes/{id}` | Una solicitud por su número. |
| `GET /api/solicitudes?estado=activas&aulaId=1&categoriaId=1` | Bandeja: solicitudes ordenadas por prioridad (Alta, Media, Baja) y antigüedad. `estado` puede ser `activas`, `Pendiente`, `En proceso` o `Resuelta`; sin filtros, todas. |
| `PATCH /api/solicitudes/{id}/estado` | Avanza el estado un paso: `{ "estado": "En proceso" }` o `{ "estado": "Resuelta" }`. |

## Pruebas
```
dotnet test
```

## Estructura
```
db/                       Scripts de SQL Server, en el orden en que se ejecutan
docs/                     Modelo de datos y documentación
src/AveriasUnphu.Web/     Aplicación web (API y páginas en wwwroot)
tests/AveriasUnphu.Pruebas/  Pruebas automáticas (xUnit)
.github/workflows/        Pruebas en GitHub Actions
```

## Historial
Las versiones `v0.1` a `v0.3` son el prototipo en Node.js que se usó en la exposición de la Unidad 3.
A partir de aquí el proyecto continúa en ASP.NET Core y SQL Server.
