using ExamSys.Core.Enums;

namespace ExamSys.Core.ResponseModels.Exam
{
    public class ExamInfoResponse
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public ExamStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseName { get; set; }
    }
}
