# Matriz de trazabilidad

Cada requerimiento se conecta con su tarea (issue), la rama donde se programó, sus casos de prueba y el resultado.

| Requerimiento | Tarea | Rama | Casos de prueba | Resultado |
|---|---|---|---|---|
| RF-01 Registrar una avería con aula, tipo y descripción | #1 | `feature/registro-averia` | CP-01 (`tests/validacion.test.js`) | Aprobado (v0.1) |
| RF-02 Cambiar el estado de una avería | #2 | `feature/estado-averia` | CP-04 (`tests/estados.test.js`) | Aprobado (v0.2) |
| RF-03 Asignar un técnico a la avería | #2 | `feature/estado-averia` | CP-04 (`tests/estados.test.js`) | Aprobado (v0.2) |
| RF-04 Adjuntar una foto a la avería (cambio CR-01) | #3 | `feature/foto-averia`, pull request que cierra #3 | CP-08 (`tests/fotos.test.js`) | Aprobado (v0.3) |
