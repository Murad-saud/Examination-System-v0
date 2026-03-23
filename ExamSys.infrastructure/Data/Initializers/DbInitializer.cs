using Microsoft.AspNetCore.Identity;

namespace ExamSys.Infrastructure.Data;

public static class DbInitializer
{
    public static readonly IReadOnlyList<string> AllRoles = new[] { "Admin", "Instructor", "Participant" };
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.Roles.Any())
        {
            foreach (var roleName in AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
