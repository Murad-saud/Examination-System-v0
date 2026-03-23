using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Core.ResponseModels.Exam;

namespace ExamSys.Core.Interfaces.Repositories
{
    public interface IExamRepository : IRepository<Exam>
    {
        Task<ExamWithQuestionsResponse> GetExamWithQuestions(int examId);
        Task<List<ExamInfoResponse>> GetFilteredExams(string InstructorId, int? ExamStatus,
             string? CourseName, string? SearchText);
        Task<ExamDetailsResponse?> GetExamByIdAsync(int examId);
        Task<IEnumerable<ExamSummaryResponse>> GetAllByStatus(ExamStatus status);
        Task<IEnumerable<Exam>> GetExamsToSync();
        Task<IEnumerable<AvailableExamResponse>> GetAvailableExams();
    }
}
