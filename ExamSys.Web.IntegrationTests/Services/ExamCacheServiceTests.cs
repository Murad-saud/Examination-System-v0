public class ExamCacheServiceTests
{
    //[Fact]
    //public async Task Cache_ShouldReturnCachedQuestions_WhenAvailable()
    //{
    //    var cache = new MemoryCache(new MemoryCacheOptions());
    //    var service = new ExamCacheService(cache, NullLogger<ExamCacheService>.Instance);

    //    var cached = new CachedExamDetails
    //    {
    //        AllQuestions = new List<QuestionWithAnswersResponse>
    //        {
    //            new QuestionWithAnswersResponse { QuestionId = 1 }
    //        },
    //        TotalPages = 1,
    //        EndTime = DateTime.Now.AddMinutes(30)
    //    };

    //    await service.SetExamQuestions(1, cached);

    //    var result = await service.GetExamQuestionsAsync(1);

    //    result.Should().NotBeNull();
    //    result.AllQuestions.Should().HaveCount(1);
    //}
}
