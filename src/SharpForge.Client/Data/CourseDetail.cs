namespace SharpForge.Client.Data;

/// <summary>
/// All data needed to render a course overview page.
/// </summary>
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

    public string StartUrl => $"training/{Slug}/lesson/1";
    public int LessonCount => Lessons.Count;
}

public record CourseLessonDetail(int Number, string Title, string Description, string Duration);

