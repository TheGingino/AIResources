using UnityEngine;

public class ScannableObject : MonoBehaviour
{
    [Header("In-Game Metadata")]
    public string title;
    [TextArea] public string snippet;
    public string[] tags;
    [TextArea] public string metadataJson; // bv. {"containsPII":true,"source":"USB","filetype":"csv"}
    
    public bool IsMarked { get; private set; }

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
              ""labels"": [""bias"",""privacy"",""people"",""transparency""],
              ""explanation"": ""max 1 korte zin"",
              ""actions"": [""sanitize"",""ignore"",""investigate"",""escalate""],
              ""score"": 0
            }}";
    }

    public void ApplyTagSafe(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return;
        s = s.Trim().ToLowerInvariant();
        if (tags == null) tags = new string[] { s };
        else
        {
            foreach (var t in tags) if (t.ToLowerInvariant() == s) return; // already present
            var list = new System.Collections.Generic.List<string>(tags) { s };
            tags = list.ToArray();
        }
    }
    
    public void SetMarked(bool v)
    {
        IsMarked = v;
        // optional: play a sound, flash outline, add a worldspace icon, etc.
    }
    
}