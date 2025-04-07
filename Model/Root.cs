using Newtonsoft.Json;

namespace LifeSciencesHackathon.Model
{
    public class Root
    {
        public Record Record { get; set; }
    }

    public class Record
    {
        public string RecordType { get; set; }
        public int RecordNumber { get; set; }
        public string RecordTitle { get; set; }
        public List<Section> Section { get; set; }
        public List<Reference> Reference { get; set; }
    }

    public class Section
    {
        public string TOCHeading { get; set; }
        public string Description { get; set; }
        [JsonProperty("Section")]
        public List<Section> Subsections { get; set; }
        public DisplayControls DisplayControls { get; set; }
        public List<Information> Information { get; set; }
        public string URL { get; set; }
    }

    public class DisplayControls
    {
        public bool MoveToTop { get; set; }
        public CreateTable CreateTable { get; set; }
        public int? ShowAtMost { get; set; }
        public bool? HideThisSection { get; set; }
        public string ListType { get; set; }
    }

    public class CreateTable
    {
        public string FromInformationIn { get; set; }
        public int NumberOfColumns { get; set; }
        public List<string> ColumnContents { get; set; }
        public List<string> ColumnHeadings { get; set; }
        public ColumnsFromNamedLists ColumnsFromNamedLists { get; set; }
    }

    public class ColumnsFromNamedLists
    {
        public List<string> Name { get; set; }
        public bool UseNamesAsColumnHeadings { get; set; }
    }

    public class Information
    {
        public int ReferenceNumber { get; set; }
        public Value Value { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public List<ExtendedReference> ExtendedReference { get; set; }
        public List<string> Reference { get; set; }
        public string URL { get; set; }
    }

    public class Value
    {
        public List<decimal> Number { get; set; }
        public List<bool> Boolean { get; set; }
        public List<StringWithMarkup> StringWithMarkup { get; set; }
        public List<string> DateISO8601 { get; set; }
        public List<string> ExternalDataURL { get; set; }
        public string MimeType { get; set; }
        public string ExternalTableName { get; set; }
        public List<string> Binary { get; set; }
        public string Unit { get; set; }
        public int? ExternalTableNumRows { get; set; }
    }

    public class StringWithMarkup
    {
        public string String { get; set; }
        public List<Markup> Markup { get; set; }
    }

    public class Markup
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string URL { get; set; }
        public string Type { get; set; }
        public string Extra { get; set; }
    }

    public class ExtendedReference
    {
        public string Citation { get; set; }
        public Matched Matched { get; set; }
    }

    public class Matched
    {
        public int PCLID { get; set; }
        public string Citation { get; set; }
        public string DOI { get; set; }
        public int? PMID { get; set; }
        public string PMCID { get; set; }
    }

    public class Reference
    {
        public int ReferenceNumber { get; set; }
        public string SourceName { get; set; }
        public string SourceID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string LicenseURL { get; set; }
        public int ANID { get; set; }
        public string LicenseNote { get; set; }
        public bool? IsToxnet { get; set; }
    }
}
