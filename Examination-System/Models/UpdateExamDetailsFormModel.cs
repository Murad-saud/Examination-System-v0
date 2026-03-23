using System.ComponentModel.DataAnnotations;

namespace Examination_System.Models
{
    public class UpdateExamDetailsFormModel
    {
        public int ExamId { get; set; }
        [Required(ErrorMessage = "Exam title is required")]
        public string ExamTitle { get; set; }
        [Required(ErrorMessage = "Assiging course is required")]
        public int CourseId { get; set; }
        [Required(ErrorMessage = "Exam's start date is required")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Exam's duration is required")]
        public int Duration { get; set; }
    }
}
