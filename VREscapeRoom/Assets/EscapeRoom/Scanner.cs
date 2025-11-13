using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public OllamaClient2 ollama;
    public AIAssistantController assistant;
    public ShowData showData; 

    private CancellationTokenSource _cts;

    public async Task ScanAsync(ScannableObject target)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        var prompt = target.BuildUserPrompt();
        var json = await ollama.GenerateAsync(prompt, _cts.Token, temperature: 0.1f);
        var result = AssistantResult.FromJson(json);
        if (result == null)
        {
            // Fallback bij JSON-mislukking
            result = new AssistantResult {
                risk = "warning",
                labels = new[] {"transparency"},
                explanation = "Resultaat onduidelijk geformatteerd; ga voorzichtig verder.",
                actions = new[] {"investigate"},
                score = 50
            };
        }
        assistant.OnScanResult(target, result);
        
    }

    public void OnObjectScanned(ScannableObject scanned)
    {
        if (scanned == null)
        {
            Debug.LogWarning("Scanner.OnObjectScanned called with null scanned object.");
            return;
        }

        // Ensure showData is assigned, try to find one in scene as a fallback
        if (showData == null)
        {
            showData = FindObjectOfType<ShowData>();
            if (showData == null)
            {
                Debug.LogError("Scanner.OnObjectScanned: showData is not assigned and no ShowData found in scene. Assign ShowData in the inspector.");
                return;
            }
            else
            {
                Debug.Log("Scanner.OnObjectScanned: showData auto-assigned via FindObjectOfType.");
            }
        }

        var data = new ScanResult
        {
            status = "warning",
            labels = new[] { "Phishing", "Social engineering" },
            description = "Let op: dit bericht vraagt om inloggegevens.",
            actions = new[] { "Negeer", "Meld bij security" },
            score = 72
        };

        showData.ShowScanResult(scanned, data);
    }

    public void CancelScan() => _cts?.Cancel();
}