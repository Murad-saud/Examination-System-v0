namespace ExamSys.Core.ResponseModels.Question;
using ExamSys.Core.Entities;

public class PagedQuestionsResult
{
    public List<Question> Questions { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
