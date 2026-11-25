# Guía de evidencias

Cómo obtener la evidencia de cada tarea del plan de trabajo (pestaña *Evidencias por Integrante*).
Guarde las capturas en una carpeta compartida del grupo, con el número de tarea en el nombre (por ejemplo, `2.1-consulta.png`).

## Antes de empezar

1. Ejecute los scripts de `db/` en orden (`01` a `04`) en SQL Server Management Studio.
2. Ejecute las pruebas: `dotnet test`.
3. Inicie la aplicación: `dotnet run --project src/AveriasUnphu.Web` y abra http://localhost:5080.

Los datos de prueba (`db/03_datos_prueba.sql`) ya traen una solicitud **Pendiente** de *A-101 · Proyector*,
una **En proceso** de *LAB-INF-1 · Computadora* y una **Resuelta** de *A-201 · Aire acondicionado*.
Si repite las pruebas en la página, vuelva a ejecutar `03_datos_prueba.sql` para empezar de cero.

## Evidencia por tarea

| Tarea | Responsable | Evidencia | Cómo obtenerla |
|---|---|---|---|
| 1.1 Esquema y tablas | Daniel Caamaño | Script DDL versionado y diagrama entidad-relación | Commit `feat(bd): crea el esquema relacional…` con `db/01_esquema.sql`. Diagrama en `docs/modelo-de-datos.md`. Captura en SSMS de las 4 tablas en el Explorador de objetos. |
| 1.2 Catálogos | Daniel Caamaño | Script de catálogos y conteo | Commit con `db/02_catalogos.sql`. Captura del resultado final del script (cantidad de aulas, categorías y usuarios por perfil). |
| 2.1 Consulta de solicitudes activas | Daniel Caamaño | Script SQL y captura de ejecución sobre datos de prueba | Commit con `db/04_consulta_solicitudes_activas.sql`. Captura de los 3 resultados en SSMS: la consulta (1 fila), la tabla de 5 casos y la comprobación sin duplicados (0 filas). |
| 2.2 Endpoint de validación de duplicados | Pedro Batista | Commit del servicio y capturas de P-01 a P-04 | Commit `feat(api): valida duplicados antes de guardar…`. Captura de `dotnet test` con todas las pruebas aprobadas y de los casos en la página (ver tabla de abajo). |
| 2.3 Formulario de reporte | Andrea Masini | Commit del formulario y captura cargando catálogos | Commit `feat(web): agrega el formulario…`. Captura del formulario con la lista de aulas abierta. |
| 2.4 Alerta y vista de bloqueo | Andrea Masini | Commit de la vista y captura del bloqueo en P-02 | Commit `feat(web): agrega la alerta y la vista de bloqueo…`. Captura del caso P-02 (abajo). |
| 2.5 Integración formulario–validación | Pedro Batista | Flujo de bloqueo y registro funcionando | Commit `feat(web): integra el formulario…`. Capturas de P-01 y P-02. |
| 3.1 Prioridad automática | Pedro Batista | Commit de las reglas y capturas de la asignación | Commit `feat(prioridad): …`. Captura de `dotnet test` (pruebas `PruebasPrioridad`) y de un registro con prioridad Alta y otro con Baja. |
| 3.2 Listar y actualizar estado | Daniel Caamaño | Commit de los endpoints | Commit `feat(api): lista solicitudes por prioridad…`. Captura de http://localhost:5080/api/solicitudes en el navegador. |
| 3.3 Bandeja de seguimiento | Andrea Masini | Commit y captura de la bandeja | Commit `feat(web): agrega la bandeja…`. Captura de la bandeja y de una solicitud que pasa a *En proceso*. |

## Casos funcionales P-01 a P-04 en la página

| Caso | Qué hacer en el formulario | Resultado esperado |
|---|---|---|
| **P-01** Registro inicial | Aula **B-210**, categoría **Proyector**, impacto *Impide la clase*. Enviar. | Mensaje verde: *Solicitud #N registrada*, estado Pendiente, prioridad Alta. |
| **P-02** Bloqueo por duplicado | Aula **A-101**, categoría **Proyector**. | Alerta naranja con la solicitud vigente y el botón Enviar desactivado. |
| **P-03** Otra categoría | Misma aula **A-101**, categoría **Aire acondicionado**. Enviar. | La alerta desaparece y la solicitud se registra. |
| **P-04** Reapertura | Aula **A-201**, categoría **Aire acondicionado** (la anterior está Resuelta). Enviar. | Se registra una solicitud nueva. |

Después de cada caso, la consulta de `db/04_consulta_solicitudes_activas.sql` (parte 3) debe seguir devolviendo **0 filas**:
no hay dos solicitudes activas para la misma aula y categoría.
