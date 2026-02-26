using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates user session information, including user identifier, full
    /// name, email address, and role description.
    /// </summary>
    /// <remarks>This class is typically used to transfer session-related user details between application
    /// layers, such as from authentication services to client interfaces. It is intended for scenarios where user
    /// identity and role information are required to manage access or personalize user experience.</remarks>
    public class SesionDTO
    {
        public int IdUsuario { get; set; }

        public string? NombreCompleto { get; set; }

        public string? Correo { get; set; }

        public string? RolDescripcion { get; set; }
    }
}
