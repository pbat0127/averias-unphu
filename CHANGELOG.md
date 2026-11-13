# Registro de cambios

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).
Las versiones siguen [Versionado Semántico](https://semver.org/lang/es/): MAYOR.MENOR.PARCHE.

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
