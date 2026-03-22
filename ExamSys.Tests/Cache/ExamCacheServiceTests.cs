using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Services;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExamSys.Tests.Caching
{
    public class ExamCacheServiceTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ExamCacheService _cacheService;

        public ExamCacheServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cacheService = new ExamCacheService(
                _memoryCache,
                NullLogger<ExamCacheService>.Instance
            );
        }

        [Fact] // Test 1
        public async Task GetExamQuestionsAsync_WhenCacheEmpty_ReturnsNull()
        {
            // Act
            var result = await _cacheService.GetExamQuestionsAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact] // Test 2
        public async Task SetExamQuestions_ThenGetExamQuestions_ReturnsCachedData()
        {
            // Arrange
            var examId = 10;
            var cachedDetails = new CachedExamDetails
            {
                EndTime = DateTime.Now.AddMinutes(30),
                TotalPages = 2,
                AllQuestions = new List<QuestionWithAnswersResponse>
        {
            new QuestionWithAnswersResponse
            {
                QuestionId = 1,
                QuestionText = "Q1",
                QuestionWeight = 5,
                Answers = new List<ExamSys.Core.Entities.Answer>()
            }
        }
            };

            // Act
            await _cacheService.SetExamQuestions(examId, cachedDetails);
            var result = await _cacheService.GetExamQuestionsAsync(examId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.AllQuestions);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact] // Test 3
        public async Task GetAllCorrectAnswers_WhenCacheHit_ReturnsCorrectAnswers()
        {
            // Arrange
            var examId = 20;
            var cachedDetails = new CachedExamDetails
            {
                EndTime = DateTime.Now.AddMinutes(20),
                TotalPages = 1,
                AllQuestions = new List<QuestionWithAnswersResponse>
        {
            new QuestionWithAnswersResponse
            {
                QuestionId = 1,
                CorrectAnswerId = 3,
                QuestionWeight = 10
            },
            new QuestionWithAnswersResponse
            {
                QuestionId = 2,
                CorrectAnswerId = 5,
                QuestionWeight = 5
            }
        }
            };

            await _cacheService.SetExamQuestions(examId, cachedDetails);

            // Act
            var result = await _cacheService.GetAllCorrectAnswers(examId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.QuestionId == 1);
            Assert.Contains(result, r => r.QuestionId == 2);
        }


        // =========== EDGE CASES ===========

        [Fact]
        public async Task GetAllCorrectAnswers_WhenCacheMiss_ReturnsNull()
        {
            // Act
            var result = await _cacheService.GetAllCorrectAnswers(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetExamQuestions_WhenCacheFails_DoesNotThrow()
        {
            // Arrange
            var brokenCache = new MemoryCache(new MemoryCacheOptions());
            brokenCache.Dispose();

            var service = new ExamCacheService(
                brokenCache,
                NullLogger<ExamCacheService>.Instance
            );

            var cachedDetails = new CachedExamDetails
            {
                EndTime = DateTime.Now.AddMinutes(10),
                TotalPages = 1,
                AllQuestions = new List<QuestionWithAnswersResponse>()
            };

            // Act & Assert
            await service.SetExamQuestions(1, cachedDetails);
        }










    }
}
