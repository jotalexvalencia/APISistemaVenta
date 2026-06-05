using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SistemaVenta.Model;

namespace SistemaVenta.Utility.Seguridad
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Usuario usuario)
        {
            // 1. Definir los claims (datos del usuario dentro del token)
            //    - Asegurarse de no pasar valores nulos al constructor de Claim
            //    - Usar coalescencia nula para strings que son nullable en el modelo
            var claims = new List<Claim>
            {
                // IdUsuario siempre convertido a string (no nulo)
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),

                // Correo puede ser null en el modelo; usar string.Empty para evitar CS8604
                new Claim(ClaimTypes.Name, usuario.Correo ?? string.Empty),

                // Rol: usar el nombre del rol si existe, si no, "Cliente".
                // Usar el operador ?. y ?? para evitar pasar null al Claim
                new Claim(ClaimTypes.Role, usuario.IdRolNavigation?.Nombre ?? "Cliente")
            };

            // 2. Configurar la llave secreta
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada. Verifica appsettings o variables de entorno.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Generar el Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            // 4. Retornar el string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
