using Examination_System.Models;
using ExamSys.Application.DTOs.User;

namespace Examination_System.ViewModels.Admin
{
    public class CreateCourseVM
    {
        public List<UserNameDto>? Instructors { get; set; }
        public CourseFormModel FormInputs { get; set; }
    }
}
