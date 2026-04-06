namespace SharpForge.Client.Models;

public class CourseDetail
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Level { get; set; } = "Beginner";
    public string Duration { get; set; } = "";
    public string StudentCount { get; set; } = "0";
    public int ExerciseCount { get; set; }
    public string ProjectDescription { get; set; } = "";
    public List<string> Topics { get; set; } = [];
    public string IconSvg { get; set; } = "";
    public List<string> LearningOutcomes { get; set; } = [];
    public List<string> Prerequisites { get; set; } = [];
    public List<CourseLessonDetail> Lessons { get; set; } = [];

    public CourseLevel CourseLevel => Enum.TryParse<CourseLevel>(Level, ignoreCase: true, out var lvl) ? lvl : CourseLevel.Beginner;

    public int LessonCount => Lessons.Count;
    public string BaseUrl => $"courses/{Slug}";
    public string StartUrl => $"{BaseUrl}/lesson/1";
    public string GetLessonUrl(int lessonNumber) => $"{BaseUrl}/lesson/{lessonNumber}";
}
