using TMPro;
using UnityEngine;

/// Shows the seed inventory count in a TMP label, refreshed on every seed change.
public class SeedCounterUI : MonoBehaviour
{
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string prefix = "Seeds: ";

    private void Awake()
    {
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (label == null) label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (seedInventory != null) seedInventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (seedInventory != null) seedInventory.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (label != null && seedInventory != null)
            label.text = prefix + seedInventory.Total;
    }
}
