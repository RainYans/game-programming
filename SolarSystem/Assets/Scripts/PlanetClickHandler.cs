using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to any planet or moon that should be clickable.
/// Requires a Collider (SphereCollider) on the same GameObject.
/// </summary>
public class PlanetClickHandler : MonoBehaviour
{
    [Header("Planet Info (shown on click)")]
    public string planetName = "Planet";

    [TextArea(2, 5)]
    public string funFact = "A fun fact about this planet!";

    [Header("Pulse Visual Effect")]
    [Tooltip("How much bigger the planet briefly grows when clicked (1.3 = 30% bigger)")]
    public float pulseScale = 1.3f;
    [Tooltip("How long the pulse animation takes in seconds")]
    public float pulseDuration = 0.25f;

    [Header("Highlight Color")]
    [Tooltip("The Renderer on this planet (auto-found if left empty)")]
    public Renderer planetRenderer;
    [Tooltip("Color to flash briefly on click. Set alpha to 0 to disable.")]
    public Color highlightColor = new Color(1f, 1f, 0.4f, 1f);
    public float highlightDuration = 0.4f;

    private Vector3 originalScale;
    private Color originalColor;
    private bool isPulsing = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (planetRenderer == null)
            planetRenderer = GetComponent<Renderer>();

        if (planetRenderer != null)
            originalColor = planetRenderer.material.color;
    }

    // OnMouseDown requires a Collider on the GameObject
    void OnMouseDown()
    {
        if (SolarSystemController.Instance != null)
            SolarSystemController.Instance.FocusOn(transform, planetName, funFact);

        if (!isPulsing)
            StartCoroutine(PulseEffect());

        if (planetRenderer != null && highlightColor.a > 0f)
            StartCoroutine(FlashColor());
    }

    IEnumerator PulseEffect()
    {
        isPulsing = true;
        Vector3 bigScale = originalScale * pulseScale;
        float elapsed = 0f;

        // Grow
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, bigScale, elapsed / pulseDuration);
            yield return null;
        }

        elapsed = 0f;

        // Shrink back
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(bigScale, originalScale, elapsed / pulseDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        isPulsing = false;
    }

    IEnumerator FlashColor()
    {
        planetRenderer.material.color = highlightColor;
        yield return new WaitForSeconds(highlightDuration);
        planetRenderer.material.color = originalColor;
    }
}
