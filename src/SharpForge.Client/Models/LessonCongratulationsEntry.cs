namespace SharpForge.Client.Models;

public class LessonCongratulationsEntry
{
    public string CourseTitle { get; set; } = "";
    public List<string> Topics { get; set; } = [];
    public string ClosingMessage { get; set; } = "<strong>What's next?</strong> Continue your journey with more courses!";
}
