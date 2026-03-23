using ExamSys.Application.Common;
using ExamSys.Core.Enums;

namespace ExamSys.Application.Interfaces
{
    public interface IExamStateManager
    {
        bool CanEdit(ExamStatus status);
        bool CanDelete(ExamStatus status);
        bool CanAddQuestions(ExamStatus status);
        bool CanSubmit(ExamStatus status);
        bool CanSchedule(ExamStatus status);
        bool CanJoin(ExamStatus status);
        bool CanViewResults(ExamStatus status);
        // ... other actions
        Task<Result> SubmitExamAsync(int examId);
        Task<Result> ApproveExamAsync(int examId);
        Task<Result> RejectExamAsync(int examId);
        // ... other state transitions
    }
}
