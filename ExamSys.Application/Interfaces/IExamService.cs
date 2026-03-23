using ExamSys.Application.Common;
using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.ParticipantExam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Core.Enums;

namespace ExamSys.Application.Interfaces
{
    public interface IExamService
    {
        Task<int> CreateExamAsync(CreateExamDto createExamDto);
        Task<ExamBuilderDto?> GetExamDetailsAsync(int examId);
        Task<List<ExamTableItemDto>> GetInstructorExams(string InstructorId, int? ExamStatus, string? CourseName,
             string? SearchText);
        Task<ExamDetailsDto?> GetExamByIdAsync(int examId);
        Task<Result> UpdateExamAsync(UpdateExamDto updateExamDto);
        Task<Result> DeleteExamAsync(int examId);
        Task<string?> GetExamStatus(int examId);
        Task<Result> AddQuestionToExam(CreateQuestionDto createQuestionDto);
        Task<Result> DeleteQuestionFromExam(int questionId);
        Task<List<ExamSummaryDto>> GetExamsByState(ExamStatus status);
        Task<Result> SubmitExamAsync(int examId, string userId);
        Task<Result> ChangeExamStateAsync(int exmaId, ExamStatus newStatus);
        Task SyncExamsState();
        Task<List<AvailableExamDto>> GetAvailableExamsAsync();
        Task<Result> ValidateExamOwnership(int examId, string userId);
        Task<Result<ExamResultsDto>> GetAllParticipantExamsResults(int examId);
        Task<Result<ParticipantExamPageDto>> GetParticipantExamPageAsync(int participantExamId, int pageNumber);
    }
}
