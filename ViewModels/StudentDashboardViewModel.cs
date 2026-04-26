namespace AD_COURSEWORK_2.ViewModels;

public class StudentDashboardViewModel
{
    public List<EnrolledCourseRow> EnrolledCourses { get; set; } = new();
    public List<DeadlineRow> UpcomingDeadlines { get; set; } = new();
    public int UnreadMessages { get; set; }

    public class EnrolledCourseRow
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Lecturer { get; set; } = string.Empty;
    }

    public class DeadlineRow
    {
        public string Title { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime DueDateUtc { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
