using ExamSys.Core.Enums;
using ExamSys.Core.Interfaces;

namespace ExamSys.Core.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public ExamStatus Status { get; set; }
        public string InstructorId { get; set; }
        public int CourseId { get; set; }

        //Navigation Properties
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public Course Course { get; set; }
        public virtual IUser Instructor { get; set; }
    }
}
