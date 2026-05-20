using System;
using UnityEngine;

/// Player resource currency. Mission rewards add here; the shop will spend from here (M4).
public class Wallet : MonoBehaviour
{
    [SerializeField] private int resources = 0;

    public int Resources => resources;
    public event Action Changed;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        resources += amount;
        Changed?.Invoke();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || resources < amount) return false;
        resources -= amount;
        Changed?.Invoke();
        return true;
    }
}
