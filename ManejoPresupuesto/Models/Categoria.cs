using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Este campo {0} es requerido")]
        [StringLength(maximumLength:50, ErrorMessage ="Este campo no puede tener mas caracteres de {1}")]
        public string Nombre { get; set; }

        public TipoOperacion TipoOperacion { get; set; }

        public int UsuarioId { get; set; }

    }
}
