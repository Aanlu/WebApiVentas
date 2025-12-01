using Microsoft.AspNetCore.Mvc;

namespace WebApiVentas.Controladores
{
    // Controlador específico para la entidad Frutas.
    // Define la ruta base HTTP y hereda toda la funcionalidad de ProductoBaseControlador.
    [Route("api/Frutas")]
    [ApiController]
    public class FrutasControlador : ProductoBaseControlador
    {
        // Constructor que inyecta la configuración al padre:
        // - Nombre de la tabla SQL: "Frutas"
        // - Mensaje de protocolo Socket: "UPDATE_FRUTAS"
        public FrutasControlador(IConfiguration config)
            : base(config, "Frutas", "UPDATE_FRUTAS")
        {
        }
    }
}