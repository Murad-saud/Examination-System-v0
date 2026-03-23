using System.ComponentModel.DataAnnotations;

namespace Examination_System.ViewModels
{
    public class EditUserVM
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "First Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "First Name length must be between 2 and 25 charecters")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Middle Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Middle Name length must be between 2 and 25 charecters")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last Name can't be blank")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Last Name length must be between 2 and 25 charecters")]
        public string LastName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string RoleName { get; set; }
    }
}
