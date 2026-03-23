using ExamSys.Core.Enums;

namespace ExamSys.Core.ResponseModels.Exam
{
    public class ExamDetailsResponse
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public ExamStatus ExamStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public string InstructorId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int QuestionsCount { get; set; }
    }
}
