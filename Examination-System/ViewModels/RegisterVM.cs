using System.ComponentModel.DataAnnotations;

namespace Examination_System.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "First Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "First Name length must be between 2 and 25 charecters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Middle Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Middle Name length must be between 2 and 25 charecters")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Last Name length must be between 2 and 25 charecters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAdress { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords didn't match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Choose your role")]
        public string RoleName { get; set; }
    }
}
