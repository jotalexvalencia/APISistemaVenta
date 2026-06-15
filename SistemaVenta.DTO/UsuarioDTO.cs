namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates user information, including identification, contact details,
    /// role, and status.
    /// </summary>
    /// <remarks>This record is intended for transferring user data between application layers. The Clave
    /// property should only be set during user creation or update operations and must remain null when retrieving user
    /// data to ensure security.</remarks>
    public record UsuarioDTO
    {
        public int IdUsuario { get; set; }

        public string? NombreCompleto { get; set; }

        public string? Correo { get; set; }

        public int? IdRol { get; set; }

        public string? RolDescripcion { get; set; }

        public string? UrlFoto { get; set; }

        // Used for creation/update. Should be null when retrieving user data for security reasons.
        public string? Clave { get; set; } 

        public int? EsActivo { get; set; }
    }
}
