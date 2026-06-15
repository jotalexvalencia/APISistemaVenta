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
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly IWebHostEnvironment _env;

        public UsuarioController(IUsuarioService usuarioServicio, IWebHostEnvironment env)
        {
            _usuarioServicio = usuarioServicio;
            _env = env;
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            var rsp = new Response<List<UsuarioDTO>>();

            try
            {
                rsp.status = true;
                rsp.Value = await _usuarioServicio.Lista();
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("IniciarSesion")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginDTO login)
        {
            var rsp = new Response<SesionDTO>();

            try
            {
                rsp.status = true;
                rsp.Value = await _usuarioServicio.ValidarCredenciales(login.Correo,login.Clave);
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
        public async Task<IActionResult> Guardar([FromBody] UsuarioDTO usuario)
        {
            var rsp = new Response<UsuarioDTO>();

            try
            {
                rsp.status = true;
                rsp.Value = await _usuarioServicio.Crear(usuario);
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
        public async Task<IActionResult> Editar([FromBody] UsuarioDTO usuario)
        {
            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.Value = await _usuarioServicio.Editar(usuario);
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpPost]
        [Route("SubirFoto/{idUsuario:int}")]
        public async Task<IActionResult> SubirFoto(int idUsuario, IFormFile archivo)
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
                string rutaCarpeta = Path.Combine(_env.WebRootPath, "imagenes", "usuarios");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                string url = $"/imagenes/usuarios/{nombreArchivo}";

                UsuarioDTO dto = new UsuarioDTO { IdUsuario = idUsuario, UrlFoto = url };
                await _usuarioServicio.Editar(dto);

                rsp.status = true;
                rsp.Value = url;
                rsp.msg = "Foto subida correctamente";
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
                rsp.Value = await _usuarioServicio.Eliminar(id);
            }
            catch (Exception ex)
            {

                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RenovarToken")]
        public async Task<IActionResult> RenovarToken([FromBody] string refreshToken)
        {
            var rsp = new Response<SesionDTO>();
            try
            {
                rsp.status = true;
                rsp.Value = await _usuarioServicio.RenovarToken(refreshToken);
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
