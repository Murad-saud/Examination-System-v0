using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Core.ResponseModels.Exam;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class ExamRepository : Repository<Exam>, IExamRepository
    {
        public ExamRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExamSummaryResponse>> GetAllByStatus(ExamStatus status)
        {
            return await _context.Exams
                .Where(e => e.Status == status)
                .Select(e => new ExamSummaryResponse()
                {
                    ExamId = e.Id,
                    ExamTitle = e.Title,
                    StartDate = e.StartDate,
                    Duration = e.Duration,
                    QuestionsCount = e.Questions.Count,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    InstructorId = e.InstructorId,
                    InstructorName = e.Instructor.FullName ?? "Unknown"
                }).ToListAsync();
        }

        public async Task<IEnumerable<AvailableExamResponse>> GetAvailableExams()
        {
            return await _context.Exams
                .Where(e => e.Status == ExamStatus.InProgress || e.Status == ExamStatus.Scheduled)
                .OrderBy(e => e.StartDate)
                .Select(e => new AvailableExamResponse()
                {
                    ExamId = e.Id,
                    ExamTitle = e.Title,
                    ExamStatus = e.Status,
                    StartDate = e.StartDate,
                    Duration = e.Duration,
                    CourseName = e.Course.Name,
                    InstructorName = e.Instructor.FullName
                })
                .AsNoTracking()
                .ToListAsync();

        }

        public async Task<ExamDetailsResponse?> GetExamByIdAsync(int examId)
        {
            var examResponse = await _context.Exams.Where(e => e.Id == examId)
                .Select(e => new ExamDetailsResponse()
                {
                    ExamId = e.Id,
                    ExamTitle = e.Title,
                    ExamStatus = e.Status,
                    StartDate = e.StartDate,
                    Duration = e.Duration,
                    InstructorId = e.InstructorId,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    QuestionsCount = e.Questions.Count
                })
                .AsNoTracking() // Improves performance for read-only operations <<## AI ##>>
                .FirstOrDefaultAsync();

            return examResponse;
        }

        public async Task<IEnumerable<Exam>> GetExamsToComplete()
        {
            return await _context.Exams
                .Where(e => e.Status == ExamStatus.Pending && e.StartDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetExamsToStart()
        {
            return await _context.Exams
                .Where(e => e.Status == ExamStatus.InProgress
                    && (e.StartDate + TimeSpan.FromMinutes(e.Duration)) <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetExamsToSync()
        {
            var currentTime = DateTime.Now;
            return await _context.Exams
                .Where(e =>
                    (e.Status == ExamStatus.Scheduled && e.StartDate <= currentTime)
                        ||
                    (e.Status == ExamStatus.InProgress && e.StartDate.AddMinutes(e.Duration) <= currentTime)
                )
                .ToListAsync();
        }

        public async Task<ExamWithQuestionsResponse> GetExamWithQuestions(int examId)
        {
            var result = await _context.Exams
                .Where(e => e.Id == examId)
                .Select(e => new ExamWithQuestionsResponse
                {
                    ExamId = e.Id,
                    InstructorId = e.InstructorId,
                    ExamTitle = e.Title,
                    StartDate = e.StartDate,
                    Status = e.Status,
                    CourseName = e.Course.Name,
                    ExamQuestions = e.Questions.Select(q => new QuestionWithCountResponse()
                    {
                        Id = q.Id,
                        ExamId = q.ExamId,
                        Text = q.Text,
                        Weight = q.Weight,
                        CorrectAnswerId = (int)q.CorrectAnswerId,
                        AnswersCount = q.Answers.Count

                        // Add other question properties you need
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<ExamInfoResponse>> GetFilteredExams(string InstructorId, int? examStatus,
            string? CourseName, string? SearchText)
        {
            if (string.IsNullOrEmpty(InstructorId))
                throw new ArgumentException("InstructorId cannot be null or empty");

            var filteredExams = _context.Exams.Where(e => e.InstructorId == InstructorId)
                .Include(e => e.Course).AsQueryable();

            if (examStatus.HasValue)
            {
                filteredExams = examStatus switch
                {
                    0 => filteredExams.Where(e => e.Status == ExamStatus.Draft),
                    1 => filteredExams.Where(e => e.Status == ExamStatus.Preparing),
                    2 => filteredExams.Where(e => e.Status == ExamStatus.Pending),
                    3 => filteredExams.Where(e => e.Status == ExamStatus.Scheduled),
                    4 => filteredExams.Where(e => e.Status == ExamStatus.Rejected),
                    5 => filteredExams.Where(e => e.Status == ExamStatus.Cancelled),
                    6 => filteredExams.Where(e => e.Status == ExamStatus.InProgress),
                    7 => filteredExams.Where(e => e.Status == ExamStatus.Completed),
                    _ => filteredExams
                };
            }

            if (!string.IsNullOrEmpty(CourseName))
            {
                filteredExams = filteredExams.Where(e => e.Course.Name.ToLower().Contains(CourseName.ToLower()));
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                filteredExams = filteredExams.Where(e => e.Title.ToLower().Contains(SearchText.ToLower()));
            }

            //var filteredExamsList = filteredExams.ToListAsync();
            var examsInfoResponse = await filteredExams.Select(e => new ExamInfoResponse()
            {
                ExamId = e.Id,
                ExamTitle = e.Title,
                StartDate = e.StartDate,
                Status = e.Status,
                CourseName = e.Course.Name
            })
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return examsInfoResponse;
        }

    }
}
