using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.API.Utilidad;

namespace SistemaVenta.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoServicio;
        private readonly IWebHostEnvironment _env;

        public ProductoController(IProductoService productoServicio, IWebHostEnvironment env)
        {
            _productoServicio = productoServicio;
            _env = env;
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            var rsp = new Response<List<ProductoDTO>>();

            try
            {
                rsp.status = true;
                rsp.Value = await _productoServicio.Lista();
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] ProductoDTO producto)
        {
            var rsp = new Response<ProductoDTO>();

            try
            {
                rsp.status = true;
                rsp.Value = await _productoServicio.Crear(producto);
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] ProductoDTO producto)
        {
            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.Value = await _productoServicio.Editar(producto);
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpPost]
        [Route("SubirImagen/{idProducto:int}")]
        public async Task<IActionResult> SubirImagen(int idProducto, IFormFile archivo)
        {
            var rsp = new Response<string>();

            try
            {
                if (archivo == null || archivo.Length == 0)
                {
                    rsp.status = false;
                    rsp.msg = "No se recibió ningún archivo";
                    return Ok(rsp);
                }

                string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    rsp.status = false;
                    rsp.msg = "Tipo de archivo no permitido. Use jpg, jpeg, png, gif o webp";
                    return Ok(rsp);
                }

                string nombreArchivo = $"{Guid.NewGuid()}{extension}";
                string rutaCarpeta = Path.Combine(_env.WebRootPath, "imagenes", "productos");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                string url = $"/imagenes/productos/{nombreArchivo}";

                ProductoDTO dto = new ProductoDTO { IdProducto = idProducto, UrlImagen = url };
                await _productoServicio.Editar(dto);

                rsp.status = true;
                rsp.Value = url;
                rsp.msg = "Imagen subida correctamente";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.Value = await _productoServicio.Eliminar(id);
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }
    }
}
