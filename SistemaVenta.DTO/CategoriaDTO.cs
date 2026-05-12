using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DTO
{
    /// <summary>
    /// Represents a data transfer object for a category, containing its unique identifier and name.
    /// </summary>
    /// <remarks>Use this record to transfer category information between application layers. The 'Nombre'
    /// property may be null if the category does not have a name assigned.</remarks>
    public record CategoriaDTO
    {
        public int IdCategoria { get; init; }

        public string? Nombre { get; init; }
    }
}
