using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;
using SharpForge.Client.Services;

namespace SharpForge.Client.Pages;

public partial class Courses
{
    [Inject]
    public required MarkdownService MarkdownService { get; set; }
    
    private string _selectedFilter = "All";
    private IReadOnlyList<CourseDetail> _courses = [];

    protected override void OnInitialized()
    {
        _courses = MarkdownService.GetAllCourses();
    }

    private IEnumerable<CourseDetail> FilteredCourses => _selectedFilter == "All"
        ? _courses
        : _courses.Where(c => c.Level == _selectedFilter);

    private void SetFilter(string filter)
    {
        _selectedFilter = filter;
    }
}
