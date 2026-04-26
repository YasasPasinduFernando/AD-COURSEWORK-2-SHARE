using AD_COURSEWORK_2.Data;
using AD_COURSEWORK_2.Models;
using AD_COURSEWORK_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AD_COURSEWORK_2.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var studentId = _userManager.GetUserId(User);
        if (studentId == null)
            return Challenge();

        var courses = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lecturer)
            .OrderBy(e => e.Course.Code)
            .Select(e => new StudentDashboardViewModel.EnrolledCourseRow
            {
                Code = e.Course.Code,
                Name = e.Course.Name,
                Lecturer = e.Course.Lecturer.FullName
            })
            .ToListAsync();

        var deadlines = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.Course.Enrollments.Any(e => e.StudentId == studentId))
            .OrderBy(a => a.DueDateUtc)
            .Take(5)
            .Select(a => new StudentDashboardViewModel.DeadlineRow
            {
                Title = a.Title,
                CourseCode = a.Course.Code,
                DueDateUtc = a.DueDateUtc,
                Status = a.Submissions
                    .Where(s => s.StudentId == studentId)
                    .Select(s => s.Status.ToString())
                    .FirstOrDefault() ?? "Not submitted"
            })
            .ToListAsync();

        var unreadMessages = await _context.Messages
            .AsNoTracking()
            .CountAsync(m => m.ReceiverId == studentId && !m.IsRead);

        var model = new StudentDashboardViewModel
        {
            EnrolledCourses = courses,
            UpcomingDeadlines = deadlines,
            UnreadMessages = unreadMessages
        };

        return View(model);
    }
}
