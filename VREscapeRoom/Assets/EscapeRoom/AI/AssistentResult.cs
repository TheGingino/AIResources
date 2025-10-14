using System;
using UnityEngine;

[Serializable]
public class AssistantResult
{
    public string risk;            // "safe","warning","critical"
    public string[] labels;        // bijv. ["privacy","security"]
    public string explanation;     // max 1 zin
    public string[] actions;       // bijv. ["sanitize","investigate"]
    public int score;              // 0-100

    public static AssistantResult FromJson(string json)
    {
        try { return JsonUtility.FromJson<AssistantResult>(json); }
        catch { return null; }
    }
}