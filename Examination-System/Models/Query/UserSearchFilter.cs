using Microsoft.AspNetCore.Mvc;

namespace Examination_System.Models.Query
{
    public class UserSearchFilter
    {
        [FromQuery(Name = "searchBy")]
        public string? SearchBy { get; set; }
        [FromQuery(Name = "role")]
        public string? Role { get; set; }
        [FromQuery(Name = "keywords")]
        public string? Keywords { get; set; }
    }
}
