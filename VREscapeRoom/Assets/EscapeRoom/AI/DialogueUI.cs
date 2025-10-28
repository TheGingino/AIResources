using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public System.Action<bool> OnMarkToggled;
    public System.Action OnClosePressed; // optional

    [Header("Mark Control")]
    public Toggle markToggle;
    public TMP_Text markLabel; // optional label next to the toggle
    //public Button closeButton; // optional close button
    
    public Image statusPill;
    public TMP_Text titleText;
    public TMP_Text statusText;
    public TMP_Text explanationText;
    public TMP_Text labelsText;
    public TMP_Text actionsText;
    public TMP_Text scoreText;

    [Header("UI")]
    public Button explanationButton; // assign in Inspector

    private string pendingExplanation;
    
    private bool _suppressToggleEvent;


    void Awake()
    {
        // Ensure explanation is hidden at start
        if (explanationText) explanationText.gameObject.SetActive(false);

        if (explanationButton)
            explanationButton.onClick.AddListener(OnExplanationClicked);
    }
    
    void Start()
    {
        if (markToggle)
            markToggle.onValueChanged.AddListener(v => { if (!_suppressToggleEvent) OnMarkToggled?.Invoke(v); });
        //if (closeButton) closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
    }
    public void ShowMarkState(bool isMarked)
    {
        if (markToggle)
        {
            _suppressToggleEvent = true;
            markToggle.isOn = isMarked;
            _suppressToggleEvent = false;
        }
        if (markLabel) markLabel.text = isMarked ? "Gemarkeerd" : "Markeer";
    }

    
    public void ShowResult(string title, string status, Color color,
         string labels, string actions, int score)
    {
        if (statusPill) statusPill.color = color;
        if (titleText) titleText.text = title;
        if (statusText) statusText.text = status;
        if (labelsText) labelsText.text = $"Labels: {labels}";
        if (actionsText) actionsText.text = $"Acties: {actions}";
        if (scoreText) scoreText.text = $"Score: {score}";

        // Hide explanation each new result; (re-)enable the button
        if (explanationText) explanationText.gameObject.SetActive(false);
        if (explanationButton) explanationButton.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }

    // Called by controller to set (but not show) the explanation
    public void PrepareExplanation(string explanation)
    {
        pendingExplanation = explanation;
        // Make sure the button is visible/enabled when there is something to show
        if (explanationButton) explanationButton.gameObject.SetActive(true);
        if (explanationText) explanationText.gameObject.SetActive(false);
    }

    private void OnExplanationClicked()
    {
        if (explanationText)
        {
            explanationText.text = string.IsNullOrEmpty(pendingExplanation) ? "Geen uitleg." : pendingExplanation;
            explanationText.gameObject.SetActive(true);
        }
        if (explanationButton) explanationButton.gameObject.SetActive(false);
    }


    public void ShowSystemHint(string text)
    {
        ShowResult(
            title: "Systeem",
            status: "INFO",
            color: new Color(0.2f, 0.6f, 1f),
            labels: "",
            actions: "",
            score: 0
        );
    }
}