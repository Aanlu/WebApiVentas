using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebApiVentas.Servicios
{
    // Servicio encargado de la comunicación en tiempo real mediante Sockets TCP.
    // Gestiona múltiples conexiones de clientes y el envío de mensajes broadcast.
    public class ServiciosSockets
    {
        private static TcpListener _listener;
        // Lista compartida que almacena los clientes conectados actualmente.
        private static List<TcpClient> _client = new List<TcpClient>();
        private static bool _interruptor = false;

        // Objeto de bloqueo (Monitor) para sincronizar el acceso a la lista de clientes.
        private static readonly object _lock = new object();

        public static void IniciarServidor(int puerto)
        {
            if (_interruptor) return;

            // Configuración del listener para aceptar conexiones en cualquier interfaz de red.
            _listener = new TcpListener(IPAddress.Any, puerto);
            _listener.Start();
            _interruptor = true;

            // Se crea un hilo dedicado (Thread) para aceptar conexiones entrantes.
            // Es necesario usar un hilo explícito aquí para mantener un bucle infinito de escucha
            // sin congelar el flujo principal de la aplicación.
            Thread hiloEscucha = new Thread(EscucharNuevosClientes);
            hiloEscucha.IsBackground = true; // El hilo terminará si la aplicación se cierra.
            hiloEscucha.Start();
        }

        private static void EscucharNuevosClientes()
        {
            while (_interruptor)
            {
                try
                {
                    // Operación bloqueante que espera hasta que llega un nuevo cliente.
                    TcpClient nuevoCliente = _listener.AcceptTcpClient();

                    // Sección crítica: Se bloquea la lista _client para agregar el nuevo socket
                    // de forma segura, evitando condiciones de carrera (Race Conditions).
                    lock (_lock)
                    {
                        _client.Add(nuevoCliente);
                    }
                }
                catch
                {
                    // Ignoramos errores de conexión para mantener el servicio activo.
                }
            }
        }

        // Método para enviar notificaciones a todos los clientes conectados.
        public static void NotificarCambios(string mensaje)
        {
            byte[] paquetes = Encoding.UTF8.GetBytes(mensaje);

            // El envío masivo se ejecuta en una tarea asíncrona (Task) para no afectar
            // el rendimiento de la operación que solicitó la notificación.
            Task.Run(() =>
            {
                // Se bloquea la lista durante la iteración para asegurar la integridad de los datos.
                lock (_lock)
                {
                    // Iteración inversa para permitir la eliminación segura de elementos
                    // si detectamos que un cliente se ha desconectado.
                    for (int i = _client.Count - 1; i >= 0; i--)
                    {
                        TcpClient cliente = _client[i];
                        try
                        {
                            NetworkStream stream = cliente.GetStream();
                            stream.Write(paquetes, 0, paquetes.Length);
                        }
                        catch
                        {
                            // Si falla el envío, asumimos desconexión y limpiamos el recurso.
                            _client.RemoveAt(i);
                        }
                    }
                }
            });
        }
    }
}