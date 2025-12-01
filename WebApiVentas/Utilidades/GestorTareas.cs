using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

namespace WebApiVentas.Utilizades
{
    // Clase estática encargada de registrar operaciones en la bitácora del servidor.
    // Implementa técnicas de sincronización para evitar conflictos de escritura.
    public class GestorTareas
    {
        // Se utiliza un SemaphoreSlim como mecanismo de sincronización (Semáforo).
        // Configurado en (1, 1), actúa como un bloqueo exclusivo (Mutex), asegurando
        // que solo un hilo pueda escribir en el archivo a la vez.
        private static readonly SemaphoreSlim Semaforo = new SemaphoreSlim(1, 1);

        public static void RegistrarOperacion(string mensaje)
        {
            // La escritura se delega a un hilo del ThreadPool mediante Task.Run.
            // Esto libera al hilo principal de la API para que pueda responder al cliente
            // inmediatamente, sin esperar a que termine la operación de disco (I/O).
            Task.Run(async () =>
            {
                await EscribirEnBitacora(mensaje);
            });
        }

        private static async Task EscribirEnBitacora(string mensaje)
        {
            // El hilo solicita acceso al recurso compartido. Si otro hilo está escribiendo,
            // este esperará aquí de forma asíncrona sin bloquear el procesador.
            await Semaforo.WaitAsync();
            try
            {
                string rutaBitacora = "bitacora.txt";
                string contenido = $"{DateTime.Now}: {mensaje}\n";

                // Escritura asíncrona en el archivo físico.
                await File.AppendAllTextAsync(rutaBitacora, contenido);
            }
            catch
            {
                // Manejo de excepciones silencioso para garantizar la estabilidad del servicio.
            }
            finally
            {
                // Liberación obligatoria del semáforo para permitir el paso a otros hilos en cola.
                Semaforo.Release();
            }
        }
    }
}