// AppWindow.cs

using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppWindow : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [Header("Wiring")]
    [SerializeField] private Image iconImage;
    [SerializeField] private  TextMeshProUGUI titleText;
    [SerializeField] private  RectTransform dragHandle;
    [SerializeField] private  CanvasGroup canvasGroup;

    [HideInInspector] public AppDefinition Definition;


    public TextMeshProUGUI appText;


    private RectTransform _rt;
    private Vector2 _dragOffset;
    private bool _minimized;

    void Awake()
    {
        _rt = (RectTransform)transform;
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(AppDefinition def)
    {
        Definition = def;
        if (iconImage) iconImage.sprite = def.Icon;
        if (titleText) titleText.text = def.DisplayName;
        Focus();
    }

    public void Focus()
    {
        // Sets the selected window to the front
        transform.SetSiblingIndex(transform.parent.childCount - 1);
        canvasGroup.alpha = 1f;
    }

    public void Close()
    {
        MiniOSManager.Instance.CloseApp(this);
    }

    public void ToggleMinimize()
    {
        _minimized = !_minimized;
        gameObject.SetActive(!_minimized);
    }

    public void OnClickClose() => Close();
    public void OnClickMinimize() => ToggleMinimize();

    // Focus on click anywhere inside the window
    public void OnPointerDown(PointerEventData eventData) => Focus();

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragHandle) return;
        if (!RectTransformUtility.RectangleContainsScreenPoint(dragHandle, eventData.position, eventData.pressEventCamera))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, eventData.pressEventCamera, out var localMouse);
        _dragOffset = (Vector2)_rt.localPosition - localMouse;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        if (!dragHandle) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var local))
            return;

        Debug.Log("Dragging the Window");

        
        // Keep window within parent bounds (simple clamp)
        var parent = (_rt.parent as RectTransform).rect;
        var size = _rt.rect.size;
        var target = local + _dragOffset;

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        target.x = Mathf.Clamp(target.x, parent.xMin + halfW, parent.xMax - halfW);
        target.y = Mathf.Clamp(target.y, parent.yMin + halfH, parent.yMax - halfH);

        _rt.anchoredPosition = target;
    }
}
