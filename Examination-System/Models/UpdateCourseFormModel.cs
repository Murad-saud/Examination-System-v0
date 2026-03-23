using System.ComponentModel.DataAnnotations;

namespace Examination_System.Models
{
    public class UpdateCourseFormModel
    {
        public int CourseId { get; set; }
        [Required(ErrorMessage = "The course name is required")]
        public string CourseName { get; set; }
        [Required(ErrorMessage = "The course instructor must be assigned")]
        public string InstructorId { get; set; }
    }
}
