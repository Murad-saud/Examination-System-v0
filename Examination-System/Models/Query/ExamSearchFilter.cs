using Microsoft.AspNetCore.Mvc;

namespace Examination_System.Models.Query
{
    public class ExamSearchFilter
    {
        [FromQuery(Name = "examstatus")]
        public int? ExamStatus { get; set; }
        [FromQuery(Name = "coursename")]
        public string CourseName { get; set; }
        [FromQuery(Name = "keywords")]
        public string Keywords { get; set; }
    }
}
