namespace SharpForge.Client.Data;

public class CourseDetail
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Level { get; init; }
    public required string Duration { get; init; }
    public required string StudentCount { get; init; }
    public required int ExerciseCount { get; init; }
    public string ProjectDescription { get; init; } = "";
    public required string IconSvg { get; init; }
    public required List<string> LearningOutcomes { get; init; }
    public required List<string> Prerequisites { get; init; }
    public required List<CourseLessonDetail> Lessons { get; init; }
    public int LessonCount => Lessons.Count;

    public string BaseUrl => $"training/{Slug}";
    public string StartUrl => $"{BaseUrl}/lesson/1";
    public string GetLessonUrl(int lessonNumber) => $"{BaseUrl}/lesson/{lessonNumber}";
}

public record CourseLessonDetail(int Number, string Title, string Description, string Duration);
