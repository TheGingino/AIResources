using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppLauncherUI : MonoBehaviour
{
    [Tooltip("App entries to show in the launcher grid/panel")]
    public List<AppDefinition> apps;

    [Tooltip("Prefab for a launcher tile/button (NOT the taskbar button)")]
    public Button launcherButtonPrefab;

    [Tooltip("Parent transform for launcher buttons (e.g., PC_UI/PC_Screen/LauncherPanel)")]
    public Transform container;

    void Awake()
    {
        // Clear previous children if this is re-run in play mode
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        foreach (var app in apps)
        {
            var captured = app; // avoid any closure weirdness
            var b = Instantiate(launcherButtonPrefab, container);

            // Find icon/label on THIS button (don’t touch other UI)
            var label = b.GetComponentInChildren<Text>(true);
            var icon  = b.GetComponentInChildren<Image>(true);

            if (label) label.text = captured.DisplayName;
            if (icon && captured.Icon) icon.sprite = captured.Icon;

            b.onClick.AddListener(() => MiniOSManager.Instance.OpenApp(captured));
        }
    }
}