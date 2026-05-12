namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a menu item that contains identification, display name, icon, and navigation URL information for use
    /// in user interface components.
    /// </summary>
    /// <remarks>Use this record to encapsulate the details of a menu option when building navigation
    /// structures or rendering menus in applications. Each property provides essential information for displaying and
    /// linking menu items within the UI.</remarks>
    public record MenuDTO
    {
        public int IdMenu { get; set; }

        public string? Nombre { get; set; }

        public string? Icono { get; set; }

        public string? Url { get; set; }
    }
}
