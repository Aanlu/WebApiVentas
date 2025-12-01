using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebApiVentas.Modelos;
using WebApiVentas.Utilizades;
using System.Threading.Tasks;

namespace WebApiVentas.Controladores
{
    [Route("api/Login")]
    [ApiController]
    public class LoginControlador : ControllerBase
    {
        private readonly string _cadenaConexion;

        public LoginControlador(IConfiguration config)
        {
            _cadenaConexion = config.GetConnectionString("CadenaSQL");
        }

        [HttpPost]
        public async Task<IActionResult> Validar([FromBody] Usuario datos)
        {
            try
            {
                // Uso de conexión asíncrona (await OpenAsync) para mejorar la escalabilidad del servidor.
                await using var con = new SqlConnection(_cadenaConexion);
                await con.OpenAsync();

                const string query = "SELECT Rol FROM Usuarios WHERE NombreUsuario = @u AND Password = @p";

                await using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@u", datos.NombreUsuario);
                cmd.Parameters.AddWithValue("@p", datos.Password);

                var resultado = await cmd.ExecuteScalarAsync();

                if (resultado != null)
                {
                    var rolDetectado = resultado.ToString();

                    // Registro de auditoría en hilo separado (Fire-and-forget).
                    _ = Task.Run(() => GestorTareas.RegistrarOperacion($"Login exitoso: {datos.NombreUsuario}"));

                    return Ok(new { autorizado = true, rol = rolDetectado });
                }

                _ = Task.Run(() => GestorTareas.RegistrarOperacion($"Login fallido: {datos.NombreUsuario}"));
                return Unauthorized(new { autorizado = false });
            }
            catch (Exception ex)
            {
                return BadRequest("Error de servidor.");
            }
        }

        // Endpoint para registro de nuevos usuarios
        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario datos)
        {
            try
            {
                await using var con = new SqlConnection(_cadenaConexion);
                await con.OpenAsync();

                // Validación de existencia previa
                string checkQuery = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @u";
                await using var cmdCheck = new SqlCommand(checkQuery, con);
                cmdCheck.Parameters.AddWithValue("@u", datos.NombreUsuario);
                int count = (int)await cmdCheck.ExecuteScalarAsync();

                if (count > 0) return BadRequest("El usuario ya existe.");

                // Inserción del nuevo usuario
                string insertQuery = "INSERT INTO Usuarios (NombreUsuario, Password, Rol) VALUES (@u, @p, 'Vendedor')";

                await using var cmd = new SqlCommand(insertQuery, con);
                cmd.Parameters.AddWithValue("@u", datos.NombreUsuario);
                cmd.Parameters.AddWithValue("@p", datos.Password);

                await cmd.ExecuteNonQueryAsync();

                _ = Task.Run(() => GestorTareas.RegistrarOperacion($"Nuevo usuario registrado: {datos.NombreUsuario}"));

                return Ok(new { mensaje = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest("Error al registrar: " + ex.Message);
            }
        }
    }
}