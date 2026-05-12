namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object that provides aggregated sales and product information for a dashboard view.
    /// </summary>
    /// <remarks>This record includes properties for total sales, total income, total products, and a
    /// collection of weekly sales data. The 'VentasUltimaSemana' property is initialized to an empty collection to
    /// prevent null reference exceptions when accessing its members.</remarks>
    public record DashBoardDTO
    {
        public int TotalVentas { get; set; }
        public string? TotalIngresos { get; set; }

        public int TotalProductos { get; set; }
        // Initialized to prevent null reference exceptions
        public ICollection<VentasSemanaDTO> VentasUltimaSemana { get; set; } = new List<VentasSemanaDTO>();
    }
}
