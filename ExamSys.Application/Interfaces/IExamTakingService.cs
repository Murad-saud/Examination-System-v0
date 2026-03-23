using ExamSys.Application.Common;
using ExamSys.Application.DTOs.ExamTaking;

namespace ExamSys.Application.Interfaces
{
    public interface IExamTakingService
    {
        Task<Result<JoinExamResult>> JoinExamAsync(int examId, string userId);
        //Task<Result<ParticipantExam>> ValidateParticipantExamAsync(int participantExamId
        //    , string userId);
        Task<Result<ExamPageDto>> GetExamPageAsync(int participantExamId, int pageNumber
            , string userId);
        Task<Result> SaveParticipantAnswersAsync(List<SaveAnswersRequest> saveAnswers, string userId
            , int participantExamId);
        Task<Result> SubmitExamAsync(int participantExamId, string? userId);
    }
}
