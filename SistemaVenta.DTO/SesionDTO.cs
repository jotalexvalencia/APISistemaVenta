using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DTO
{
    public class SesionDTO
    {
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public string? RolDescripcion { get; set; }

        // NUEVO: Aquí es donde el Backend devolverá el Token JWT 
        public string? Token { get; set; } // JWT (1 hora)
        public string? RefreshToken { get; set; } // Refresh (7 días) <--- NUEVO
    }
}
