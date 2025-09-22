// MiniOSManagerManual.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniOSManager2 : MonoBehaviour
{
    public static MiniOSManager2 Instance { get; private set; }

    [Header("Scene Wiring")]
    public RectTransform taskbarArea;
    public Button taskbarButtonPrefab; // optional taskbar button

    // Track open apps by AppId
    readonly Dictionary<string, AppWindow> _openApps = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Register a window that already exists in the scene or that you spawned yourself.
    /// This will initialize it, track it, and (optionally) create a taskbar button.
    /// </summary>
    public void RegisterApp(AppDefinition def, AppWindow window, bool createTaskbarButton = true)
    {
        if (def == null || window == null) return;

        // Prevent duplicate registration
        if (_openApps.ContainsKey(def.AppId))
        {
            Debug.LogWarning($"App {def.AppId} is already registered.");
            return;
        }

        // Initialize the window with its definition
        window.Initialize(def);
        _openApps[def.AppId] = window;

        // Create a taskbar button if desired
        Button btn = null;
        if (createTaskbarButton && taskbarButtonPrefab && taskbarArea)
        {
            btn = Instantiate(taskbarButtonPrefab, taskbarArea);
            var label = btn.GetComponentInChildren<Text>();
            var icon = btn.GetComponentInChildren<Image>();
            if (label) label.text = def.DisplayName;
            if (icon && def.Icon) icon.sprite = def.Icon;

            btn.onClick.AddListener(() =>
            {
                if (window.gameObject.activeSelf)
                    window.ToggleMinimize();
                else
                {
                    window.gameObject.SetActive(true);
                    window.Focus();
                }
            });
        }

        // When the window is destroyed, clean up bookkeeping and taskbar button
        window.gameObject.AddComponent<OnDestroyCallback>().OnDestroyed += () =>
        {
            if (btn) Destroy(btn.gameObject);
            _openApps.Remove(def.AppId);
        };
    }

    /// <summary>
    /// Close a registered app window and remove it from tracking.
    /// </summary>
    public void CloseApp(AppWindow window)
    {
        if (!window) return;

        if (_openApps.ContainsKey(window.Definition.AppId))
            _openApps.Remove(window.Definition.AppId);

        Destroy(window.gameObject);
    }

    /// <summary>
    /// Try to get an already registered window by appId.
    /// </summary>
    public AppWindow GetApp(string appId)
    {
        return _openApps.TryGetValue(appId, out var w) ? w : null;
    }
}