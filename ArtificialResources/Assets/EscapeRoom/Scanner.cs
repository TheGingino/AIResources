using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public OllamaClient2 ollama;
    public AIAssistantController assistant;

    private CancellationTokenSource _cts;

    public async Task<AssistantResult> ScanAsync(ScannableObject target)
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
        return result;
    }

    public void CancelScan() => _cts?.Cancel();
}