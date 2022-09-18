using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var priemraLetra= value.ToString()[0].ToString();

            if(priemraLetra != priemraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debo ser mayuscula");
            }

            return ValidationResult.Success;
        }
    }
}
