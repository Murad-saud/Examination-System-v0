using ExamSys.Application.DTOs.Course;

namespace Examination_System.ViewModels.Exam
{
    public class ExamManagementVM
    {
        public List<ExamTableItemVM> ExamTableItems { get; set; }
        public List<CourseNameDto> AvailableCourses { get; set; }
    }
}
