using Examination_System.Models;
using ExamSys.Application.DTOs.Course;

namespace Examination_System.ViewModels.Exam
{
    public class CreateExamVM
    {
        public List<CourseNameDto> AvailableCourses { get; set; }
        public CreateExamFormModel FormInputs { get; set; }
    }
}
