using System.ComponentModel.DataAnnotations;

namespace Examination_System.ViewModels.Question
{
    public class CreateQuestionFormModel
    {
        public int ExamId { get; set; }
        [Required(ErrorMessage = "Please provide a {0}.")]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }
        public List<string> AnswersText { get; set; }
        [Required(ErrorMessage = "Cannot proceed without assigning {0}.")]
        [Range(1, int.MaxValue, ErrorMessage = "Cannot proceed without a {0}.")]
        [Display(Name = "Correct Answer")]
        public int IndexOfCorrectAnswer { get; set; }
        [Required(ErrorMessage = "Please provide a {0}.")]
        [Range(1, int.MaxValue, ErrorMessage = "Weight should be more than or equal to 1.")]
        [Display(Name = "Question Weight")]
        public int QuestionWeight { get; set; }
    }
}
