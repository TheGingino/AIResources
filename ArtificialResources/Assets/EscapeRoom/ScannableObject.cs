using UnityEngine;

public class ScannableObject : MonoBehaviour
{
    [Header("In-Game Metadata")]
    public string title;
    [TextArea] public string snippet;
    public string[] tags;
    [TextArea] public string metadataJson; // bv. {"containsPII":true,"source":"USB","filetype":"csv"}

    // Handige helper:
    public string BuildUserPrompt()
    {
        var tagsCsv = (tags == null || tags.Length == 0) ? "-" : string.Join(", ", tags);
        var meta = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson;

        return
            $@"Analyseer het object hieronder op bias, privacy, security en transparantie. Geef 1 zin uitleg en concrete acties.
Object:
- Titel: {title}
- Tags: {tagsCsv}
- Tekstfragment: {(string.IsNullOrWhiteSpace(snippet) ? "(geen)" : snippet)}
- Bekende metadata: {meta}

Alleen JSON volgens dit schema:
{{
  ""risk"": ""safe|warning|critical"",
  ""labels"": [""bias"",""privacy"",""security"",""transparency""],
  ""explanation"": ""max 1 korte zin"",
  ""actions"": [""sanitize"",""ignore"",""investigate"",""escalate""],
  ""score"": 0
}}";
    }
}