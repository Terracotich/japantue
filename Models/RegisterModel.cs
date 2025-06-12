using System.ComponentModel.DataAnnotations;

namespace japantune.Models
{
    public class RegisterModel : LoginModel
    {
        [Required(ErrorMessage = "Повторите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        public string SurName { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }
    }
}