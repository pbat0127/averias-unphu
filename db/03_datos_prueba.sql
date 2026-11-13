-- 03_datos_prueba.sql
-- Averías ficticias para probar el sistema. Cubren todos los estados.

SET NAMES utf8mb4;
USE averias;

INSERT INTO averia (aula_id, tipo_id, descripcion, reportado_por, estado, tecnico_id) VALUES
  (1, 6, 'El proyector no enciende.', 'Estudiante de prueba', 'Reportada', NULL),
  (4, 3, 'No hay conexión a internet en las computadoras de la fila 3.', 'Profesor de prueba', 'Asignada', 3),
  (3, 1, 'El aire acondicionado no enfría; se escucha un ruido fuerte en el compresor.', NULL, 'En reparación', 1),
  (2, 4, 'Dos pupitres tienen el brazo partido.', 'Estudiante de prueba', 'Cerrada', 2),
  (6, 2, 'Se va la luz del escenario cuando se encienden las bocinas.', NULL, 'Reportada', NULL);
