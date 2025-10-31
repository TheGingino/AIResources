using System;
using UnityEngine;
using UnityEngine.Events;

public class AIAssistantController : MonoBehaviour
{
    public DialogueUI ui;
    
    [Header("Level Timer")]
    public bool enableTimer = true;
    public float totalSeconds = 600f; // 10 minutes as per concept
    
    [Header("Input – Object Flagging")]
    public bool markToggles = true; // if true: key toggles; if false: key always sets marked
    public bool showMarkToast = true; // show a small system hint when flag state changes
    
    private float _remaining;
    private int _lastWholeSecond = -1;
    private bool _timesUpShown;

    private ScannableObject _current;
    
    void Awake()
    {
        if (ui) ui.OnMarkToggled += HandleMarkToggled;
    }
    private void Update()
    {
        HandleMarkInput();
    }

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

        //if (obj != null && r?.suggested_tags != null)
        // {
            //foreach (var t in r.suggested_tags)
            //{
                // You can gate allowed tags on the ScannableObject side if you have a whitelist
            //    obj.ApplyTagSafe(t);
            //}
        //}
        _current = obj;                      // <-- DON’T forget this!
        
        ui.ShowResult(
            title: obj.title,
            status: status.ToUpperInvariant(),
            color: color,
            labels: labelText,
            actions: actions,
            score: r?.score ?? 0
        );


        // Only prepare the explanation; UI button will reveal it on click
        ui.PrepareExplanation(line);
        ui.ShowMarkState(obj && obj.IsMarked);
        if (showMarkToast && obj)
        {
            ui?.ShowSystemHint($"Druk {OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)} om dit object te {(obj.IsMarked ? "ontmarkeren" : "markeren")}.");
        }
    }
    
    private void HandleTimer()
    {
        if (!enableTimer || _timesUpShown) return;
        if (_remaining <= 0f)
        {
            _timesUpShown = true;
            ui?.ShowSystemHint("Tijd is om. Het model wordt nu getraind…");
            return; // Hook scene transition here if desired
        }
        
        _remaining -= Time.deltaTime;

// Tick UI each second
        int sec = Mathf.Max(0, Mathf.CeilToInt(_remaining));
        if (sec != _lastWholeSecond)
        {
            _lastWholeSecond = sec;
            //ui?.ShowTimeRemaining(System.TimeSpan.FromSeconds(sec));
            
            // Milestone nudges
            if (sec == 5 * 60) ui?.ShowSystemHint("Nog 5 minuten. Maak bewuste keuzes—nauwkeurigheid vs. eerlijkheid.");
            if (sec == 60) ui?.ShowSystemHint("Laatste minuut! Finaliseer je dataset of verwijder risicovolle items.");
        }
    }
    
    private void HandleMarkToggled(bool marked)
    {
        if (_current == null) return;
        _current.SetMarked(marked);          // <-- triggers the material swap
        ui.ShowMarkState(_current.IsMarked); // keep toggle label in sync
    }
    
    private void HandleMarkInput()
    {
        if (_current == null) return;
        Debug.Log("No current object to mark.");
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) || Input.GetKeyDown(KeyCode.M))
        {
            bool newState = markToggles ? !_current.IsMarked : true;
            _current.SetMarked(newState);
            if (showMarkToast)
            {
                ui?.ShowSystemHint(newState ? $"\"{_current.title}\" gemarkeerd (druk {OVRInput.Button.SecondaryHandTrigger} om te ontmarkeren)." : $"\"{_current.title}\" ontmarkeerd.");
            }
        }
    }
}