using System.ComponentModel.DataAnnotations;

namespace Presupuestos.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool Recuerdame { get; set; }
    }
}
