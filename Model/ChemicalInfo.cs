namespace LifeSciencesHackathon.Model
{
    public class ChemicalInfo
    {
        public int CID { get; set; }
        public string Name { get; set; }
        public string MolecularFormula { get; set; }
        public decimal MolecularWeight { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Toxicity { get; set; } = new();
        public Dictionary<string, string> PhysicalProperties { get; set; } = new();
    }
}
