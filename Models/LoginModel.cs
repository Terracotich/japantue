using System.ComponentModel.DataAnnotations;

namespace japantune.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "No login")]

        public string Login {  get; set; }

        [Required(ErrorMessage = "No password")]
        [DataType(DataType.Password)]

        public string Password { get; set; }   
    }
}
