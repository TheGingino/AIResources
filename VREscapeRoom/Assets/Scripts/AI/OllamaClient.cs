using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class OllamaClient
{
    [Serializable] class Req { public string model; public string prompt; public bool stream = false; }
    [Serializable] class Resp { public string response; }

    public static IEnumerator Generate(string endpoint, string model, string prompt, Action<string> done, Action<string> fail = null)
    {
        var json = JsonUtility.ToJson(new Req { model = model, prompt = prompt });
        using var req = new UnityWebRequest(endpoint, "POST");
        var body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) { fail?.Invoke(req.error); yield break; }
        var r = JsonUtility.FromJson<Resp>(req.downloadHandler.text);
        done?.Invoke(r != null ? r.response : "");
    }
}
