using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebApiVentas.Modelos;
using WebApiVentas.Utilizades;
using WebApiVentas.Servicios;

namespace WebApiVentas.Controladores
{
    // Clase base abstracta que implementa la lógica CRUD reutilizable.
    // Utiliza herencia para evitar duplicidad de código en los controladores específicos.
    public abstract class ProductoBaseControlador : ControllerBase
    {
        protected readonly string _cadenaConexion;

        // Propiedades protegidas que definen el comportamiento específico de cada hijo (Tabla y Mensaje).
        protected readonly string _nombreTabla;
        protected readonly string _mensajeSocket;

        public ProductoBaseControlador(IConfiguration config, string nombreTabla, string mensajeSocket)
        {
            _cadenaConexion = config.GetConnectionString("CadenaSQL");
            _nombreTabla = nombreTabla;
            _mensajeSocket = mensajeSocket;
        }

        // Endpoint GET genérico
        [HttpGet]
        public IActionResult Obtener()
        {
            List<Producto> lista = new List<Producto>();
            try
            {
                using (SqlConnection con = new SqlConnection(_cadenaConexion))
                {
                    con.Open();
                    // Construcción dinámica de la consulta SQL basada en la tabla inyectada.
                    string query = $"SELECT * FROM {_nombreTabla}";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Producto
                        {
                            id = Convert.ToInt32(reader["Id"]),
                            nombre = reader["Nombre"].ToString(),
                            precio = Convert.ToDecimal(reader["Precio"]),
                            stock = Convert.ToInt32(reader["Stock"])
                        });
                    }
                }
                return Ok(lista);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // Endpoint POST para inserción de datos
        [HttpPost]
        public IActionResult Guardar([FromBody] Producto item)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_cadenaConexion))
                {
                    con.Open();
                    string query = $"INSERT INTO {_nombreTabla} (Nombre, Precio, Stock) VALUES (@n, @p, @s)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@n", item.nombre);
                    cmd.Parameters.AddWithValue("@p", item.precio);
                    cmd.Parameters.AddWithValue("@s", 0);
                    cmd.ExecuteNonQuery();
                }

                // Ejecución de tareas en segundo plano (Hilos):
                // 1. Registro en bitácora (Log).
                Task.Run(() => GestorTareas.RegistrarOperacion($"Se agregó {item.nombre} a {_nombreTabla}"));

                // 2. Notificación en tiempo real vía Sockets a los clientes conectados.
                ServiciosSockets.NotificarCambios(_mensajeSocket);

                return Ok(new { mensaje = "Guardado" });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut]
        public IActionResult Actualizar([FromBody] Producto item)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_cadenaConexion))
                {
                    con.Open();
                    string query = $"UPDATE {_nombreTabla} SET Precio = @p WHERE Nombre = @n";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@n", item.nombre);
                    cmd.Parameters.AddWithValue("@p", item.precio);
                    cmd.ExecuteNonQuery();
                }

                Task.Run(() => GestorTareas.RegistrarOperacion($"Precio actualizado en {_nombreTabla}: {item.nombre}"));

                // Notificación de actualización para refrescar las interfaces de usuario remotas.
                ServiciosSockets.NotificarCambios(_mensajeSocket);

                return Ok(new { mensaje = "Actualizado" });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{nombre}")]
        public IActionResult Eliminar(string nombre)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_cadenaConexion))
                {
                    con.Open();
                    string query = $"DELETE FROM {_nombreTabla} WHERE Nombre = @n";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@n", nombre);
                    cmd.ExecuteNonQuery();
                }

                Task.Run(() => GestorTareas.RegistrarOperacion($"Eliminado de {_nombreTabla}: {nombre}"));
                ServiciosSockets.NotificarCambios(_mensajeSocket);

                return Ok(new { mensaje = "Eliminado" });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}