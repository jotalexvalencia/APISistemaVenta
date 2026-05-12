namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a sales transaction, including its identifying information, payment details, total amount,
    /// registration date, and associated sale items.
    /// </summary>
    /// <remarks>The DetalleVenta collection is initialized to an empty list to ensure it is always ready for
    /// use and to prevent null reference exceptions when adding sale item details. This record is typically used to
    /// transfer sale data between application layers or services.</remarks>
    public record VentaDTO
    {
        public int IdVenta { get; set; }

        public string? NumeroDocumento { get; set; }

        public string? TipoPago { get; set; }

        public string? TotalTexto { get; set; }

        public string? FechaRegistro { get; set; }

        // Initialized collection to prevent null reference exceptions when adding items to the collection.
        public virtual ICollection<DetalleVentaDTO> DetalleVenta { get; set; } = new List<DetalleVentaDTO>();

    }
}
