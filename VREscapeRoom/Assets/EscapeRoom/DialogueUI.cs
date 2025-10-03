using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public Image statusPill;
    public TMP_Text titleText;
    public TMP_Text statusText;
    public TMP_Text explanationText;
    public TMP_Text labelsText;
    public TMP_Text actionsText;
    public TMP_Text scoreText;

    public void ShowResult(string title, string status, Color color,
        string explanation, string labels, string actions, int score)
    {
        if (statusPill) statusPill.color = color;
        if (titleText) titleText.text = title;
        if (statusText) statusText.text = status;
        if (explanationText) explanationText.text = explanation;
        if (labelsText) labelsText.text = $"Labels: {labels}";
        if (actionsText) actionsText.text = $"Acties: {actions}";
        if (scoreText) scoreText.text = $"Score: {score}";
        gameObject.SetActive(true);
    }
}