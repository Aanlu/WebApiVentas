using WebApiVentas.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Se agregan los servicios de controladores al contenedor de inyección de dependencias
// para permitir que la API procese las solicitudes HTTP entrantes.
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// Inicialización del Servidor de Sockets en el puerto 6000.
// Esta llamada arranca un hilo en segundo plano que escucha conexiones TCP,
// permitiendo que el sistema de notificaciones funcione en paralelo a la API Web.
ServiciosSockets.IniciarServidor(6000);

app.Run();