namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates user session information, including identification details
    /// and authentication tokens.
    /// </summary>
    /// <remarks>This record is typically used to convey session-related data between application layers, such
    /// as after a successful login. It includes both a short-lived JWT token for authentication and a long-lived
    /// refresh token for obtaining new access tokens. The properties provide essential user details required for
    /// session management and authorization.</remarks>
    public record SesionDTO
    {
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public string? RolDescripcion { get; set; }

        // JWT Token (Short-lived, e.g., 1 hour) 
        public string? Token { get; set; }
        // Refresh Token (Long-lived, e.g., 7 days)
        public string? RefreshToken { get; set; } 
    }
}
