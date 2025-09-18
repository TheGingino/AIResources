// PCScreenInteraction.cs
// Drop this on the 3D mesh that displays the UI RenderTexture.
// Requires: Collider on the mesh, EventSystem in scene, Canvas with GraphicRaycaster rendered by UICamera to a RenderTexture.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class PCScreenInteraction : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The camera the player uses to look at the 3D world (NOT the UI camera).")]
    public Camera worldCamera;

    [Tooltip("The camera that renders your UI to the RenderTexture.")]
    public Camera uiCamera;

    [Tooltip("The Canvas that is being rendered by the UI camera.")]
    public Canvas uiCanvas;

    [Tooltip("The RenderTexture that the UI camera renders into (shown on the mesh).")]
    public RenderTexture renderTexture;

    [Header("Options")]
    [Tooltip("Flip Y if your UV mapping makes input feel vertically inverted.")]
    public bool invertY = false;

    [Tooltip("Max distance for raycasts from world camera to this screen.")]
    public float maxHitDistance = 100f;

    [Tooltip("Treat interaction only when directly looking at the screen (prevents interacting through walls).")]
    public bool requireHitToInteract = true;

    // Internal state
    private GameObject _currentHover;
    private GameObject _pressedGo;
    private bool _dragging;
    private PointerEventData _pointerData;
    private EventSystem _es;
    private Collider _collider;

    void Awake()
    {
        _es = EventSystem.current;
        if (_es == null)
        {
            Debug.LogError("PCScreenInteraction: No EventSystem found in scene. Add one via GameObject > UI > Event System.");
        }

        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            Debug.LogError("PCScreenInteraction: Requires a Collider on the same GameObject.");
        }

        if (uiCanvas == null || uiCamera == null || renderTexture == null)
        {
            Debug.LogWarning("PCScreenInteraction: Please assign uiCanvas, uiCamera, and renderTexture.");
        }
    }

    void Update()
    {
        if (_es == null || _collider == null || uiCamera == null || uiCanvas == null || renderTexture == null)
            return;

        // 1) Find pointer position over the 3D screen
        if (!TryGetUIPixelFromWorldPointer(out Vector2 uiPixel, out bool hit))
        {
            // If we didn’t hit the screen, clear hover and exit (unless we want to allow dragging off-screen)
            if (requireHitToInteract && _currentHover != null)
            {
                SendPointerExit();
            }
            return;
        }

        // 2) Build PointerEventData in UI (RenderTexture) pixel space
        if (_pointerData == null) _pointerData = new PointerEventData(_es);
        _pointerData.Reset();
        _pointerData.position = uiPixel;       // This is treated like “screen” pixels for the UI camera
        _pointerData.scrollDelta = Input.mouseScrollDelta;

        // 3) Raycast into UI
        var results = RaycastUI(_pointerData);

        // 4) Handle hover enter/exit
        HandleHover(results);

        // 5) Buttons & drag
        HandleMouseButtons(results);

        // 6) Scroll (over current hover)
        if (_currentHover != null && _pointerData.scrollDelta.sqrMagnitude > 0.0f)
        {
            ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.scrollHandler);
        }
    }

    // Convert a world-space pointer (mouse cursor ray) to UI pixel coords inside the RenderTexture
    private bool TryGetUIPixelFromWorldPointer(out Vector2 uiPixel, out bool hit)
    {
        uiPixel = Vector2.zero;
        hit = false;

        if (worldCamera == null) worldCamera = Camera.main;
        if (worldCamera == null) return false;

        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
        if (!_collider.Raycast(ray, out RaycastHit rh, maxHitDistance))
        {
            return false;
        }

        hit = true;

        // UV from the mesh hit (0..1)
        Vector2 uv = rh.textureCoord;

        // Some meshes/materials flip the RT vertically; toggle via inspector if needed
        if (invertY) uv.y = 1f - uv.y;

        // Convert to pixels in the RT
        float px = uv.x * renderTexture.width;
        float py = uv.y * renderTexture.height;

        // Clamp to texture bounds (avoids edge cases)
        px = Mathf.Clamp(px, 0.5f, renderTexture.width - 0.5f);
        py = Mathf.Clamp(py, 0.5f, renderTexture.height - 0.5f);

        uiPixel = new Vector2(px, py);
        return true;
    }

    private List<RaycastResult> RaycastUI(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();

        // EventSystem.RaycastAll uses all active raycasters, including our UI Canvas's GraphicRaycaster.
        _es.RaycastAll(eventData, results);

        // Optional: prioritize by sorting (usually already sorted by distance/sort order)
        return results;
    }

    private void HandleHover(List<RaycastResult> results)
    {
        GameObject newHover = results.Count > 0 ? results[0].gameObject : null;

        if (newHover != _currentHover)
        {
            if (_currentHover != null)
                ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.pointerExitHandler);

            _currentHover = newHover;

            if (_currentHover != null)
                ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.pointerEnterHandler);
        }
    }

    private void HandleMouseButtons(List<RaycastResult> results)
    {
        // Left / Right / Middle buttons
        HandleButton(results, 0, PointerEventData.InputButton.Left);
        HandleButton(results, 1, PointerEventData.InputButton.Right);
        HandleButton(results, 2, PointerEventData.InputButton.Middle);
    }

    private void HandleButton(List<RaycastResult> results, int mouseButton, PointerEventData.InputButton buttonId)
    {
        bool down = Input.GetMouseButtonDown(mouseButton);
        bool held = Input.GetMouseButton(mouseButton);
        bool up   = Input.GetMouseButtonUp(mouseButton);

        if (!down && !held && !up) return;

        _pointerData.button = buttonId;

        GameObject target = results.Count > 0 ? results[0].gameObject : null;

        if (down)
        {
            _pointerData.pressPosition = _pointerData.position;
            _pointerData.pointerPressRaycast = results.Count > 0 ? results[0] : default;
            _pressedGo = ExecuteEvents.ExecuteHierarchy(target, _pointerData, ExecuteEvents.pointerDownHandler);

            if (_pressedGo == null) _pressedGo = target;

            // begin drag
            ExecuteEvents.Execute(_pressedGo, _pointerData, ExecuteEvents.initializePotentialDrag);
        }

        if (held)
        {
            // BeginDrag when movement threshold met (Unity normally handles this; we approximate)
            if (!_dragging && _pressedGo != null && (Vector2.Distance(_pointerData.pressPosition, _pointerData.position) > 3f))
            {
                ExecuteEvents.Execute(_pressedGo, _pointerData, ExecuteEvents.beginDragHandler);
                _dragging = true;
            }

            if (_dragging)
            {
                ExecuteEvents.Execute(_pressedGo, _pointerData, ExecuteEvents.dragHandler);
            }
        }

        if (up)
        {
            // End drag if in progress
            if (_dragging && _pressedGo != null)
            {
                ExecuteEvents.Execute(_pressedGo, _pointerData, ExecuteEvents.endDragHandler);
                _dragging = false;
            }

            // Pointer Up
            ExecuteEvents.ExecuteHierarchy(target, _pointerData, ExecuteEvents.pointerUpHandler);

            // Click if we released over the same object
            GameObject clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            if (_pressedGo != null && clickHandler == _pressedGo)
            {
                ExecuteEvents.Execute(_pressedGo, _pointerData, ExecuteEvents.pointerClickHandler);
            }

            _pressedGo = null;
        }
    }

    private void SendPointerExit()
    {
        if (_currentHover != null)
        {
            ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData ?? new PointerEventData(_es), ExecuteEvents.pointerExitHandler);
            _currentHover = null;
        }
    }
}
