using System;
using UnityEngine;

/// <summary>
/// Plain data used to fill the UI. No AI required.
/// You can fill this from scriptable objects, lookup tables, hardcoded values, etc.
/// </summary>
[Serializable]
public class ScanResult
{
    public string status;       // e.g. "safe", "warning", "critical"
    public string[] labels;     // e.g. ["privacy", "phishing"]
    public string description;  // short explanation text
    public string[] actions;    // e.g. ["report", "ignore", "investigate"]
    public int score;           // 0–100 (optional)

    public Color GetStatusColor()
    {
        switch (status)
        {
            case "safe":      return Color.green;
            case "warning":   return new Color(1f, 0.65f, 0f);
            case "critical":  return Color.red;
            default:          return Color.yellow;
        }
    }
}