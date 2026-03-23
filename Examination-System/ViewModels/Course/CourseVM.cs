using ExamSys.Application.DTOs.User;

namespace Examination_System.ViewModels.Course
{
    public class CourseVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InstructorDto Instructor { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
