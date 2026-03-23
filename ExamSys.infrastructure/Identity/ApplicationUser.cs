using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ExamSys.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser, IUser
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateCreated { get; set; }

        //Navigation Properties
        // FIX: Add { get; set; } and make them virtual
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public virtual ICollection<ParticipantExam> ParticipantExams { get; set; } = new List<ParticipantExam>();
    }

}