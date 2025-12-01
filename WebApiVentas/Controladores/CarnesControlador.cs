using Microsoft.AspNetCore.Mvc;

namespace WebApiVentas.Controladores
{
    [Route("api/Carnes")]
    [ApiController]
    // HERENCIA: Heredamos de ProductoBaseController
    public class CarnesControlador : ProductoBaseControlador
    {
        // En el constructor, pasamos los valores fijos al Padre (base)
        public CarnesControlador(IConfiguration config)
            : base(config, "Carnes", "UPDATE_CARNES")
        {
        }
    }
}