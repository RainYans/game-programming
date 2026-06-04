using UnityEngine;

/// A doorway between two battle rooms. Closed by default — its Collider2D blocks the leader.
/// BattleManager calls Open() when the previous stage clears: the collider turns off and the
/// sprite shifts to an "open" tint so the player can see (and walk through) the path forward.
public class BattleGate : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Collider2D blocker;
    [SerializeField] private Color closedColor = new Color(0.78f, 0.20f, 0.20f, 1f);
    [SerializeField] private Color openColor = new Color(0.30f, 0.65f, 0.30f, 0.45f);

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (blocker == null) blocker = GetComponent<Collider2D>();
        ApplyClosed();
    }

    public void Open()
    {
        IsOpen = true;
        if (blocker != null) blocker.enabled = false;
        if (sprite != null) sprite.color = openColor;
        SfxManager.Play(SfxKind.GateOpen);
    }

    public void Close()
    {
        IsOpen = false;
        ApplyClosed();
    }

    private void ApplyClosed()
    {
        IsOpen = false;
        if (blocker != null) blocker.enabled = true;
        if (sprite != null) sprite.color = closedColor;
    }
}
