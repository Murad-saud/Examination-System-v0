using Examination_System.Models;
using ExamSys.Application.DTOs.User;

namespace Examination_System.ViewModels.Course
{
    public class UpdateCourseVM
    {
        public UpdateCourseFormModel FormInputs { get; set; }
        public UserNameDto CourseInstructor { get; set; }
        public List<UserNameDto> AllInstructors { get; set; }
    }
}
