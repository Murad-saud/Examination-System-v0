using Examination_System.Models.Query;
using ExamSys.Application.DTOs.User;

namespace Examination_System.ViewModels.Admin
{
    public class UserManagementVM
    {
        public UserSearchFilter Filters { get; set; }
        public List<FullUserDto> Users { get; set; }
    }
}
