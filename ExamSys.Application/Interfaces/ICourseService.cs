using ExamSys.Application.Common;
using ExamSys.Application.DTOs.Course;

namespace ExamSys.Application.Interfaces
{
    public interface ICourseService
    {
        Task<Result> CreateCourseAsync(CreateCourseDto createCourseDto);
        Task<List<CourseTableItemDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int courseId);
        Task<CourseTableItemDto> GetCourseWithInstructorAsync(int courseId);
        Task<bool> UpdateCourse(UpdateCourseDto updateCourseDto);
        Task<bool> RemoveCourse(int courseId);
        Task<List<CourseNameDto>> GetInstructorCourses(string instructorId);
    }
}
