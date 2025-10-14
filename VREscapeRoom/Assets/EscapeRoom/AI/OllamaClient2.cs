using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class OllamaGenerateRequest {
    public string model;
    public string prompt;
    public bool stream = false;
    public string format = "json"; // forceer JSON
    public object options = null;  // bv. new { temperature = 0.2 }
}

public class OllamaClient2 : MonoBehaviour
{
    [Header("Ollama")]
    public string baseUrl = "http://localhost:11434";
    public string model;

    private static readonly HttpClient _http = new HttpClient();

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default, float temperature = 0.2f)
    {
        var req = new OllamaGenerateRequest {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json",
            options = new { temperature = temperature }
        };

        var json = JsonUtility.ToJson(req);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync($"{baseUrl}/api/generate", content, ct);
        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Ollama error {resp.StatusCode}: {body}");

        // Antwoord is JSON (maar Ollama wikkelt het soms in {"response":"...","done":true})
        // We parsen "response" er eerst uit.
        var wrapper = JsonUtility.FromJson<GenerateWrapper>(SanitizeForJson(body));
        return wrapper != null && !string.IsNullOrEmpty(wrapper.response) ? wrapper.response : body;
    }

    [Serializable]
    private class GenerateWrapper { public string response; }

    // Soms zitten er newline/escape issues in de string; simpele sanitisers helpen.
    private string SanitizeForJson(string input)
    {
        // Unity's JsonUtility vereist geldige JSON. Vaak is het al prima.
        // Laat voorlopig ongewijzigd.
        return input;
    }
}
