// MiniOSManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniOSManager : MonoBehaviour
{
    public static MiniOSManager Instance { get; private set; }

    [Header("Scene Wiring")]
    public RectTransform desktopArea;
    public RectTransform taskbarArea;
    public Button taskbarButtonPrefab; // simple Button with Icon+Label

    readonly Dictionary<string, AppWindow> _openApps = new(); // one instance per appId; change to List for multi-instance

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OpenApp(AppDefinition def)
    {
        if (_openApps.TryGetValue(def.AppId, out var alreadyOpen))
        {
            if (!alreadyOpen.gameObject.activeSelf) alreadyOpen.gameObject.SetActive(true);
            alreadyOpen.Focus();
            return;
        }

        var window = Instantiate(def.WindowPrefab, desktopArea);
        window.Initialize(def);
        _openApps[def.AppId] = window;

        // Create taskbar button
        var btn = Instantiate(taskbarButtonPrefab, taskbarArea);
        var label = btn.GetComponentInChildren<Text>();
        var icon = btn.GetComponentInChildren<Image>();
        if (label) label.text = def.DisplayName;
        if (icon && def.Icon) icon.sprite = def.Icon;

        btn.onClick.AddListener(() =>
        {
            // Toggle minimize/restore when clicking taskbar
            if (window.gameObject.activeSelf)
                window.ToggleMinimize();
            else
            {
                window.gameObject.SetActive(true);
                window.Focus();
            }
        });

        // When the window is closed, also remove the taskbar button
        window.gameObject.AddComponent<OnDestroyCallback>().OnDestroyed += () =>
        {
            if (btn) Destroy(btn.gameObject);
            _openApps.Remove(def.AppId);
        };
    }

    public void CloseApp(AppWindow window)
    {
        if (window && _openApps.ContainsKey(window.Definition.AppId))
            _openApps.Remove(window.Definition.AppId);

        if (window) Destroy(window.gameObject);
    }
}

// Small utility for cleanup
public class OnDestroyCallback : MonoBehaviour
{
    public System.Action OnDestroyed;
    void OnDestroy() => OnDestroyed?.Invoke();
}
