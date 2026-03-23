using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Interfaces;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ExamSys.Application.Services
{
    public class ExamCacheService : IExamCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ExamCacheService> _logger;

        public ExamCacheService(IMemoryCache cache, ILogger<ExamCacheService> logger)
        {
            _memoryCache = cache;
            _logger = logger;
        }

        public Task SetExamQuestions(int examId, CachedExamDetails cachedExamDetails)
        {
            try
            {
                var cacheKey = $"exam_{examId}";

                // ✅ FIXED: Use absolute expiration until exam end time
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = cachedExamDetails.EndTime.AddMinutes(2), // Your buffer
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                _memoryCache.Set(cacheKey, cachedExamDetails, cacheOptions);

                var cacheDuration = cachedExamDetails.EndTime - DateTime.Now + TimeSpan.FromMinutes(2);
                _logger.LogInformation("Cached {Count} questions for exam {ExamId} until {EndTime}",
                    cachedExamDetails.AllQuestions.Count, examId, cachedExamDetails.EndTime);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache questions for exam {ExamId}", examId);
                return Task.CompletedTask; // Don't throw - caching failure shouldn't break main flow
            }
        }

        public async Task<CachedExamDetails> GetExamQuestionsAsync(int examId)
        {
            var cacheKey = $"exam_{examId}";
            try
            {
                if (_memoryCache.TryGetValue(cacheKey, out CachedExamDetails cachedData))
                {
                    _logger.LogDebug("Cache hit for exam {ExamId}", examId);
                    return cachedData;
                }

                _logger.LogDebug("Cache miss for exam {ExamId}", examId);
                return null; // ✅ FIXED: Return null instead of empty object
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for exam {ExamId}", examId);
                return null; // Return null to trigger DB fallback
            }
        }

        public async Task<List<QuestionCorrectAnswerResponse>> GetAllCorrectAnswers(int examId)
        {
            var cacheKey = $"exam_{examId}";
            try
            {
                if (_memoryCache.TryGetValue(cacheKey, out CachedExamDetails cachedData))
                {
                    _logger.LogDebug("Cache hit for exam {ExamId}", examId);

                    var correctAnswers = cachedData.AllQuestions
                        .Select(q => new QuestionCorrectAnswerResponse()
                        {
                            QuestionId = q.QuestionId,
                            CorrectAnswerId = q.QuestionId,
                            QuestionWeight = q.QuestionWeight
                        })
                        .ToList();

                    return correctAnswers;
                }

                _logger.LogDebug("Cache miss for exam {ExamId}", examId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for exam {ExamId}", examId);
                return null; // Return null to trigger DB fallback
            }
        }

    }
}
