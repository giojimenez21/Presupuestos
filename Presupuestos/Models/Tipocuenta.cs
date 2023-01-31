using Microsoft.AspNetCore.Mvc;
using Presupuestos.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace Presupuestos.Models
{
    public class Tipocuenta //: IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1}")]
        [Display(Name = "Nombre del tipo cuenta")]
        [Remote(action: "VerificarExisteTipoCuenta", controller:"TiposCuentas")]
        //[PrimeraMayuscula]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        // Nivel modelo
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Nombre != null && Nombre.Length > 0)
        //    {
        //        var primeraLetra = Nombre[0].ToString();
        //        if (primeraLetra != primeraLetra.ToUpper())
        //        {
        //            yield return new ValidationResult("Primera debe ser mayuscula", new[] { nameof(Nombre) });
        //        }
        //    }
        //}
    }
}
