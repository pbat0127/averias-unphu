# Registro de cambios

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).
Las versiones siguen [Versionado Semántico](https://semver.org/lang/es/): MAYOR.MENOR.PARCHE.

## [Sin publicar]
### Agregado
- Base de datos en SQL Server: esquema relacional, catálogos iniciales, datos de prueba y consulta de solicitudes activas (tareas 1.1, 1.2 y 2.1).
- Formulario web de reporte de averías que carga los catálogos (tarea 2.3).
- Alerta y vista de bloqueo con la solicitud vigente ante un reporte duplicado (tarea 2.4).
- Validación de duplicados en el backend antes de guardar: `POST /api/solicitudes` responde 409 con la solicitud vigente (tarea 2.2).
- Integración del formulario con la validación (tarea 2.5).
- Listado por prioridad y cambio de estado: `GET /api/solicitudes` y `PATCH /api/solicitudes/{id}/estado` (tarea 3.2).
- Bandeja de seguimiento para el perfil Mantenimiento (tarea 3.3).
- Prioridad automática Alta, Media o Baja según la criticidad del equipo y el impacto en la clase (tarea 3.1).
- Reglas de negocio, modelo de datos y guía de evidencias en `docs/`.

### Cambiado
- El proyecto pasa de ser el prototipo en Node.js y MySQL a la versión real en ASP.NET Core 10 y SQL Server.
- Las pruebas automáticas se ejecutan con xUnit (`dotnet test`) en GitHub Actions.

### Eliminado
- El código del prototipo en Node.js (sigue disponible en las etiquetas `v0.1`, `v0.2` y `v0.3`).

## Prototipo en Node.js (etiquetas v0.1 a v0.3)

## [0.3.0]
### Agregado
- Foto opcional al reportar una avería: JPG o PNG de hasta 2 MB (cambio CR-01, RF-04, #3).
- Script de migración `db/04_foto_averia.sql`.
- Casos de prueba CP-08.

### Corregido
- Se rechazan las fotos de más de 2 MB (falla encontrada por CP-08 en la revisión del pull request).

## [0.2.0]
### Agregado
- Flujo de estados Reportada → Asignada → En reparación → Cerrada (RF-02, #2).
- Asignación de técnico (RF-03, #2).
- Filtro de la lista por estado.
- Casos de prueba CP-04.

## [0.1.0]
### Agregado
- Registro y lista de averías (RF-01, #1).
- Scripts de base de datos `01` a `03`, con datos de prueba.
- Pruebas automáticas (CP-01) que corren en GitHub Actions.
