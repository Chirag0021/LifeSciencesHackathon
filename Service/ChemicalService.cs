using LifeSciencesHackathon.Model;
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
                            // 🔹 Handle Toxicity
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

                                                if (info.TryGetProperty("Value", out var val) &&
                                                    val.TryGetProperty("StringWithMarkup", out var values))
                                                {
                                                    foreach (var item in values.EnumerateArray())
                                                    {
                                                        var valueStr = item.GetProperty("String").GetString();
                                                        if (!string.IsNullOrWhiteSpace(valueStr) &&
                                                            !chemicalInfo.PhysicalProperties.ContainsKey(label))
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
                var doc = JsonDocument.Parse(viewResponse);

                var root = doc.RootElement;

                if (!root.TryGetProperty("Record", out var record) ||
                    !record.TryGetProperty("Section", out var sections))
                    return null;

                var hazardInfo = new HazardInfo();

                foreach (var section in sections.EnumerateArray())
                {
                    if (section.TryGetProperty("TOCHeading", out var heading) &&
                        heading.GetString() == "Safety and Hazards")
                    {
                        if (section.TryGetProperty("Section", out var subSections))
                        {
                            foreach (var sub in subSections.EnumerateArray())
                            {
                                if (sub.TryGetProperty("TOCHeading", out var subHeading) &&
                                    subHeading.GetString() == "GHS Classification")
                                {
                                    if (sub.TryGetProperty("Information", out var infoArray))
                                    {
                                        foreach (var info in infoArray.EnumerateArray())
                                        {
                                            if (info.TryGetProperty("Value", out var value) &&
                                                value.TryGetProperty("StringWithMarkup", out var strings))
                                            {
                                                foreach (var str in strings.EnumerateArray())
                                                {
                                                    if (str.TryGetProperty("String", out var textProp))
                                                    {
                                                        var text = textProp.GetString();
                                                        if (!string.IsNullOrWhiteSpace(text))
                                                        {
                                                            hazardInfo.GhsHazards.Add(text);

                                                            if (text.Contains("Warning") || text.Contains("Danger"))
                                                            {
                                                                hazardInfo.SignalWord = text;
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
                    }

                    if (section.TryGetProperty("TOCHeading", out heading) &&
                        heading.GetString() == "Names and Identifiers")
                    {
                        if (section.TryGetProperty("Section", out var nameSubSections))
                        {
                            foreach (var sub in nameSubSections.EnumerateArray())
                            {
                                if (sub.TryGetProperty("TOCHeading", out var subHeading) &&
                                    subHeading.GetString() == "Synonyms")
                                {
                                    if (sub.TryGetProperty("Information", out var infoArray))
                                    {
                                        foreach (var info in infoArray.EnumerateArray())
                                        {
                                            if (info.TryGetProperty("Value", out var val) &&
                                                val.TryGetProperty("StringWithMarkup", out var synList))
                                            {
                                                foreach (var s in synList.EnumerateArray())
                                                {
                                                    if (s.TryGetProperty("String", out var textProp))
                                                    {
                                                        var text = textProp.GetString();
                                                        if (!string.IsNullOrWhiteSpace(text))
                                                            hazardInfo.Synonyms.Add(text);
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