using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class CoursesAdminViewModel : ViewModelBase
    {
        public List<Course> Courses { get; } = InMemoryStore.Instance.Courses.OrderBy(c => c.CourseCode).ToList();
    }
}
