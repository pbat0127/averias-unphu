/*
  04_consulta_solicitudes_activas.sql · Tarea 2.1
  Consulta que detecta si ya existe una solicitud ACTIVA (Pendiente o En proceso)
  para la misma aula y categoría de equipo. Es la base de la validación de duplicados (tarea 2.2).

  Ejecútelo después de 03_datos_prueba.sql. No modifica datos.
*/
SET NOCOUNT ON;
GO

USE AveriasUnphu;
GO

/* ------------------------------------------------------------------------------------------
   1. La consulta. Es la misma que ejecuta el backend antes de guardar una avería,
      con @aula_id y @categoria_id como parámetros.
      - Devuelve una fila  → ya hay una solicitud activa: el nuevo reporte es DUPLICADO.
      - No devuelve filas  → no hay solicitud activa: el reporte se puede registrar.
   ------------------------------------------------------------------------------------------ */
DECLARE @aula_id INT      = (SELECT id FROM dbo.aulas WHERE codigo = N'A-101');
DECLARE @categoria_id INT = (SELECT id FROM dbo.categorias_equipo WHERE nombre = N'Proyector');

SELECT TOP (1)
       s.id AS solicitud, a.codigo AS aula, c.nombre AS categoria,
       s.estado, s.prioridad, s.fecha_reporte, s.descripcion
FROM dbo.solicitudes AS s
JOIN dbo.aulas AS a             ON a.id = s.aula_id
JOIN dbo.categorias_equipo AS c ON c.id = s.categoria_id
WHERE s.aula_id = @aula_id
  AND s.categoria_id = @categoria_id
  AND s.estado IN (N'Pendiente', N'En proceso')
ORDER BY s.fecha_reporte DESC;
GO

/* ------------------------------------------------------------------------------------------
   2. La misma consulta aplicada a varios casos, en una sola tabla (sirve como evidencia).
   ------------------------------------------------------------------------------------------ */
SELECT casos.caso, casos.aula, casos.categoria,
       COALESCE(activa.estado, N'—') AS solicitud_activa,
       CASE WHEN activa.id IS NULL
            THEN N'Se puede registrar'
            ELSE N'Duplicado: ya existe la solicitud #' + CAST(activa.id AS NVARCHAR(10))
       END AS resultado
FROM (VALUES
    (N'1. Proyector con solicitud Pendiente',       N'A-101',     N'Proyector'),
    (N'2. Misma aula, otra categoría',              N'A-101',     N'Aire acondicionado'),
    (N'3. Computadora con solicitud En proceso',    N'LAB-INF-1', N'Computadora'),
    (N'4. Aire con la solicitud anterior Resuelta', N'A-201',     N'Aire acondicionado'),
    (N'5. Aula sin solicitudes',                    N'B-210',     N'Proyector')
) AS casos (caso, aula, categoria)
JOIN dbo.aulas AS a             ON a.codigo = casos.aula
JOIN dbo.categorias_equipo AS c ON c.nombre = casos.categoria
OUTER APPLY (
    SELECT TOP (1) s.id, s.estado
    FROM dbo.solicitudes AS s
    WHERE s.aula_id = a.id
      AND s.categoria_id = c.id
      AND s.estado IN (N'Pendiente', N'En proceso')
    ORDER BY s.fecha_reporte DESC
) AS activa
ORDER BY casos.caso;
GO

/* ------------------------------------------------------------------------------------------
   3. Comprobación: no hay duplicados activos en la tabla.
      Debe devolver CERO filas.
   ------------------------------------------------------------------------------------------ */
SELECT s.aula_id, s.categoria_id, COUNT(*) AS solicitudes_activas
FROM dbo.solicitudes AS s
WHERE s.estado IN (N'Pendiente', N'En proceso')
GROUP BY s.aula_id, s.categoria_id
HAVING COUNT(*) > 1;
GO
