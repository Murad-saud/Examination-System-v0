using ExamSys.Core.Enums;

namespace Examination_System.ViewModels.Exam
{
    public class ExamTableItemVM
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public ExamStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseName { get; set; }
        //public List<CourseNameDto> InstructorCourses { get; set; }

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanViewResults { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsInProgress { get; set; }
        public string StatusMessage { get; set; }
    }
}
