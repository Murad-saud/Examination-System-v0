namespace ExamSys.Core.ResponseModels.Exam
{
    public class CorrectionResult
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
        public int? CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
        public int Weight { get; set; }
    }
}
