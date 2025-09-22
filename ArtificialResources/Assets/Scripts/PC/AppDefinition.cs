// AppDefinition.cs
using UnityEngine;

[CreateAssetMenu(menuName = "MiniOS/App Definition")]
public class AppDefinition : ScriptableObject
{
    [SerializeField] private string appId = "notes";
    [SerializeField] private string displayName = "Notes";
    [SerializeField] private Sprite icon;
    [SerializeField] private AppWindow windowPrefab;
    

    public string AppId => appId;
    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    public Sprite Icon => icon;
    public AppWindow WindowPrefab => windowPrefab;
}