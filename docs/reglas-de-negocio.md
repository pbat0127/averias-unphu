# Reglas de negocio

## Reglas de duplicidad

Una **solicitud activa** es la que está en estado **Pendiente** o **En proceso**.
Para decidir si un reporte nuevo es duplicado, el sistema compara solo el **aula** y la **categoría de equipo**.

| Regla | Condición | Qué hace el sistema | Caso de prueba |
|---|---|---|---|
| **RD-01** Duplicado | Misma aula + misma categoría + existe una solicitud activa | **Bloquea** el registro y muestra la solicitud vigente (número, estado, prioridad, fecha y descripción). | P-02 |
| **RD-02** Otra categoría | Misma aula, distinta categoría | No es duplicado: **registra** la solicitud. | P-03 |
| **RD-03** Otra aula | Misma categoría, distinta aula | No es duplicado: **registra** la solicitud. | — |
| **RD-04** Reapertura | Misma aula + misma categoría, pero la solicitud anterior está **Resuelta** | No es duplicado: **registra** una solicitud nueva (la avería volvió a ocurrir). | P-04 |
| **RD-05** Envío simultáneo | Dos personas envían el mismo reporte al mismo tiempo | La base de datos solo acepta el primero (índice único `ux_solicitudes_una_activa`); el segundo recibe el bloqueo de RD-01. | — |

Notas:
- La **descripción no se compara**: es texto libre y cada persona la escribe distinto.
- El registro inicial (P-01) es el caso base: sin solicitudes activas para esa aula y categoría, se registra.
- **Límite conocido:** si un aula tiene dos equipos de la misma categoría (por ejemplo, dos aires),
  el segundo se bloquea mientras el primero esté activo. Una mejora futura es agregar el equipo específico.

### Dónde se aplica

| Lugar | Cómo |
|---|---|
| Base de datos | Índice único filtrado `ux_solicitudes_una_activa` (`db/01_esquema.sql`). |
| Consulta | `db/04_consulta_solicitudes_activas.sql` (tarea 2.1). |

## Estados de una solicitud

```
Pendiente  →  En proceso  →  Resuelta
```
Solo se avanza un paso a la vez. Una solicitud Resuelta no se reabre: si la avería vuelve, se reporta una nueva (RD-04).
Lo controla `ServicioSeguimiento.CambiarEstadoAsync` (tarea 3.2): un salto como Pendiente → Resuelta responde **400**.

## Orden de la bandeja

Las solicitudes se ordenan por **prioridad** (Alta, Media, Baja) y, dentro de la misma prioridad,
de la **más antigua a la más nueva**, para atender primero lo urgente y no dejar olvidado lo que lleva más tiempo esperando.
