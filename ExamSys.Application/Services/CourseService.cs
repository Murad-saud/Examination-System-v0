using ExamSys.Application.Common;
using ExamSys.Application.DTOs.Course;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces;

namespace ExamSys.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        public CourseService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<Result> CreateCourseAsync(CreateCourseDto createCourseDto)
        {
            try
            {
                var newCourse = new Course()
                {
                    Name = createCourseDto.CourseName,
                    InstructorId = createCourseDto.InstructorId,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                };

                await _unitOfWork.Courses.AddAsync(newCourse);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Log the exception here
                return Result.Failure("Failed to create course. Please try again.");
            }
        }

        public async Task<List<CourseTableItemDto>> GetAllCoursesAsync()
        {
            var coursesDb = await _unitOfWork.Courses.GetAllCoursesDetails();
            return coursesDb.Select(c => new CourseTableItemDto()
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseDateCreated = c.CourseDateCreated,
                InstructorId = c.InstructorId,
                InstructorName = c.InstructorName
            }).ToList();
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            var courseDb = await _unitOfWork.Courses.GetByIdAsync(courseId);
            var result = courseDb != null ? new CourseDto()
            {
                Id = courseDb.Id,
                CourseName = courseDb.Name,
                InstuctorId = courseDb.InstructorId
            } : null;

            return result;
        }

        public async Task<CourseTableItemDto?> GetCourseWithInstructorAsync(int courseId)
        {
            var courseDb = await _unitOfWork.Courses.GetCourseWithInstructorAsync(courseId);

            var result = courseDb != null ? new CourseTableItemDto()
            {
                CourseId = courseDb.CourseId,
                CourseName = courseDb.CourseName,
                CourseDateCreated = courseDb.DateCreated,
                InstructorId = courseDb.InstructorId,
                InstructorName = courseDb.InstructorName
            } : null;

            return result;
        }

        public async Task<List<CourseNameDto>> GetInstructorCourses(string instructorId)
        {
            try
            {
                var result = await _unitOfWork.Courses.GetAllInstructorCourses(instructorId);
                return result.Select(c => new CourseNameDto()
                {
                    CourseId = c.Id,
                    CourseName = c.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                return new List<CourseNameDto>();
            }

        }

        public async Task<bool> RemoveCourse(int courseId)
        {
            var courseDb = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (courseDb is null) return false;

            try
            {
                _unitOfWork.Courses.Remove(courseDb);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCourse(UpdateCourseDto updateCourseDto)
        {
            var courseDb = await _unitOfWork.Courses.GetByIdAsync(updateCourseDto.CourseId);
            if (courseDb is null) return false;

            courseDb.Name = updateCourseDto.CourseName;
            courseDb.InstructorId = updateCourseDto.InstructorId;
            courseDb.DateUpdated = DateTime.UtcNow;

            try
            {
                _unitOfWork.Courses.Update(courseDb);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
