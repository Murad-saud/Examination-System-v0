using ExamSys.Application.Common;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Enums;

namespace ExamSys.Application.Services
{
    public class ExamStateManager : IExamStateManager
    {
        public bool CanEdit(ExamStatus status)
        {
            return status == ExamStatus.Draft || status == ExamStatus.Preparing || status == ExamStatus.Cancelled;
        }

        public bool CanDelete(ExamStatus status)
        {
            return status == ExamStatus.Draft ||
                   status == ExamStatus.Preparing ||
                   status == ExamStatus.Pending ||
                   status == ExamStatus.Rejected ||
                   status == ExamStatus.Cancelled;
        }


        public bool CanCancel(ExamStatus status)
        {
            return status == ExamStatus.Pending ||
                   status == ExamStatus.Scheduled;
        }

        public bool CanAddQuestions(ExamStatus status)
        {
            return status == ExamStatus.Draft || status == ExamStatus.Preparing;
        }

        public bool CanSubmit(ExamStatus status)
        {
            return status == ExamStatus.Preparing || status == ExamStatus.Cancelled;
        }

        public bool CanSchedule(ExamStatus status)
        {
            return status == ExamStatus.Pending;
        }

        public bool CanViewResults(ExamStatus status)
        {
            return status == ExamStatus.Completed || status == ExamStatus.InProgress;
        }



        public bool CanJoin(ExamStatus status)
        {
            return status == ExamStatus.InProgress;
        }

        public Task<Result> SubmitExamAsync(int examId)
        {
            throw new NotImplementedException();
        }

        public Task<Result> ApproveExamAsync(int examId)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RejectExamAsync(int examId)
        {
            throw new NotImplementedException();
        }
    }
}
