using LifeSciencesHackathon.Model;
using Newtonsoft.Json;
using System.Text.Json;

namespace LifeSciencesHackathon.Service
{
    public class ChemicalService
    {
        private readonly HttpClient _httpClient;

        public ChemicalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ChemicalInfo?> GetChemicalInfoAsync(string name)
        {
            try
            {
                var cidUrl = $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{name}/cids/JSON";
                var cidResponse = await _httpClient.GetStringAsync(cidUrl);
                var cidDoc = JsonDocument.Parse(cidResponse);
                var cid = cidDoc.RootElement
                    .GetProperty("IdentifierList")
                    .GetProperty("CID")[0]
                    .GetInt32();

                var propUrl = $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/cid/{cid}/property/MolecularFormula,MolecularWeight/JSON";
                var propResponse = await _httpClient.GetStringAsync(propUrl);
                var propDoc = JsonDocument.Parse(propResponse);
                var props = propDoc.RootElement
                    .GetProperty("PropertyTable")
                    .GetProperty("Properties")[0];

                var chemicalInfo = new ChemicalInfo
                {
                    CID = cid,
                    Name = name,
                    MolecularFormula = props.GetProperty("MolecularFormula").GetString(),
                    MolecularWeight = Convert.ToDecimal(props.GetProperty("MolecularWeight").ToString()),
                    ImageUrl = $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/cid/{cid}/PNG"
                };
                var detailUrl = $"https://pubchem.ncbi.nlm.nih.gov/rest/pug_view/data/compound/{cid}/JSON";
                var detailResponse = await _httpClient.GetStringAsync(detailUrl);
                var detailDoc = JsonDocument.Parse(detailResponse);

                if (detailDoc.RootElement.TryGetProperty("Record", out var record) &&
                    record.TryGetProperty("Section", out var sections))
                {
                    foreach (var section in sections.EnumerateArray())
                    {
                        if (section.TryGetProperty("TOCHeading", out var heading))
                        {
                            if (heading.GetString() == "Toxicity")
                            {
                                if (section.TryGetProperty("Section", out var toxSections))
                                {
                                    foreach (var tox in toxSections.EnumerateArray())
                                    {
                                        if (tox.TryGetProperty("Information", out var infoArray))
                                        {
                                            foreach (var info in infoArray.EnumerateArray())
                                            {
                                                if (info.TryGetProperty("Value", out var val) &&
                                                    val.TryGetProperty("StringWithMarkup", out var list))
                                                {
                                                    foreach (var item in list.EnumerateArray())
                                                    {
                                                        var txt = item.GetProperty("String").GetString();
                                                        if (!string.IsNullOrWhiteSpace(txt))
                                                            chemicalInfo.Toxicity.Add(txt);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (heading.GetString() == "Chemical and Physical Properties")
                            {
                                if (section.TryGetProperty("Section", out var propSections))
                                {
                                    foreach (var sub in propSections.EnumerateArray())
                                    {
                                        if (sub.TryGetProperty("Information", out var infoArray))
                                        {
                                            foreach (var info in infoArray.EnumerateArray())
                                            {
                                                if (!info.TryGetProperty("Name", out var nameProp)) continue;
                                                var label = nameProp.GetString();

                                                string? valueStr = null;

                                                if (info.TryGetProperty("Value", out var val))
                                                {
                                                    // First try to get from StringWithMarkup
                                                    if (val.TryGetProperty("StringWithMarkup", out var markup))
                                                    {
                                                        foreach (var item in markup.EnumerateArray())
                                                        {
                                                            valueStr = item.GetProperty("String").GetString();
                                                            break;
                                                        }
                                                    }

                                                    // If no StringWithMarkup, try Number + Unit
                                                    if (string.IsNullOrWhiteSpace(valueStr) && val.TryGetProperty("Number", out var number))
                                                    {
                                                        valueStr = number.GetDouble().ToString();

                                                        if (val.TryGetProperty("Unit", out var unit))
                                                        {
                                                            valueStr += " " + unit.GetString();
                                                        }
                                                    }
                                                }

                                                if (!string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(valueStr))
                                                {
                                                    if (!chemicalInfo.PhysicalProperties.ContainsKey(label))
                                                    {
                                                        chemicalInfo.PhysicalProperties.Add(label, valueStr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (chemicalInfo.Toxicity.Any())
                {
                    if (chemicalInfo.Toxicity.Any(t => t.Contains("lethal", StringComparison.OrdinalIgnoreCase) ||
                                                       t.Contains("carcinogenic", StringComparison.OrdinalIgnoreCase)))
                    {
                        chemicalInfo.RiskLevel = "High";
                    }
                    else if (chemicalInfo.Toxicity.Any(t => t.Contains("harmful", StringComparison.OrdinalIgnoreCase)))
                    {
                        chemicalInfo.RiskLevel = "Medium";
                    } 
                    else
                    {
                        chemicalInfo.RiskLevel = "Low";
                    }
                }
                else
                {
                    chemicalInfo.RiskLevel = "Unknown";
                }
                return chemicalInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return null;
            }
        }
        public async Task<HazardInfo?> GetHazardInfoAsync(int cid)
        {
            try
            {
                var viewUrl = $"https://pubchem.ncbi.nlm.nih.gov/rest/pug_view/data/compound/{cid}/JSON";
                var viewResponse = await _httpClient.GetStringAsync(viewUrl);
                var roots = JsonConvert.DeserializeObject<Root>(viewResponse);

                if (roots?.Record?.Section == null)
                    return null;

                var hazardInfo = new HazardInfo();

                foreach (var section in roots.Record.Section)
                {
                    if (section.TOCHeading == "Safety and Hazards")
                    {
                        var ghsSection = section.Subsections?.FirstOrDefault(s => s.TOCHeading == "GHS Classification");
                        if (ghsSection?.Information != null)
                        {
                            foreach (var info in ghsSection.Information)
                            {
                                foreach (var markup in info.Value?.StringWithMarkup ?? new List<StringWithMarkup>())
                                {
                                    var txt = markup.String;
                                    if (!string.IsNullOrWhiteSpace(txt))
                                    {
                                        hazardInfo.GhsHazards.Add(txt);

                                        if (txt.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
                                            txt.Contains("Danger", StringComparison.OrdinalIgnoreCase))
                                        {
                                            hazardInfo.SignalWord = txt;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (section.TOCHeading == "Names and Identifiers")
                    {
                        var synonymSection = section.Subsections?.FirstOrDefault(s => s.TOCHeading == "Synonyms");
                        if (synonymSection?.Information != null)
                        {
                            foreach (var info in synonymSection.Information)
                            {
                                foreach (var markup in info.Value?.StringWithMarkup ?? new List<StringWithMarkup>())
                                {
                                    var txt = markup.String;
                                    if (!string.IsNullOrWhiteSpace(txt))
                                    {
                                        hazardInfo.Synonyms.Add(txt);
                                    }
                                }
                            }
                        }
                    }
                }

                return hazardInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetHazardInfoAsync failed: {ex.Message}");
                return null;
            }
        }

    }
}