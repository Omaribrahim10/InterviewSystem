using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class DbInitializer
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "SuperAdmin", "Admin", "Agent" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var superAdminEmail = "superadmin@admin.com";
        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdmin == null)
        {
            var user = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                FullName = "System SuperAdmin",
                Role = RoleEnum.SuperAdmin,
                DepartmentID = 1
            };

            var result = await userManager.CreateAsync(user, "SuperAdmin@1234");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "SuperAdmin");
                await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "SuperAdmin"));
            }
        }

        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Default Admin",
                Role = RoleEnum.Admin,
                DepartmentID = 1
            };

            var result = await userManager.CreateAsync(user, "Admin@1234");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Admin"));
            }
        }
    }
}
