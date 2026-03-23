using ExamSys.Application.Common;
using ExamSys.Application.DTOs.Answer;
using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.Extensions.Logging;

namespace ExamSys.Application.Services
{
    public class ExamTakingService : IExamTakingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IExamCacheService _examCacheService;
        private readonly IExamStateManager _examStateManager;
        private readonly ILogger<ExamTakingService> _logger;
        private const int _pageSize = 8;

        public ExamTakingService(IUnitOfWork unitOfWork, IExamCacheService examCacheService,
        IExamStateManager examStateManager, ILogger<ExamTakingService> logger)
        {
            _unitOfWork = unitOfWork;
            _examCacheService = examCacheService;
            _examStateManager = examStateManager;
            _logger = logger;
        }

        public async Task<Result<JoinExamResult>> JoinExamAsync(int examId, string userId)
        {
            try
            {
                _logger.LogInformation("User {UserId} attempting to join exam {ExamId}", userId, examId);

                // 1- Validating Exam
                var examDb = await _unitOfWork.Exams.GetExamByIdAsync(examId);
                if (examDb is null)
                {
                    _logger.LogWarning("Exam {ExamId} not found for user {UserId}", examId, userId);
                    return Result<JoinExamResult>.Failure("Exam wasn't found.");
                }

                var examEndTime = examDb.StartDate + TimeSpan.FromMinutes(examDb.Duration);

                // Enhanced time validation
                if (DateTime.Now < examDb.StartDate)
                    return Result<JoinExamResult>.Failure("Exam hasn't started yet");

                if (DateTime.Now > examEndTime)
                    return Result<JoinExamResult>.Failure("Exam has already ended");

                // 2- Validating ParticipantExam
                var participantExamDb = await _unitOfWork.ParticipantExams.GetOrCreateParticipantExamAsync(examId, userId);
                if (participantExamDb.isSubmitted)
                    return Result<JoinExamResult>.Failure("You have already submitted this exam");

                var joinExamData = new JoinExamResult()
                {
                    ExamId = examDb.ExamId, // Fixed: Use Id instead of ExamId
                    ParticipantExamId = participantExamDb.Id,
                    ExamEndTime = examEndTime,
                    TotalPages = (int)Math.Ceiling((double)examDb.QuestionsCount / _pageSize)
                };

                _logger.LogInformation("User {UserId} successfully joined exam {ExamId}", userId, examId);
                return Result<JoinExamResult>.Success(joinExamData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join exam {ExamId} for user {UserId}. Error: {ErrorMessage}",
                    examId, userId, ex.Message);
                return Result<JoinExamResult>.Failure("Failed to join exam due to system error");
            }
        }

