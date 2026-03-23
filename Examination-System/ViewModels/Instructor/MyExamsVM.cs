using Examination_System.Models.Query;
using Examination_System.ViewModels.Exam;
using ExamSys.Application.DTOs.Course;

namespace Examination_System.ViewModels.Instructor
{
    public class MyExamsVM
    {
        public List<ExamTableItemVM> Exams { get; set; }
        public List<CourseNameDto> AvailableCourses { get; set; }
        public ExamSearchFilter FilterInputs { get; set; }
    }
}
