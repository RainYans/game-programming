using TMPro;
using UnityEngine;

/// A tiny floating-text used as the "+5", "Dodge!", "-2" popups above agents when they're hit
/// or healed. Self-contained: it owns its lifetime and animation, so it survives even if the
/// agent that spawned it is destroyed in the same frame (e.g. on a killing blow).
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float floatSpeed = 1.1f;
    private TextMeshPro tmp;
    private float t;
    private Color startColor;

    public static DamagePopup Spawn(Vector3 worldPos, string text, Color color)
    {
        var go = new GameObject("DmgPopup", typeof(RectTransform));
        go.transform.position = worldPos + new Vector3(0f, 0.7f, 0f);
        var tmp = go.AddComponent<TextMeshPro>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = 2.6f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 20;
        var popup = go.AddComponent<DamagePopup>();
        popup.tmp = tmp;
        popup.startColor = color;
        return popup;
    }

    private void Awake()
    {
        if (tmp == null) tmp = GetComponent<TextMeshPro>();
        if (tmp != null) startColor = tmp.color;
    }

    private void Update()
    {
        t += Time.deltaTime;
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
        if (tmp != null)
        {
            float a = Mathf.Clamp01(1f - t / lifetime);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, a);
        }
        if (t >= lifetime) Destroy(gameObject);
    }
}
