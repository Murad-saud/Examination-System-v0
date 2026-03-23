using ExamSys.Core.Interfaces;

namespace ExamSys.Core.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InstructorId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        //Navigation properties
        public virtual IUser Instructor { get; set; }
    }
}
