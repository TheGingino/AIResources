using UnityEngine;

/// <summary>
/// Responsible for taking scan data and pushing it into the DialogueUI.
/// No AI calls, no HTTP, just data -> UI.
/// </summary>
public class ShowData : MonoBehaviour
{
    [Header("References")]
    public DialogueUI ui;

    [Header("Input – Object Flagging")]
    public bool markToggles = true;     // if true: input toggles mark on/off
    public bool showMarkToast = true;   // show hint text when marked/unmarked

    private ScannableObject _current;

    private void Awake()
    {
        if (!ui)
        {
            ui = GetComponentInChildren<DialogueUI>(true);
        }

        if (ui != null)
        {
            ui.OnMarkToggled += HandleMarkToggledFromUI;
            ui.OnClosePressed += HandleClosePressedFromUI;
        }
    }

    private void Update()
    {
        HandleMarkInput();
    }

    /// <summary>
    /// Call this from your scanning logic when you know what to show.
    /// </summary>
    public void ShowScanResult(ScannableObject obj, ScanResult data)
    {
        if (ui == null || data == null)
            return;

        _current = obj;

        // Build display strings
        string labelsText =
            (data.labels != null && data.labels.Length > 0)
                ? string.Join(", ", data.labels)
                : "onbekend";

        string actionsText =
            (data.actions != null && data.actions.Length > 0)
                ? string.Join(" • ", data.actions)
                : "-";

        var color = data.GetStatusColor();
        var title = obj != null && !string.IsNullOrEmpty(obj.title)
            ? obj.title
            : "Onbekend object";

        // Push into UI
        ui.ShowResult(
            title:  title,
            status: (data.status ?? "").ToUpperInvariant(),
            color:  color,
            labels: labelsText,
            actions: actionsText,
            score:  data.score
        );

        // Use description as explanation text
        if (!string.IsNullOrEmpty(data.description))
        {
            ui.ShowSystemHint(data.description);
        }

        // Sync mark state in UI
        if (obj != null)
            ui.ShowMarkState(obj.IsMarked);
    }

    /// <summary>
    /// Hide / reset the UI.
    /// </summary>
    public void Clear()
    {
        _current = null;
        if (ui != null)
            ui.gameObject.SetActive(false);
    }

    // ---------- Mark handling ----------

    // Called when the toggle in DialogueUI is used
    private void HandleMarkToggledFromUI(bool isOn)
    {
        if (_current == null) return;

        _current.SetMarked(isOn);
        ui?.ShowMarkState(_current.IsMarked);

        if (showMarkToast && ui != null)
        {
            ui.ShowSystemHint(
                _current.IsMarked
                    ? $"\"{_current.title}\" gemarkeerd."
                    : $"\"{_current.title}\" ontmarkeerd."
            );
        }
    }

    // Optional close button
    private void HandleClosePressedFromUI()
    {
        Clear();
    }

    // Optional: input-based marking (VR / keyboard)
    private void HandleMarkInput()
    {
        if (_current == null) return;

        // Example: keyboard M to toggle mark
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool newState = markToggles ? !_current.IsMarked : true;
            _current.SetMarked(newState);
            ui?.ShowMarkState(_current.IsMarked);

            if (showMarkToast && ui != null)
            {
                ui.ShowSystemHint(
                    newState
                        ? $"\"{_current.title}\" gemarkeerd. (druk M om te ontmarkeren)"
                        : $"\"{_current.title}\" ontmarkeerd."
                );
            }
        }

        // If you use OVRInput, you can add that here as well.
        // if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)) { ... }
    }
}
