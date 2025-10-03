using UnityEngine;

public class AIAssistantController : MonoBehaviour
{
    public DialogueUI ui;

    public void OnScanResult(ScannableObject obj, AssistantResult r)
    {
        // Kleur/icoon o.b.v. risk
        var status = r?.risk ?? "warning";
        var color = status switch {
            "safe" => Color.green,
            "warning" => new Color(1f, 0.65f, 0f),
            "critical" => Color.red,
            _ => Color.yellow
        };

        string labelText = r != null && r.labels != null ? string.Join(", ", r.labels) : "onbekend";
        string actions = r != null && r.actions != null ? string.Join(" • ", r.actions) : "investigate";
        string line = r?.explanation ?? "Geen uitleg.";

        ui.ShowResult(
            title: obj.title,
            status: status.ToUpperInvariant(),
            color: color,
            explanation: line,
            labels: labelText,
            actions: actions,
            score: r?.score ?? 0
        );
    }
}