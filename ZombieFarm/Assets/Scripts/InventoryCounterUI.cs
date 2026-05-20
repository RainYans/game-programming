using TMPro;
using UnityEngine;

/// Shows the inventory total in a TMP label, refreshed on every inventory change.
public class InventoryCounterUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string prefix = "Zombies: ";

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (label == null) label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (label != null && inventory != null)
            label.text = prefix + inventory.Total;
    }
}
