// Punto de entrada del Sistema de Averías UNPHU.
// La configuración de servicios y rutas está en Aplicacion.cs, para poder reutilizarla en las pruebas.
using AveriasUnphu.Web;

var app = Aplicacion.Crear(args);
app.Run();