        public async Task<Result<ExamPageDto>> GetExamPageAsync(int participantExamId, int pageNumber
            , string userId)
        {
            try
            {
                _logger.LogDebug("Retrieving page {PageNumber} for participant exam {ParticipantExamId}",
                    pageNumber, participantExamId);

                // 1- Validate ParticipantExam ownership
                var validation = await ValidateParticipantExamAsync(participantExamId, userId);
                if (validation.Failed)
                    return Result<ExamPageDto>.Failure(validation.Error);

                var participantExam = validation.Data;
                var examId = participantExam.ExamId;

                // Additional exam time validation
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (examDb == null)
                    return Result<ExamPageDto>.Failure("Exam not found");

                var examEndTime = examDb.StartDate + TimeSpan.FromMinutes(examDb.Duration);
                if (DateTime.Now > examEndTime)
                    return Result<ExamPageDto>.Failure("Exam time has expired");

                // 2- Get questions with fallback - FIXED CACHE LOGIC
                List<QuestionWithAnswersResponse> allExamQuestions;
                int totalPages;
                CachedExamDetails cachedExam = await _examCacheService.GetExamQuestionsAsync(examId);

                if (cachedExam?.AllQuestions?.Any() == true) // ✅ FIXED: Use cache when it has data
                {
                    _logger.LogDebug("Using cached questions for exam {ExamId}", examId);
                    allExamQuestions = cachedExam.AllQuestions;
                    totalPages = cachedExam.TotalPages;
                }
                else
                {
                    _logger.LogDebug("Cache miss - loading questions from DB for exam {ExamId}", examId);
                    allExamQuestions = await _unitOfWork.Questions.GetAllExamQuestionsAsync(examId);

                    if (allExamQuestions == null || !allExamQuestions.Any())
                        return Result<ExamPageDto>.Failure("No questions found for this exam");

                    totalPages = (int)Math.Ceiling((double)allExamQuestions.Count / _pageSize);

                    // Cache for future requests
                    var cachedExamDetails = new CachedExamDetails()
                    {
                        AllQuestions = allExamQuestions,
                        TotalPages = totalPages,
                        EndTime = examEndTime
                    };

                    await _examCacheService.SetExamQuestions(examId, cachedExamDetails);
                    _logger.LogInformation("Cached {Count} questions for exam {ExamId}", allExamQuestions.Count, examId);
                }

                // Validate page number
                if (pageNumber < 1 || pageNumber > totalPages)
                    return Result<ExamPageDto>.Failure("Invalid page number");

                // 3- Paginate questions
                var pageQuestions = allExamQuestions
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize)
                    .Select(q => new SanitizedQuestionDto()
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        QuestionWeight = q.QuestionWeight,
                        Answers = q.Answers.Select(a => new AnswerPageDto()
                        {
                            AnswerId = a.Id,
                            AnswerText = a.Text,
                            QuestionId = a.QuestionId
                        }).ToList()
                    }).ToList();

                // 4- Get saved answers for these questions
                var questionIds = pageQuestions.Select(q => q.QuestionId).ToList();
                var savedAnswersDict = await _unitOfWork.ParticipantAnswers
                    .GetPageSavedAnswersAsync(participantExamId, questionIds);

                // 5- Map to Dto
                var examPageDto = new ExamPageDto()
                {
                    ExamId = examId,
                    ParticipantExamId = participantExamId,
                    PageQuestions = pageQuestions,
                    SavedAnswers = savedAnswersDict ?? new Dictionary<int, int>(), // Ensure not null
                    CurrentPage = pageNumber,
                    TotalPages = totalPages,
                    EndTime = examDb.StartDate + TimeSpan.FromMinutes(examDb.Duration)
                };

