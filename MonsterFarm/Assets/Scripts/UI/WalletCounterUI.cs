using TMPro;
using UnityEngine;

/// Shows the wallet balance in a TMP label, refreshed on every wallet change.
public class WalletCounterUI : MonoBehaviour
{
    [SerializeField] private Wallet wallet;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string prefix = "Resources: ";

    private void Awake()
    {
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (label == null) label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (wallet != null) wallet.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (label != null && wallet != null)
            label.text = prefix + wallet.Resources;
    }
}
