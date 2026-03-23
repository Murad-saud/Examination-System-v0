using ExamSys.Core.Enums;

namespace ExamSys.Application.DTOs.Exam
{
    public class AvailableExamDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public ExamStatus ExamStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
    }
}
