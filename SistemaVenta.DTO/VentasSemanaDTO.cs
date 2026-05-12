using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents the sales data for a specific week, including the date and total sales amount.
    /// </summary>
    /// <remarks>Use this record to encapsulate weekly sales information for reporting or analytics purposes.
    /// The date should be provided in a consistent format to ensure accurate processing and comparison across
    /// records.</remarks>
    public record VentasSemanaDTO
    {
        public string? Fecha { get; set; }
        public int Total { get; set; }
    }
}
