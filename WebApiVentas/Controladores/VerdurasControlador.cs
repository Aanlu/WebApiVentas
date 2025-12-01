using Microsoft.AspNetCore.Mvc;

namespace WebApiVentas.Controladores
{
       [Route("api/Verduras")]
       [ApiController]
       public class VerdurasControlador : ProductoBaseControlador
        {
           public VerdurasControlador(IConfiguration config)
               : base(config, "Verduras", "UPDATE_VERDURAS")
           {
           }
        }
}
