using UnityEngine;

public class ScannableObject : MonoBehaviour
{
    [Header("In-Game Metadata")]
    public string title;
    [TextArea] public string snippet;
    public string[] tags;
    [TextArea] public string metadataJson; // bv. {"containsPII":true,"source":"USB","filetype":"csv"}
    
    public bool IsMarked { get; private set; }
    
    [Header("Visuals on Mark")]
    [Tooltip("Renderer that holds multiple materials (MeshRenderer/SkinnedMeshRenderer). If empty, will try to auto-find on this object or children.")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("Which material slot to change when marking (0 = first, 1 = second).")]
    [Min(0)] [SerializeField] private int materialIndex = 1;

    [Tooltip("Material to use when the object is marked.")]
    [SerializeField] private Material markedMaterial;

    [Tooltip("Optional material for the unmarked state; if empty, we restore the original material at that index.")]
    [SerializeField] private Material unmarkedMaterial;

    [Tooltip("When ON, edits sharedMaterials (affects prefab/instances). Usually keep this OFF.")]
    [SerializeField] private bool useSharedMaterials = false;
    private Material _originalAtIndex;


    private void Awake()
    {
        if (!targetRenderer)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }
    }
    
    private void CacheOriginalMaterial()
    {
        if (!targetRenderer) return;

        var mats = useSharedMaterials ? targetRenderer.sharedMaterials : targetRenderer.materials;
        if (materialIndex >= 0 && materialIndex < mats.Length)
        {
            _originalAtIndex = mats[materialIndex];
            if (!unmarkedMaterial) unmarkedMaterial = _originalAtIndex; // fallback to original
        }
    }
    
    private void ApplyMarkedVisual(bool marked)
    {
        if (!targetRenderer) return;

        var mats = useSharedMaterials ? targetRenderer.sharedMaterials : targetRenderer.materials;
        if (materialIndex < 0 || materialIndex >= mats.Length) return;

        // Ensure we have original cached (covers cases where Awake order differs)
        if (_originalAtIndex == null) CacheOriginalMaterial();

        var desired = marked
            ? (markedMaterial ? markedMaterial : mats[materialIndex])
            : (unmarkedMaterial ? unmarkedMaterial : _originalAtIndex);

        if (desired != null && mats[materialIndex] != desired)
        {
            mats[materialIndex] = desired;
            if (useSharedMaterials) targetRenderer.sharedMaterials = mats;
            else targetRenderer.materials = mats;
        }
    }

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
        ApplyMarkedVisual(v);
    }
}