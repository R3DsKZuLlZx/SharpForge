namespace SharpForge.Client.Models;

public class LessonFrontmatter
{
    public string Title { get; set; } = "";
    public string CourseSlug { get; set; } = "";
    public int LessonNumber { get; set; }
    public string Duration { get; set; } = "10 min";
    public List<string> Takeaways { get; set; } = [];
    public List<LessonExerciseEntry> Exercises { get; set; } = [];
    public LessonCongratulationsEntry? Congratulations { get; set; }
}
