namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates user login credentials, including the email address and
    /// password.
    /// </summary>
    /// <remarks>Both the email and password properties are optional and may be null. This record is typically
    /// used to transmit authentication information between client and server during login operations.</remarks>
    public record LoginDTO
    {
        public string? Correo { get; init; }
        public string? Clave { get; init; }
    }
}
