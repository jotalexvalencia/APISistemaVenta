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

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper, IJwtService jwtService)
        {
            _usuarioRepositorio = usuarioRepositorio;
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

        public async Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {
            try
            {
                // 1. Buscamos SOLO por correo (ya no filtramos por clave en la query)
                var queryUsuario = await _usuarioRepositorio.Consultar(u => u.Correo == correo);
                var usuarioEncontrado = queryUsuario.Include(rol => rol.IdRolNavigation).FirstOrDefault();

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                // 2. Verificamos la clave con BCrypt (Comparación segura)
                // Nota: La BD tiene claves viejas planas, mañana habrá que actualizarlas.
                // Por ahora, si la verificación falla, asumimos que es la clave plana antigua para compatibilidad temporal, 
                // PERO LO CORRECTO ES SOLO USAR Verify.

                bool claveValida = BCrypt.Net.BCrypt.Verify(clave, usuarioEncontrado.Clave);

                if (!claveValida)
                    throw new TaskCanceledException("La contraseña es incorrecta");

                // 3. Generar Token
                var sesion = _mapper.Map<SesionDTO>(usuarioEncontrado);
                sesion.Token = _jwtService.GenerateToken(usuarioEncontrado);

                return sesion;
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