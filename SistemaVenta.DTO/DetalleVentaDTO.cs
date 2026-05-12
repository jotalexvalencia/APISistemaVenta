namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents the details of a sale transaction, including product information and pricing.
    /// </summary>
    /// <remarks>This record is used to encapsulate the information related to a specific product in a sale,
    /// such as its ID, description, quantity, and pricing details. The properties are nullable to accommodate scenarios
    /// where certain details may not be available.</remarks>
    public record DetalleVentaDTO
    {
        public int? IdProducto { get; set; }

        public string? DescripcionProducto { get; set; }


        public int? Cantidad { get; set; }

        public string? PrecioTexto { get; set; }

        public string? TotalTexto { get; set; }
    }
}
