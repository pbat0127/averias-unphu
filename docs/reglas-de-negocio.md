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
| Backend | `ServicioSolicitudes.RegistrarAsync` valida antes de guardar (tarea 2.2). El endpoint `POST /api/solicitudes` responde **409** con la solicitud vigente. |
| Pruebas | `tests/AveriasUnphu.Pruebas/PruebasRegistroDuplicados.cs` (P-01 a P-04 y RD-01 a RD-05). |

## Prioridad automática

Al registrar una avería, el sistema asigna la prioridad cruzando la **criticidad** de la categoría de equipo
(qué tan necesario es para dar la clase) con el **impacto en la clase** que indica quien reporta (tarea 3.1).

| Criticidad del equipo ↓ / Impacto → | Impide la clase | Afecta parcialmente | No afecta |
|---|---|---|---|
| **Alta** (proyector, computadora, red e internet) | **Alta** | **Alta** | Media |
| **Media** (aire acondicionado, iluminación, electricidad, audio) | **Alta** | Media | Baja |
| **Baja** (mobiliario, pizarra) | Media | Baja | Baja |

En palabras:
- **Alta:** la avería impide la clase y el equipo es de criticidad Alta o Media, o el equipo es de criticidad Alta y afecta parcialmente.
- **Baja:** la avería no afecta la clase y el equipo no es de criticidad Alta, o el equipo es de criticidad Baja y no impide la clase.
- **Media:** los demás casos.

La criticidad de cada categoría está en `db/02_catalogos.sql`. La regla está en
`src/AveriasUnphu.Web/Dominio/ReglasPrioridad.cs` y se prueba en `PruebasPrioridad.cs` (las 9 combinaciones).

## Estados de una solicitud

```
Pendiente  →  En proceso  →  Resuelta
```
Solo se avanza un paso a la vez. Una solicitud Resuelta no se reabre: si la avería vuelve, se reporta una nueva (RD-04).
Lo controla `ServicioSeguimiento.CambiarEstadoAsync` (tarea 3.2): un salto como Pendiente → Resuelta responde **400**.

## Orden de la bandeja

Las solicitudes se ordenan por **prioridad** (Alta, Media, Baja) y, dentro de la misma prioridad,
de la **más antigua a la más nueva**, para atender primero lo urgente y no dejar olvidado lo que lleva más tiempo esperando.
