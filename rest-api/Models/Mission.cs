namespace rest_api.Models
{
    public class Mission
    {
        public string? Name { get; set; }
        public string? UniqueName { get; set; }
        public string? Planet { get; set; }
        public int MasteryXp { get; set; }
        public string? Type { get; set; }

        public virtual ICollection<MissionCompletion> MissionCompletions { get; set; } = new List<MissionCompletion>();
    }
}