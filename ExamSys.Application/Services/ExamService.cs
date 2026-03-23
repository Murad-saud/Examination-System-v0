using ExamSys.Application.Common;
using ExamSys.Application.DTOs.Answer;
using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.ParticipantExam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExamSys.Application.Services
{
    public class ExamService : IExamService
    {
        private readonly ILogger<ExamService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExamStateManager _examStateManager;
        private readonly IExamTakingService _examTakingService;
        private readonly int _pageSize = 10;

        public ExamService(IUnitOfWork unitOfWork, IExamStateManager examStateManager
            , IExamTakingService examTakingService, ILogger<ExamService> logger)
        {
            _unitOfWork = unitOfWork;
            _examStateManager = examStateManager;
            _examTakingService = examTakingService;

            _logger = logger;
        }



        public async Task<int> CreateExamAsync(CreateExamDto createExamDto)
        {
            try
            {
                var newExam = new Exam()
                {
                    Title = createExamDto.ExamTitle,
                    StartDate = createExamDto.StartDate,
                    Duration = createExamDto.ExamDuration,
                    CourseId = createExamDto.CourseId,
                    InstructorId = createExamDto.InstructorId,

                    Status = ExamStatus.Draft
                };

                await _unitOfWork.Exams.AddAsync(newExam);
                await _unitOfWork.SaveChangesAsync();

                return newExam.Id;

                //return Result.Success();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Unexpected error creating exam");
                //return Result.Failure("An unexpected error occurred while creating the exam. Please try again.");
                return 0;
            }
        }

        public async Task<ExamBuilderDto?> GetExamDetailsAsync(int examId)
        {
            try
            {
                var examWithQuestions = await _unitOfWork.Exams.GetExamWithQuestions(examId);

                if (examWithQuestions == null)
                {
                    // Log that the exam wasn't found (optional)
                    // _logger.LogInformation("Exam with ID {ExamId} was not found", examId);
                    return null;
                }

                return new ExamBuilderDto()
                {

                    ExamId = examWithQuestions.ExamId,
                    ExamTitle = examWithQuestions.ExamTitle,
                    StartDate = examWithQuestions.StartDate,
                    ExamStatus = examWithQuestions.Status,
                    QuestionsCount = examWithQuestions.ExamQuestions.Count,
                    CourseName = examWithQuestions.CourseName,
                    ExamQuestions = examWithQuestions.ExamQuestions.Select(q => new QuestionDetailsDto()
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        QuestionWeight = q.Weight,
                        AnswersCount = q.AnswersCount
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                // Actually log the exception
                //_logger.LogError(ex, "Error retrieving exam details for exam ID {ExamId}", examId);

                // Consider throwing a custom exception instead of returning null
                //throw new DataRetrievalException($"Could not retrieve exam details for exam ID {examId}", ex);
                throw new Exception($"Could not retrieve exam details for exam ID {examId}", ex);
            }
        }

        public async Task<ExamDetailsDto?> GetExamByIdAsync(int examId)
        {
            try
            {
                var examResponseDb = await _unitOfWork.Exams.GetExamByIdAsync(examId);
                var examDetailsDto = examResponseDb != null ? new ExamDetailsDto()
                {
                    ExamId = examResponseDb.ExamId,
                    ExamTitle = examResponseDb.ExamTitle,
                    StartDate = examResponseDb.StartDate,
                    Duration = examResponseDb.Duration,
                    InstructorId = examResponseDb.InstructorId,
                    CourseId = examResponseDb.CourseId,
                    CourseName = examResponseDb.CourseName
                } : null;

                return examDetailsDto;
            }
            catch (Exception ex)
            {
                //logger shit -- consider it implemented
                return null;
            }
        }

        public async Task<List<ExamTableItemDto>> GetInstructorExams(string InstructorId, int? ExamStatus,
             string? CourseName, string? SearchText)
        {
            try
            {
                var filteredExamsDb = await _unitOfWork.Exams.GetFilteredExams(InstructorId, ExamStatus, CourseName,
                     SearchText);

                if (filteredExamsDb == null)
                {
                    //_logger.LogWarning("Repository returned null for instructor exams");
                    return new List<ExamTableItemDto>();
                }


                var examsTableDto = filteredExamsDb.Select(e => new ExamTableItemDto()
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    Status = e.Status,
                    CourseName = e.CourseName,
                    StartDate = e.StartDate
                }).ToList();

                return examsTableDto;

            }
            catch (Exception ex)
            {
                // Consider throwing a custom exception instead of returning an empty list
                //logger shit here #JUST IMAGINE that i have implemented logger here
                return new List<ExamTableItemDto>();
            }
        }

        public async Task<Result> UpdateExamAsync(UpdateExamDto updateExamDto)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(updateExamDto.ExamId);
                if (examDb is null)
                {
                    return Result.Failure("Failed to update the exam, Please try again later");
                }

                examDb.Title = updateExamDto.ExamTitle;
                examDb.StartDate = updateExamDto.StartDate;
                examDb.Duration = updateExamDto.Duration;
                examDb.CourseId = updateExamDto.CourseId;

                _unitOfWork.Exams.Update(examDb);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                //logger shit .....
                return Result.Failure("Something Wrong happened, Please try again later");
            }
        }

        public async Task<Result> DeleteExamAsync(int examId)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (examDb is null)
                {
                    return Result.Failure("Exam not found ...");
                }

                _unitOfWork.Exams.Remove(examDb);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Something wrong happened, Please try again later.");
            }
        }

        public async Task<string?> GetExamStatus(int examId)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (examDb is null)
                {
                    return "";
                }

                return examDb.Status.ToString();
            }
            catch (Exception ex)
            {
                // logger shit here.....
                return null;
            }
        }

        public async Task<List<ExamSummaryDto>> GetExamsByState(ExamStatus status)
        {
            try
            {
                var examsDb = await _unitOfWork.Exams.GetAllByStatus(status);

                if (examsDb is null || !examsDb.Any())
                    return new List<ExamSummaryDto>();

                return examsDb.Select(e => new ExamSummaryDto()
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    StartDate = e.StartDate,
                    Duration = e.Duration,
                    QuestionsCount = e.QuestionsCount,
                    CourseId = e.CourseId,
                    CourseName = e.CourseName,
                    InstructorId = e.InstructorId,
                    InstructorName = e.InstructorName
                }).ToList();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving {Status} exams", status);
                throw new ApplicationException("Failed to retrieve exams. Please try again.");
            }
        }

        public async Task<Result> SubmitExamAsync(int examId, string useId)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);

                if (examDb is null)
                    return Result.Failure("Couldn't find exam attatched to this Id.");

                if (examDb.InstructorId != useId)
                    return Result.Failure("Access denied");

                if (!_examStateManager.CanSubmit(examDb.Status))
                    return Result.Failure("Exam can't be submit in this state.");

                examDb.Status = ExamStatus.Pending;

                _unitOfWork.Exams.Update(examDb);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("An expected error occurred while tyring to submit the exam, Please" +
                    "try again later.");
            }
        }

        public async Task<Result> ChangeExamStateAsync(int exmaId, ExamStatus newStatus)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(exmaId);

                if (examDb is null)
                    return Result.Failure("Couldn't find the exam");

                if (_examStateManager.CanSchedule(examDb.Status))
                {
                    examDb.Status = newStatus;

                    _unitOfWork.Exams.Update(examDb);
                    await _unitOfWork.SaveChangesAsync();

                    return Result.Success();
                }
                else
                {
                    return Result.Failure("Exam approvement isn't possible in the current exam state.");
                }
            }
            catch (Exception ex)
            {
                return Result.Failure("An unexpected error occurred, Please try again later.");
            }
        }

        public async Task SyncExamsState()
        {
            try
            {
                var examsToSync = await _unitOfWork.Exams.GetExamsToSync();
                if (examsToSync.Any())
                {
                    foreach (var exam in examsToSync)
                    {
                        if (exam.Status == ExamStatus.Scheduled)
                        {
                            exam.Status = ExamStatus.InProgress;
                        }
                        else if (exam.Status == ExamStatus.InProgress)
                            exam.Status = ExamStatus.Completed;
                    }

                    _unitOfWork.Exams.UpdateRange(examsToSync);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<AvailableExamDto>> GetAvailableExamsAsync()
        {
            try
            {
                var availableExams = await _unitOfWork.Exams.GetAvailableExams();
                if (!availableExams.Any())
                {
                    return new List<AvailableExamDto>();
                }

                return availableExams.Select(e => new AvailableExamDto()
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamStatus = e.ExamStatus,
                    StartDate = e.StartDate,
                    Duration = e.Duration,
                    CourseName = e.CourseName,
                    InstructorName = e.InstructorName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve available exams. Error: {ErrorMessage}", ex.Message);
                return new List<AvailableExamDto>();
            }
        }

        // # Questions Related :
        public async Task<Result> AddQuestionToExam(CreateQuestionDto createQuestionDto)
        {
            try
            {
                // 1. Validation
                var examDb = await _unitOfWork.Exams.GetByIdAsync(createQuestionDto.ExamId);
                if (examDb is null)
                    return Result.Failure("Exam not found");


                if (!_examStateManager.CanAddQuestions(examDb.Status))
                    return Result.Failure("Questions can only be added to exams in Draft or Preparing status");

                if (string.IsNullOrWhiteSpace(createQuestionDto.QuestionText))
                    return Result.Failure("Question text is required");

                if (createQuestionDto.AnswersText?.Count < 2)
                    return Result.Failure("At least 2 answers are required");

                if (createQuestionDto.CorrectAnswerIndex < 1 || createQuestionDto.CorrectAnswerIndex > createQuestionDto.AnswersText.Count)
                    return Result.Failure("Invalid correct answer index");

                // Check for duplicate question text
                var existingQuestion = await _unitOfWork.Questions.GetByExamAndTextAsync(createQuestionDto.ExamId,
                    createQuestionDto.QuestionText);

                if (existingQuestion != null)
                    return Result.Failure("A question with this text already exists in this exam");

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 2. Create Answers list
                    var answersList = createQuestionDto.AnswersText.Select(answerText => new Answer
                    {
                        Text = answerText
                    }).ToList();

                    var correctAnswerIndex = createQuestionDto.CorrectAnswerIndex - 1;
                    var correctAnswer = answersList[correctAnswerIndex];

                    // 3. Create Question
                    var newQuestion = new Question()
                    {
                        ExamId = createQuestionDto.ExamId,
                        Text = createQuestionDto.QuestionText,
                        Weight = createQuestionDto.QuestionWeight,
                        Answers = answersList,
                        CorrectAnswerId = null
                    };

                    await _unitOfWork.Questions.AddAsync(newQuestion);
                    await _unitOfWork.SaveChangesAsync();

                    newQuestion.CorrectAnswerId = correctAnswer.Id;
                    _unitOfWork.Questions.Update(newQuestion);

                    // 4. Update exam status if needed
                    if (examDb.Status == ExamStatus.Draft)
                    {
                        examDb.Status = ExamStatus.Preparing;
                        _unitOfWork.Exams.Update(examDb);
                    }

                    // Single database operation
                    await _unitOfWork.CommitTransactionAsync();

                    return Result.Success();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Result.Failure("An unexpected error occurred while creating the question. Please try again.");
            }
        }

        public async Task<Result> DeleteQuestionFromExam(int questionId)
        {
            try
            {
                // Validation
                var questionDb = await _unitOfWork.Questions.GetByIdAsync(questionId);
                if (questionDb is null)
                    return Result.Failure("Question not found");

                var examId = questionDb.ExamId;
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (examDb is null)
                    return Result.Failure("Question's exam is not found");

                if (!_examStateManager.CanDelete(examDb.Status))
                    return Result.Failure("Cannot delete questions from exam in current status");


                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    _unitOfWork.Questions.Remove(questionDb);

                    var questionsCount = await _unitOfWork.Questions.GetQuestionsCount(examId);
                    if (questionsCount == 1)
                    {
                        examDb.Status = ExamStatus.Draft;
                        _unitOfWork.Exams.Update(examDb);
                    }

                    //await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    return Result.Success();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Result.Failure("An unexpected error occurred while deleting the question. Please try again.");
            }
        }

        public async Task<Result> ValidateExamOwnership(int examId, string userId)
        {
            try
            {
                var examDb = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (examDb is null)
                    return Result.Failure("Exam was not found");

                if (examDb.InstructorId != userId)
                    return Result.Failure("Access Denied.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError("");
                return Result.Failure("An handeled exception occured during processing the operation");
            }
        }

        public async Task<Result<ParticipantExamPageDto>> GetParticipantExamPageAsync(int participantExamId
            , int pageNumber)
        {
            try
            {
                // Get participant exam with participant and exam info in one query
                var participantExam = await _unitOfWork.ParticipantExams
                    .GetByIdWithExamAndParticipantAsync(participantExamId);

                if (participantExam == null)
                    return Result<ParticipantExamPageDto>.Failure("Participant exam not found");

                // Get paginated questions with answers in single query
                var pagedQuestions = await _unitOfWork.Questions
                    .GetExamQuestionsWithAnswersAsync(participantExam.ExamId, pageNumber, _pageSize);

                // Get participant answers for just these questions
                var questionIds = pagedQuestions.Questions.Select(q => q.Id).ToList();
                var savedAnswers = await _unitOfWork.ParticipantAnswers
                    .GetPageSavedAnswersAsync(participantExamId, questionIds);

                var pageDto = new ParticipantExamPageDto
                {
                    ExamId = participantExam.ExamId,
                    ExamTitle = participantExam.Exam.Title,
                    ParticipantExamId = participantExamId,
                    ParticipantName = participantExam.Participant.FullName,
                    PageQuestions = pagedQuestions.Questions.Select(q => new QuestionSummaryDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        QuestionWeight = q.Weight,
                        CorrectAnswerId = (int)q.CorrectAnswerId,
                        Answers = q.Answers.Select(a => new AnswerDto
                        {
                            AnswerId = a.Id,
                            AnswerText = a.Text
                        }).ToList()
                    }).ToList(),
                    SavedAnswers = savedAnswers,
                    CurrentPage = pageNumber,
                    TotalPages = pagedQuestions.TotalPages
                };

                return Result<ParticipantExamPageDto>.Success(pageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participant exam page {ParticipantExamId}", participantExamId);
                return Result<ParticipantExamPageDto>.Failure("Failed to load participant exam page");
            }
        }
        public async Task<Result<ExamResultsDto>> GetAllParticipantExamsResults(int examId)
        {
            try
            {
                // Get exam with questions in single query
                var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
                if (exam == null)
                    return Result<ExamResultsDto>.Failure("Exam not found");

                // Calculate total points from questions
                var totalPoints = await _unitOfWork.Questions.GetExamTotalPointsAsync(examId);

                // Get all participant results
                var allExamResults = await _unitOfWork.ParticipantExams.GetAllExamResults(examId);

                var resultsDto = new ExamResultsDto
                {
                    ExamId = examId,
                    ExamTitle = exam.Title,
                    TotalPoints = totalPoints,
                    Results = allExamResults.Select(er => new ParticipantExamResultDto
                    {
                        ParticipantExamId = er.ParticipantExamId,
                        ParticipantId = er.ParticipantId,
                        ParticipantName = er.ParticipantName,
                        CorrectAnswersCount = er.CorrectAnswersCount,
                        Score = $"{er.Score}/{totalPoints}",
                        FinalScorePercentage = "%" + (totalPoints > 0 ? (er.Score / totalPoints) * 100 : 0).ToString("F2")
                    }).ToList()
                };

                return Result<ExamResultsDto>.Success(resultsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam results for exam {ExamId}", examId);
                return Result<ExamResultsDto>.Failure("Failed to load exam results");
            }
        }


    }

}
