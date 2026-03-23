using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Core.ResponseModels.Course;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CourseInstructorResponse>> GetAllCoursesDetails()
        {
            var result = from c in _context.Courses
                         join u in _context.Users
                         on c.InstructorId equals u.Id
                         select new CourseInstructorResponse
                         {
                             CourseId = c.Id,
                             CourseName = c.Name,
                             CourseDateCreated = c.DateCreated,
                             InstructorId = u.Id,
                             InstructorName = u.FullName
                         };

            return await result.ToListAsync();
        }

        public async Task<List<Course>> GetAllInstructorCourses(string instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<CourseWithInstructorResponse?> GetCourseWithInstructorAsync(int courseId)
        {
            return await _context.Courses
                .Where(c => c.Id == courseId)
                .Join(_context.Users,
                    course => course.InstructorId,
                    user => user.Id,
                    (course, user) => new CourseWithInstructorResponse
                    {
                        CourseId = course.Id,
                        CourseName = course.Name,
                        InstructorId = user.Id,
                        InstructorName = user.FullName,
                        DateCreated = course.DateCreated
                    })
                .FirstOrDefaultAsync();
        }
    }
}
