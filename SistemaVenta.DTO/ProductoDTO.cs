namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a product and its associated details, including name, category, stock, price, and active status, for
    /// use in data transfer operations.
    /// </summary>
    /// <remarks>This record is intended for transferring product information between layers of the
    /// application. All properties are nullable, indicating that specific product details may be unavailable or
    /// optional depending on the context.</remarks>
    public record ProductoDTO
    {
        public int IdProducto { get; set; }

        public string? Nombre { get; set; }

        public int? IdCategoria { get; set; }

        public string? DescripcionCategoria { get; set; }
        public string? UrlImagen { get; set; }

        public int? Stock { get; set; }

        public string? Precio { get; set; }

        public int? EsActivo { get; set; }
    }
}
