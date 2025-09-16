using System;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MailflowManager : MonoBehaviour
{
    [System.Serializable]
    public class Mail
    {
        public string id;
        public string subject;
        [TextArea(2, 5)] public string body;
        [TextArea(2, 5)] public string expected;
        public string parentName;       // new: ensure visible parent name
        public bool answered;
        public string responder;
        public string response;
        public bool correct;
        [TextArea(2, 5)] public string reason;     // new: grading reason
    }

    [Header("Ollama")]
    public string endpoint = "http://localhost:11434/api/generate";
    public string model = "llama3";

    [Header("Email Generator")]
    [TextArea(3, 8)]
    public string[] emailSeedPrompts = {
    "Write a realistic incoming email to school staff about a student absence yesterday...",
    "Write a realistic incoming email from a parent requesting a schedule change...",
    "Write a realistic incoming HR complaint email from a staff member..."
    };
public int preGenerate = 0;

    [Header("Inbox (auto-filled by AI)")]
    public Mail[] inbox;

    int i = -1;
    bool busy = false;

    [Header("UI")]
    public TMP_Text subjectText;
    public TMP_Text bodyText;
    public TMP_Text resultText;
    public TMP_InputField humanInput;
    public Button aiReplyButton;
    public Button humanReplyButton;
    public Button nextButton;

    [Header("SMTP (optional)")]
    public bool sendViaSmtp = false;
    public string smtpHost = "smtp.example.com";
    public int smtpPort = 587;
    public bool smtpUseSsl = true;
    public string smtpUser = "user@example.com";
    public string smtpPass = "password";
    public string from = "user@example.com";
    public string to = "recipient@example.com";

    // ---------- API payloads ----------
    [Serializable]
    class GenReq
    {
        public string model;
        public string prompt;
        public bool stream = false;
        public Options options = new Options();
        public string[] stop = new[] { "```", "\n\n\n" };
    }
    [Serializable] class GenResp { public string response; }
    [Serializable] class Options { public float temperature = 0.1f; }

    [Serializable] class EmailJson { public string subject; public string body; public string expected; public string parentName; }
    [Serializable] class GradeJson { public string decision; public string reason; }

    void Start()
    {
        if (preGenerate > 0)
            StartCoroutine(PreGenerateEmails(preGenerate));
        else
        {
            inbox = new Mail[0];
            i = -1;
            UpdateUI();
        }
    }

    IEnumerator PreGenerateEmails(int count)
    {
        busy = true; LockUI();
        inbox = new Mail[count];
        for (int k = 0; k < count; k++)
        {
            var m = new Mail { id = Guid.NewGuid().ToString("N") };
            yield return GenerateIncomingFromPrompt(m, k + 1);
            inbox[k] = m;
        }
        i = 0;
        busy = false; UnlockUI();
        UpdateUI();
    }

    public void NextMail()
    {
        if (busy) return;
        int next = i + 1;
        if (inbox != null && next < inbox.Length)
        {
            i = next;
            UpdateUI();
            return;
        }
        StartCoroutine(GenerateNextOnDemand());
    }

    IEnumerator GenerateNextOnDemand()
    {
        busy = true; LockUI();

        var newMail = new Mail { id = Guid.NewGuid().ToString("N") };
        yield return GenerateIncomingFromPrompt(newMail, (inbox?.Length ?? 0) + 1);

        int oldLen = inbox?.Length ?? 0;
        var arr = new Mail[oldLen + 1];
        if (oldLen > 0) Array.Copy(inbox, arr, oldLen);
        arr[oldLen] = newMail;
        inbox = arr;

        i = oldLen;
        busy = false; UnlockUI();
        UpdateUI();
    }

    IEnumerator GenerateIncomingFromPrompt(Mail m, int indexNumber)
    {
        // Force strict JSON + explicit example + parent name
        var instruction =
            "You generate ONE realistic INCOMING email for a school/HR staff member to answer. " +
            "Output MUST be EXACTLY one line of MINIFIED JSON with keys: subject, body, expected, parentName. " +
            "No markdown, no comments, no code fences, no surrounding text. " +
            "Use normal double quotes. No trailing commas. " +
            "The email body must include a visible sign-off with the same parentName. " +
            "Example of the ONLY valid output:\n" +
            "{\"subject\":\"<short subject>\",\"body\":\"<email body ending with a sign-off incl. parentName>\",\"expected\":\"<short checklist>\",\"parentName\":\"<Parent Full Name>\"}";

        // Pick one prompt (random or by index)
        string chosenPrompt = emailSeedPrompts.Length > 0
            ? emailSeedPrompts[UnityEngine.Random.Range(0, emailSeedPrompts.Length)]
            : "Write a realistic incoming email to school staff.";

        var fullPrompt = $"{instruction}\n\nSEED PROMPT:\n{chosenPrompt}\n\nReturn JSON only.";



        string raw = null;
        yield return PostToOllama(fullPrompt, s => raw = s, e => raw = null);

        if (string.IsNullOrEmpty(raw))
        {
            m.subject = $"Generated Mail #{indexNumber}";
            m.body = "Generation failed; this is a fallback message.";
            m.expected = "Acknowledge and ask for missing info.";
            m.parentName = "Jordan Rivera";
            yield break;
        }

        string json = SanitizeToJson(raw);
        EmailJson ej = TryParseEmail(json);

        if (ej == null || string.IsNullOrWhiteSpace(ej.subject) || string.IsNullOrWhiteSpace(ej.body))
        {
            // fallback: show raw but still ensure a parent name and sign-off
            m.subject = $"Generated Mail #{indexNumber}";
            m.body = EnsureParentNameInBody(raw.Trim(), out var pName);
            m.expected = "Be clear, helpful, and answer the ask.";
            m.parentName = string.IsNullOrWhiteSpace(pName) ? "Jordan Rivera" : pName;
        }
        else
        {
            m.subject = ej.subject.Trim();
            m.parentName = string.IsNullOrWhiteSpace(ej.parentName) ? GuessParentFromBody(ej.body) ?? "Jordan Rivera" : ej.parentName.Trim();
            var cleanBody = Unescape(ej.body).Trim();
            m.body = EnsureParentNameInBody(cleanBody, out var bodyName);
            if (!string.IsNullOrWhiteSpace(bodyName)) m.parentName = bodyName;
            m.expected = Unescape(ej.expected ?? "").Trim();
        }
    }

    public void AiReply()
    {
        if (busy || !HasMail() || inbox[i].answered) return;
        busy = true; LockUI();
        var m = inbox[i];
        var prompt =
            $"Draft a concise, professional email reply.\nSubject: {m.subject}\nIncoming:\n{m.body}\n" +
            $"Constraints: accurate, clear, actionable. Reply only with the email body.";

        StartCoroutine(PostToOllama(prompt, onOk =>
        {
            m.response = (onOk ?? "").Trim();
            m.answered = true;
            m.responder = "ai";
            if (humanInput) humanInput.text = m.response;
            StartCoroutine(Grade(m));
        },
        onFail =>
        {
            if (resultText) resultText.text = $"Error: {onFail}";
            busy = false; UnlockUI();
        }));
    }

    public void HumanReply()
    {
        if (busy || !HasMail() || inbox[i].answered) return;
        var txt = humanInput != null ? humanInput.text.Trim() : "";
        if (string.IsNullOrEmpty(txt)) { if (resultText) resultText.text = "Empty response."; return; }

        var m = inbox[i];
        m.response = txt;
        m.answered = true;
        m.responder = "human";
        busy = true; LockUI();
        StartCoroutine(Grade(m));
    }

    IEnumerator Grade(Mail m)
    {
        // Ask for JSON: decision + reason
        var rubric =
    "You are a fair grader. Given the incoming email and a proposed reply, decide if the reply is generally appropriate, polite, and covers most of the important points. " +
    "It does not need to be perfect, but it should be reasonably helpful to the sender. " +
    "If it is mostly good, mark PASS. Only mark FAIL if the reply is clearly missing key context, incorrect, or unhelpful. " +
    "Return STRICT JSON on one line: {\"decision\":\"PASS|FAIL\",\"reason\":\"<brief why>\"}. No extra text.\n" +
    $"Incoming:\n{m.body}\nExpected key points:\n{m.expected}\nReply:\n{m.response}\nJSON:";

        string raw = null;
        yield return PostToOllama(rubric, s => raw = s, e => raw = null);

        var gj = ParseGradeJson(SanitizeToJson(raw ?? ""));
        var decisionUpper = (gj?.decision ?? "").ToUpperInvariant();
        m.correct = decisionUpper.Contains("PASS");
        m.reason = gj?.reason ?? (string.IsNullOrWhiteSpace(raw) ? "No response from grader." : raw.Trim());

        if (resultText)
        {
            // Always show decision; show reason on FAIL (or set to always if you want)
            if (m.correct)
                resultText.text = $"{m.responder.ToUpper()} • PASS";
            else
                resultText.text = $"{m.responder.ToUpper()} • FAIL\nWhy: {m.reason}";
        }

        if (sendViaSmtp && m.correct) TrySendSmtp(m);

        busy = false; UnlockUI(); UpdateUI();
    }

    IEnumerator PostToOllama(string prompt, Action<string> done, Action<string> fail)
    {
        if (resultText) resultText.text = "Processing...";

        var payload = JsonUtility.ToJson(new GenReq { model = model, prompt = prompt });

        using (var req = new UnityWebRequest(endpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                fail?.Invoke(req.error);
                yield break;
            }

            var resp = JsonUtility.FromJson<GenResp>(req.downloadHandler.text);
            done?.Invoke(resp != null ? resp.response : "");
        }
    }

    void TrySendSmtp(Mail m)
    {
        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = smtpUseSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };
            var mail = new MailMessage(from, to, m.subject, m.response);
            client.Send(mail);
        }
        catch (Exception e)
        {
            if (resultText) resultText.text = $"{resultText.text}\nSMTP: {e.Message}";
        }
    }

    bool HasMail() => inbox != null && inbox.Length > 0 && i >= 0 && i < inbox.Length;

    void UpdateUI()
    {
        bool has = HasMail();

        if (!has)
        {
            if (subjectText) subjectText.text = "(no mail)";
            if (bodyText) bodyText.text = "";
            if (humanInput) humanInput.text = "";
            if (aiReplyButton) aiReplyButton.interactable = false;
            if (humanReplyButton) humanReplyButton.interactable = false;
            if (nextButton) nextButton.interactable = !busy;
            return;
        }

        var m = inbox[i];

        if (subjectText) subjectText.text = string.IsNullOrWhiteSpace(m.subject) ? "(no subject)" : m.subject;
        if (bodyText) bodyText.text = m.body ?? "";

        // show existing reply in input; clear if none
        if (humanInput) humanInput.text = m.answered ? (m.response ?? "") : "";

        // show last grade line (if any)
        if (resultText)
        {
            if (m.answered)
                resultText.text = m.correct ? $"{m.responder.ToUpper()} • PASS" :
                                              $"{m.responder.ToUpper()} • FAIL\nWhy: {m.reason}";
            else
                resultText.text = "";
        }

        if (aiReplyButton) aiReplyButton.interactable = !busy && !m.answered;
        if (humanReplyButton) humanReplyButton.interactable = !busy && !m.answered;
        if (nextButton) nextButton.interactable = !busy;
    }

    void LockUI()
    {
        if (aiReplyButton) aiReplyButton.interactable = false;
        if (humanReplyButton) humanReplyButton.interactable = false;
        if (nextButton) nextButton.interactable = false;
        if (resultText) resultText.text = "Processing...";
    }

    void UnlockUI() => UpdateUI();

    // ---------- Helpers: JSON cleanup & tolerant parsing ----------

    static string SanitizeToJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        string s = raw.Trim();

        // strip code fences / markdown
        s = Regex.Replace(s, @"^```[a-zA-Z]*\s*|\s*```$", "", RegexOptions.Singleline);

        // extract first {...}
        int a = s.IndexOf('{');
        int b = s.LastIndexOf('}');
        if (a >= 0 && b >= a) s = s.Substring(a, b - a + 1);

        // replace smart quotes
        s = s.Replace("“", "\"").Replace("”", "\"").Replace("’", "'");

        // remove trailing commas before } or ]
        s = Regex.Replace(s, @",\s*(\}|\])", "$1");

        // collapse whitespace
        s = Regex.Replace(s, @"\s+", " ");
        return s.Trim();
    }

    static EmailJson TryParseEmail(string json)
    {
        try
        {
            var ej = JsonUtility.FromJson<EmailJson>(json);
            if (ej != null && (!string.IsNullOrEmpty(ej.subject) || !string.IsNullOrEmpty(ej.body)))
                return ej;
        }
        catch { }

        // Fallback: regex extraction
        string subj = MatchJsonString(json, "\"subject\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        string body = MatchJsonString(json, "\"body\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        string exp = MatchJsonString(json, "\"expected\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        string par = MatchJsonString(json, "\"parentName\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        if (!string.IsNullOrEmpty(subj) || !string.IsNullOrEmpty(body))
            return new EmailJson { subject = subj, body = body, expected = exp, parentName = par };

        return null;
    }

    static GradeJson ParseGradeJson(string json)
    {
        try
        {
            var g = JsonUtility.FromJson<GradeJson>(json);
            if (g != null && !string.IsNullOrEmpty(g.decision)) return g;
        }
        catch { }

        string dec = MatchJsonString(json, "\"decision\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        string rea = MatchJsonString(json, "\"reason\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        if (!string.IsNullOrEmpty(dec) || !string.IsNullOrEmpty(rea))
            return new GradeJson { decision = dec, reason = Unescape(rea) };
        return null;
    }

    static string MatchJsonString(string text, string pattern)
    {
        var m = Regex.Match(text, pattern, RegexOptions.Singleline);
        return m.Success ? m.Groups[1].Value : null;
    }

    static string Unescape(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t")
                .Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    static string EnsureParentNameInBody(string body, out string name)
    {
        name = GuessParentFromBody(body);
        if (!string.IsNullOrWhiteSpace(name) && body.Contains(name))
            return body;

        // If missing, append a clean sign-off with a default name.
        var fallback = string.IsNullOrWhiteSpace(name) ? "Jordan Rivera" : name;
        name = fallback;

        // if body already ends with a sign-off-like line, keep it; else append
        string trimmed = body.TrimEnd();
        if (!Regex.IsMatch(trimmed, @"(?i)(best|kind|thanks|regards)[^,\n]*,\s*\S"))
            trimmed += $"\n\nBest regards,\n{fallback}";
        else if (!trimmed.EndsWith(fallback))
            trimmed += $"\n{fallback}";

        return trimmed;
    }

    static string GuessParentFromBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        // Simple guess: last non-empty line after a common sign-off
        var lines = body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int idx = lines.Length - 1; idx >= 0; idx--)
        {
            var line = lines[idx].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            // Likely a name if 2+ words with capitals
            if (Regex.IsMatch(line, @"^[A-Z][a-z]+(\s+[A-Z][a-z]+){1,3}$"))
                return line;
        }
        return null;
    }
}
