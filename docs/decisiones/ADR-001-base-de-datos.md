# ADR-001: Usar una base de datos relacional (MySQL)

- **Estado:** aceptada
- **Responsables:** Grupo 01

## Contexto
El sistema guarda aulas, tipos de avería, técnicos y averías. Cada avería pertenece a un aula, tiene un tipo y puede tener un técnico asignado.

## Decisión
Usar MySQL 8.0, una base de datos relacional, con claves foráneas entre las tablas.

## Alternativas consideradas
- **Base de datos NoSQL (por ejemplo, MongoDB):** es flexible, pero por sí sola no garantiza que una avería apunte a un aula que exista.
- **Archivos JSON:** es simple, pero no maneja bien que varias personas guarden al mismo tiempo.

## Consecuencias
- Ganamos integridad: no se puede guardar una avería con un aula o un técnico que no existen.
- Cada cambio en la estructura necesita un script nuevo en `db/` (una migración).
