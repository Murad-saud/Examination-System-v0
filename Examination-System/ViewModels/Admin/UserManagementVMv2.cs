using Examination_System.Models.Query;
using ExamSys.Application.DTOs.User;

namespace Examination_System.ViewModels.Admin
{
    public class UserManagementVMv2
    {
        public UserSearchFilter Filters { get; set; }
        public List<FullUserDtoV2> Users { get; set; }
    }
}
