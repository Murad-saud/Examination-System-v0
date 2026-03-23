using ExamSys.Core.Entities;
using ExamSys.Core.ResponseModels.Course;

namespace ExamSys.Core.Interfaces.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<CourseInstructorResponse>> GetAllCoursesDetails();
        Task<CourseWithInstructorResponse?> GetCourseWithInstructorAsync(int courseId);
        Task<List<Course>> GetAllInstructorCourses(string instructorId);

    }
}
