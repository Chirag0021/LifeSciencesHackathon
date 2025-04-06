namespace LifeSciencesHackathon.Model
{
    public class HazardInfo
    {
        public List<string> GhsHazards { get; set; } = new();
        public string? SignalWord { get; set; }
        public List<string> Synonyms { get; set; } = new();
    }
}
