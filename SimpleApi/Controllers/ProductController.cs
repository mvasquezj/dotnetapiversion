using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SimpleApi.Request;
using SimpleApi.Response;

namespace SimpleApi.Controllers
{
    /// <summary>
    /// Product API
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        /// <summary>
        /// Get Products
        /// </summary>
        /// <returns>
        /// </returns>
        /// <remarks>
        /// Aquí puedes agregar detalles extra sobre el proceso.
        /// </remarks>
        /// <response code="200">Usuario creado exitosamente.</response>
        /// <response code="409">
        /// Conflicto de negocio detectado.  
        /// Posibles validaciones:  
        /// - **USER_EXISTS**: El correo ya está en uso.  
        /// - **INVALID_DOMAIN**: El dominio del correo está bloqueado.
        /// </response>
        // GET: api/<ProductController>
        [HttpGet]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(BusinessError), 409)]
        public IEnumerable<string> Get()
        {
            return ["value1", "value2"];
        }
        
        /// <summary>
        /// Add Product
        /// </summary>
        /// <param name="request"></param>
        // POST api/<ProductController>
        [HttpPost]
        public void Post([FromBody] ProductRequest request)
        {
        }
    }
}
