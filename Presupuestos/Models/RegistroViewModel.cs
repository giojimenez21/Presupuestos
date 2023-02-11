using System.ComponentModel.DataAnnotations;

namespace Presupuestos.Models
{
    public class RegistroViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
