
namespace HeroGame.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