                _logger.LogDebug("Successfully retrieved page {PageNumber} for participant exam {ParticipantExamId}",
                    pageNumber, participantExamId);
                return Result<ExamPageDto>.Success(examPageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving page {PageNumber} for participant exam {ParticipantExamId}",
                    pageNumber, participantExamId);
                return Result<ExamPageDto>.Failure("Failed to load exam page");
            }
        }

        public async Task<Result> SaveParticipantAnswersAsync(List<SaveAnswersRequest> saveAnswers, string userId, int participantExamId)
        {
            try
            {
                _logger.LogDebug("Saving {Count} answers for participant exam {ParticipantExamId}",
                    saveAnswers.Count, participantExamId);

                // 1. Validate ownership
                var validation = await ValidateParticipantExamAsync(participantExamId, userId);
                if (validation.Failed)
                    return Result.Failure(validation.Error); // ✅ FIXED: Return Result, not Result<ExamPageDto>

                var participantExam = validation.Data;

                // Validate input
                if (saveAnswers == null || !saveAnswers.Any())
                    return Result.Failure("No answers provided to save");

                // 2. Filter first for reducing DB calls
                var questionIds = saveAnswers.Select(s => s.QuestionId).Distinct().ToList(); // Added Distinct
                var currentPageSavedAnswers = await _unitOfWork.ParticipantAnswers
                    .GetParticipantSavedAnswersAsync(participantExamId, questionIds);

                // Create lookup dictionary for existing answers
                var existingAnswersDict = currentPageSavedAnswers?.ToDictionary(a => a.QuestionId)
                    ?? new Dictionary<int, ParticipantAnswer>();

                var updatedAnswers = new List<ParticipantAnswer>();
                var newSubmittedAnswers = new List<ParticipantAnswer>();

                foreach (var request in saveAnswers)
                {
                    if (existingAnswersDict.TryGetValue(request.QuestionId, out var existingAnswer))
                    {
                        // Check if answer actually changed
                        if (existingAnswer.SelectedAnswerId != request.SelectedAnswerId)
                        {
                            existingAnswer.SelectedAnswerId = request.SelectedAnswerId;
                            updatedAnswers.Add(existingAnswer);
                        }
                        // If no change, do nothing
                    }
                    else
                    {
                        // New answer - create entity
                        var newAnswer = new ParticipantAnswer
                        {
                            ParticipantExamId = participantExamId,
                            QuestionId = request.QuestionId,
                            SelectedAnswerId = request.SelectedAnswerId
                        };
                        newSubmittedAnswers.Add(newAnswer);
                    }
                }

                // Only proceed if there are changes
                if (!updatedAnswers.Any() && !newSubmittedAnswers.Any())
                {
                    _logger.LogDebug("No changes detected for participant exam {ParticipantExamId}", participantExamId);
                    return Result.Success();
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (updatedAnswers.Any())
                    {
                        _unitOfWork.ParticipantAnswers.UpdateRange(updatedAnswers);
                        _logger.LogDebug("Updated {Count} answers", updatedAnswers.Count);
                    }

                    if (newSubmittedAnswers.Any())
                    {
                        await _unitOfWork.ParticipantAnswers.AddRangeAsync(newSubmittedAnswers);
                        _logger.LogDebug("Added {Count} new answers", newSubmittedAnswers.Count);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("Successfully saved {Total} answers for participant exam {ParticipantExamId}",
                        updatedAnswers.Count + newSubmittedAnswers.Count, participantExamId);
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Transaction failed while saving answers for participant exam {ParticipantExamId}",
                        participantExamId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save answers for participant exam {ParticipantExamId}", participantExamId);
                return Result.Failure("Failed to save answers due to system error");
            }
        }



        // Private Helper Methods:
        private async Task<Result<ParticipantExam>> ValidateParticipantExamAsync(int participantExamId
            , string userId)
        {
            var participantExam = await _unitOfWork.ParticipantExams.GetByIdAsync(participantExamId);
            if (participantExam == null)
                return Result<ParticipantExam>.Failure("Exam attempt not found");

            if (participantExam.ParticipantId != userId)
                return Result<ParticipantExam>.Failure("Access denied");

            if (participantExam.isSubmitted)
                return Result<ParticipantExam>.Failure("Exam attempt is no longer active");

            return Result<ParticipantExam>.Success(participantExam);
        }


        public async Task<Result> SubmitExamAsync(int participantExamId, string? userId)
        {
            try
            {
                // 1. Validate ownership
                var validation = await ValidateParticipantExamAsync(participantExamId, userId);
                if (validation.Failed)
                    return Result.Failure(validation.Error);

                // 2. Get Correct Answers from db to calculate result
                var participantExam = validation.Data;
                var examId = participantExam.ExamId;

                var participantAnswers = await _unitOfWork.ParticipantAnswers
                    .AutoCorrectExamAsync(participantExamId);

                // 3. Calculating score
                int score = 0;
                int correctAnswersCount = 0;
                foreach (var answer in participantAnswers)
                {
                    if (answer.IsCorrect)
                    {
                        score += answer.Weight;
                        correctAnswersCount++;
                    }

                }

                // 4. Updating Participant Exam
                participantExam.Score = score;
                participantExam.CorrectAnswersCount = correctAnswersCount;
                participantExam.isSubmitted = true;

                _unitOfWork.ParticipantExams.Update(participantExam);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError("type something later ....."); //#########
                return Result.Failure(ex.Message);
            }
        }
    }
}
