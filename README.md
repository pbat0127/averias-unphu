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
```
Con SQL Server Express use `-S localhost\SQLEXPRESS`. El modelo está explicado en [docs/modelo-de-datos.md](docs/modelo-de-datos.md).

## Ejecución
```
dotnet run --project src/AveriasUnphu.Web
```
Abra http://localhost:5080.

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
