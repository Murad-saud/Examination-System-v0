namespace ExamSys.Application.DTOs.ExamTaking
{
    public class SaveAnswersRequest
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }
}
