using ManejoPresupuesto.Validaciones;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Required (ErrorMessage="El campo {0} no puede estar vacio")]
        //[StringLength (maximumLength: 50, MinimumLength = 3, ErrorMessage ="La Longitud del campo {0} debe ser entre {2} y {1}")]
        //[DisplayName ("nombre")]       
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }
    }
}
