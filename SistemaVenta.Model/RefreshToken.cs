using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVenta.Model
{
    [Table("RefreshToken")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Token { get; set; }

        public int IdUsuario { get; set; } // <--- Esta es tu columna FK real

        public DateTime FechaExpiracion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        
        [ForeignKey("IdUsuario")] 
        public virtual Usuario? IdUsuarioNavigation { get; set; }
    }
}