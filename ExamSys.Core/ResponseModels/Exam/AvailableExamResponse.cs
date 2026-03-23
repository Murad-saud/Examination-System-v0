using ExamSys.Core.Enums;

namespace ExamSys.Core.ResponseModels.Exam
{
    public class AvailableExamResponse
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
