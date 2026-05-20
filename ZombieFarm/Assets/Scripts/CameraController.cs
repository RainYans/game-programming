using UnityEngine;
using UnityEngine.InputSystem;

/// Mouse-drag pan + scroll-wheel zoom for an orthographic 2D camera, clamped to bounds.
/// Pan uses the middle mouse button so left/right clicks stay free for gameplay.
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Zoom (orthographic size)")]
    [SerializeField] private float zoomStep = 0.5f;
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 12f;

    [Header("Pan bounds (world space, camera center)")]
    [SerializeField] private Vector2 minPosition = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 maxPosition = new Vector2(10f, 10f);

    private Camera cam;
    private Vector3 dragOrigin;
    private bool dragging;

    private void Awake() => cam = GetComponent<Camera>();

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        HandlePan(mouse);
        HandleZoom(mouse);
    }

    private void HandlePan(Mouse mouse)
    {
        if (mouse.middleButton.wasPressedThisFrame)
        {
            dragging = true;
            dragOrigin = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        }
        if (mouse.middleButton.wasReleasedThisFrame) dragging = false;

        if (!dragging) return;

        Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(mouse.position.ReadValue());
        Vector3 target = transform.position + difference;
        target.x = Mathf.Clamp(target.x, minPosition.x, maxPosition.x);
        target.y = Mathf.Clamp(target.y, minPosition.y, maxPosition.y);
        transform.position = target;
    }

    private void HandleZoom(Mouse mouse)
    {
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll > 0.01f)
            cam.orthographicSize = Mathf.Max(minOrthoSize, cam.orthographicSize - zoomStep);
        else if (scroll < -0.01f)
            cam.orthographicSize = Mathf.Min(maxOrthoSize, cam.orthographicSize + zoomStep);
    }
}
