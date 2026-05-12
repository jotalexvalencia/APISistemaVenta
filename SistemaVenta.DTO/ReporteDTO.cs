namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that encapsulates the details of a sales transaction report, including
    /// document number, payment type, registration date, total sale amount, product information, quantity, price, and
    /// total per item.
    /// </summary>
    /// <remarks>This record is intended for use in scenarios where transaction report data needs to be
    /// transferred between application layers or external systems. It provides a structured format for reporting and
    /// analyzing sales transactions, facilitating integration and data manipulation in sales management
    /// solutions.</remarks>
    public record ReporteDTO
    {
        public string? NumeroDocumento { get; set; }
        public string? TipoPago { get; set; }
        public string? FechaRegistro { get; set; }
        public string? TotalVenta { get; set; }
        public string? Producto { get; set; }
        public int? Cantidad { get; set; }
        public string? Precio { get; set; }
        public string? Total { get; set; }
        



    }
}
