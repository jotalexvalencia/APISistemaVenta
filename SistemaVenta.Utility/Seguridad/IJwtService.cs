using System;
using System.Collections.Generic;
using System.Text;
using SistemaVenta.Model;

namespace SistemaVenta.Utility.Seguridad
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
    }
}
