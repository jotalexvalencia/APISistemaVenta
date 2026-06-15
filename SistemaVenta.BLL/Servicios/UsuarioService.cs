using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;
using SistemaVenta.Utility.Seguridad; // Para IJwtService

namespace SistemaVenta.BLL.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepositorio; // <--- Nuevo

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio,
            IGenericRepository<RefreshToken> refreshTokenRepositorio, // <--- Nuevo
            IMapper mapper, IJwtService jwtService)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _refreshTokenRepositorio = refreshTokenRepositorio; // <--- Nuevo
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<List<UsuarioDTO>> Lista()
        {
            try
            {
                var queryUsuario = await _usuarioRepositorio.Consultar();
                var listaUsuarios = queryUsuario.Include(rol => rol.IdRolNavigation).ToList();

                // Mapeamos
                var listaDto = _mapper.Map<List<UsuarioDTO>>(listaUsuarios);

                // SEGURIDAD: Limpiamos la clave para que no viaje en la lista
                foreach (var item in listaDto)
                {
                    item.Clave = null;
                }

                return listaDto;
            }
            catch { throw; }
        }

        public async Task<SesionDTO> ValidarCredenciales(string? correo, string? clave)
        {
            try
            {
                // 1. Buscamos SOLO por correo
                var queryUsuario = await _usuarioRepositorio.Consultar(u => u.Correo == correo);
                var usuarioEncontrado = queryUsuario.Include(rol => rol.IdRolNavigation).FirstOrDefault();

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                // 2. Verificamos la clave con BCrypt
                bool claveValida = BCrypt.Net.BCrypt.Verify(clave, usuarioEncontrado.Clave);

                if (!claveValida)
                    throw new TaskCanceledException("La contraseña es incorrecta");

                // 3. Generar Token JWT
                var sesion = _mapper.Map<SesionDTO>(usuarioEncontrado);
                sesion.Token = _jwtService.GenerateToken(usuarioEncontrado);

                // 4. Generar Refresh Token 
                var refreshToken = new RefreshToken
                {
                    IdUsuario = usuarioEncontrado.IdUsuario,
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                    FechaExpiracion = DateTime.Now.AddDays(7),
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                // --- BLOQUE DE DIAGNÓSTICO ---
                try
                {
                    await _refreshTokenRepositorio.Crear(refreshToken);
                }
                catch (Exception dbEx)
                {
                    // Esto imprimirá el error REAL en la ventana "Salida" (Output) de Visual Studio
                    System.Diagnostics.Debug.WriteLine("=== ERROR DB INTERNO ===");
                    System.Diagnostics.Debug.WriteLine(dbEx.InnerException?.Message ?? dbEx.Message);
                    System.Diagnostics.Debug.WriteLine("========================");

                    // Lanzamos un error más claro para Scalar
                    throw new TaskCanceledException("Error al guardar Refresh Token: " + (dbEx.InnerException?.Message ?? dbEx.Message));
                }
                // -----------------------------

                sesion.RefreshToken = refreshToken.Token;
                return sesion;
            }
            catch { throw; }
        }

        public async Task<SesionDTO> RenovarToken(string? refreshToken)
        {
            try
            {
                // 1. Buscar el token en BD
                var tokenEncontrado = await _refreshTokenRepositorio.Obtener(t => t.Token == refreshToken && t.Activo == true);

                if (tokenEncontrado == null)
                    throw new TaskCanceledException("Token inválido o expirado");

                if (tokenEncontrado.FechaExpiracion < DateTime.Now)
                {
                    tokenEncontrado.Activo = false;
                    await _refreshTokenRepositorio.Editar(tokenEncontrado);
                    throw new TaskCanceledException("Token expirado");
                }

                // 2. Obtener el usuario asociado (CORREGIDO: Incluimos el Rol)
                // Usamos Consultar + Include para traer la relación
                var queryUsuario = await _usuarioRepositorio.Consultar(u => u.IdUsuario == tokenEncontrado.IdUsuario);
                var usuario = queryUsuario.Include(r => r.IdRolNavigation).FirstOrDefault();

                if (usuario == null) throw new TaskCanceledException("Usuario no encontrado");

                // 3. Generar NUEVO JWT
                var nuevoJwt = _jwtService.GenerateToken(usuario);

                // 4. Invalidar el Refresh Token viejo (Seguridad: One-time use)
                tokenEncontrado.Activo = false;
                await _refreshTokenRepositorio.Editar(tokenEncontrado);

                // 5. Generar NUEVO Refresh Token (Rotación)
                var nuevoRefreshToken = new RefreshToken
                {
                    IdUsuario = usuario.IdUsuario,
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                    FechaExpiracion = DateTime.Now.AddDays(7),
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };
                await _refreshTokenRepositorio.Crear(nuevoRefreshToken);

                // 6. Devolver respuesta (CORREGIDO: Agregamos IdUsuario y RolDescripcion)
                return new SesionDTO
                {
                    IdUsuario = usuario.IdUsuario, // <--- Agregado
                    Token = nuevoJwt,
                    RefreshToken = nuevoRefreshToken.Token,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Correo,
                    RolDescripcion = usuario.IdRolNavigation != null ? usuario.IdRolNavigation.Nombre : "Sin Rol" // <--- Agregado
                };
            }
            catch { throw; }
        }
        public async Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
            try
            {
                // 1. Validación Clean Architecture: ¿Existe el correo?
                var usuarioExistente = await _usuarioRepositorio.Obtener(u => u.Correo == modelo.Correo);
                if (usuarioExistente != null)
                    throw new TaskCanceledException("El correo ya está registrado.");

                // 2. Mapear y CIFRAR CLAVE (Regla de Negocio)
                var usuarioModelo = _mapper.Map<Usuario>(modelo);

                // AQUÍ ESTÁ LA MAGIA: Hasheamos la clave antes de tocar la BD
                usuarioModelo.Clave = BCrypt.Net.BCrypt.HashPassword(usuarioModelo.Clave);

                // 3. Guardar
                var usuarioCreado = await _usuarioRepositorio.Crear(usuarioModelo);

                if (usuarioCreado.IdUsuario == 0)
                    throw new TaskCanceledException("No se pudo crear el usuario");

                var query = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);
            }
            catch { throw; }
        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                // Lógica de actualización
                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.IdRol = usuarioModelo.IdRol;
                usuarioEncontrado.EsActivo = usuarioModelo.EsActivo;
                usuarioEncontrado.UrlFoto = usuarioModelo.UrlFoto;

                // LÓGICA DE NEGOCIO: Si viene una clave nueva, la ciframos. Si no, dejamos la anterior.
                // (Validación simple: si la clave es diferente de vacío/nulo, se actualiza)
                if (!string.IsNullOrEmpty(usuarioModelo.Clave))
                {
                    usuarioEncontrado.Clave = BCrypt.Net.BCrypt.HashPassword(usuarioModelo.Clave);
                }

                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);
                if (!respuesta) throw new TaskCanceledException("No se pudo editar");

                return respuesta;
            }
            catch { throw; }
        }

        public async Task<bool> ActualizarFoto(int idUsuario, string url)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == idUsuario);
                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                usuarioEncontrado.UrlFoto = url;
                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo actualizar la foto");
                return respuesta;
            }
            catch { throw; }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);
                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");

                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);
                if (!respuesta) throw new TaskCanceledException("No se pudo eliminar");

                return respuesta;
            }
            catch { throw; }
        }
    }
}