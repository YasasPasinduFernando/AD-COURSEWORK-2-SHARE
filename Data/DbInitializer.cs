using AD_COURSEWORK_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AD_COURSEWORK_2.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { AppRoles.Student, AppRoles.Lecturer, AppRoles.Administrator })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var lecturer = await EnsureUserAsync(userManager, "lecturer@unimanage.local", "Dr. Jane Lecturer", "Lecturer123!", AppRoles.Lecturer);
        var student = await EnsureUserAsync(userManager, "student@unimanage.local", "Alex Student", "Student123!", AppRoles.Student);
        await EnsureUserAsync(userManager, "admin@unimanage.local", "System Admin", "Admin123!", AppRoles.Administrator);

        if (await context.Courses.AnyAsync())
            return;

        var course = new Course
        {
            Code = "CS101",
            Name = "Introduction to Computer Science",
            Description = "Fundamentals of programming, problem solving, and course activities.",
            Credits = 4,
            EnrollmentLimit = 50,
            LecturerId = lecturer.Id
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        context.Enrollments.Add(new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.CourseId,
            EnrolledAtUtc = DateTime.UtcNow
        });

        context.Assignments.Add(new Assignment
        {
            CourseId = course.CourseId,
            Title = "Hello World Lab",
            Description = "Submit a short program that prints your name.",
            DueDateUtc = DateTime.UtcNow.AddDays(14),
            MaxPoints = 100,
            CreatedAtUtc = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
            return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
