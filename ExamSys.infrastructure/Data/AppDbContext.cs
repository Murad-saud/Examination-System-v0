using ExamSys.Core.Entities;
using ExamSys.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Tables
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<ParticipantExam> ParticipantExam { get; set; }
        public DbSet<ParticipantAnswer> ParticipantAnswers { get; set; }
        public DbSet<Course> Courses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.CorrectAnswer)
                .WithOne()
                .HasForeignKey<Question>(q => q.CorrectAnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tell EF Core that IUser maps to ApplicationUser
            modelBuilder.Entity<Course>()
                .HasOne<ApplicationUser>("Instructor")  // Use string for shadow property
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.InstructorId);

            modelBuilder.Entity<Exam>()
                .HasOne<ApplicationUser>("Instructor")
                .WithMany(u => u.Exams)
                .HasForeignKey(e => e.InstructorId);

            modelBuilder.Entity<ParticipantExam>()
                .HasOne<ApplicationUser>("Participant")  // Use string for shadow property
                .WithMany(u => u.ParticipantExams)
                .HasForeignKey(c => c.ParticipantId);
        }

    }

}
