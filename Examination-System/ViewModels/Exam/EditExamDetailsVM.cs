using Examination_System.Models;
using ExamSys.Application.DTOs.Course;

namespace Examination_System.ViewModels.Exam
{
    public class EditExamDetailsVM
    {
        public UpdateExamDetailsFormModel FormInputs { get; set; }
        public List<CourseNameDto> AvailableCourses { get; set; }
    }
}
