namespace rest_api.Models
{
    public class MissionCompletion
    {
        public string? UniqueName { get; set; }
        public int PlayerId { get; set; }

        public virtual Player Player { get; set; } = null!;

        public virtual Mission Mission { get; set; } = null!;

        public int CompletionCount { get; set; } = 0;

        public bool SPComplete { get; set; } = false;
    }
}