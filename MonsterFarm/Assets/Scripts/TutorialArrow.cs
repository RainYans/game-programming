using UnityEngine;
using UnityEngine.UI;

/// A floating screen-space arrow that bobs above a world target (nearest enemy, a gate, a building)
/// to draw the new player's eye. The tutorial controllers call Point(target) / Hide(). Lives on a
/// RectTransform under the overlay canvas; converts the target's world position to canvas space each
/// frame and clamps it to the screen so it never disappears off-edge.
public class TutorialArrow : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;       // the visible chevron (defaults to this rect)
    [SerializeField] private Canvas canvas;
    [SerializeField] private float screenYOffset = 64f; // float this far above the target on screen
    [SerializeField] private float bob = 10f;
    [SerializeField] private float bobSpeed = 5f;

    private Camera cam;
    private Transform target;
    private RectTransform canvasRect;

    private void Awake()
    {
        if (arrow == null) arrow = transform as RectTransform;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        Hide();
    }

    public void Point(Transform t)
    {
        target = t;
        gameObject.SetActive(t != null);
    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null || canvasRect == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 sp = cam.WorldToScreenPoint(target.position);
        if (sp.z < 0f) { arrow.gameObject.SetActive(false); return; }
        arrow.gameObject.SetActive(true);
        sp.y += screenYOffset;
        sp.x = Mathf.Clamp(sp.x, 60f, Screen.width - 60f);
        sp.y = Mathf.Clamp(sp.y, 90f, Screen.height - 60f);

        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, uiCam, out Vector2 local))
            arrow.anchoredPosition = local + new Vector2(0f, Mathf.Sin(Time.unscaledTime * bobSpeed) * bob);
    }
}
