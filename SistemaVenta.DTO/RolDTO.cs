namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates the identifier and name of a role.
    /// </summary>
    /// <remarks>Use this record to transfer role information between application layers, such as between the
    /// data access and presentation layers. This type is typically used in scenarios where only the essential role data
    /// is required, without exposing domain or persistence details.</remarks>
    public record RolDTO
    {
        public int IdRol { get; init; }

        public string? Nombre { get; init; }
    }
}
