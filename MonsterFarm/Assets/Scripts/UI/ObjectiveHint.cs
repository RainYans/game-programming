using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// A small, always-on banner that reminds the player of the current goal. On the farm it shows a
/// fixed message (set in the Inspector). In battle (pollBattle = true) it updates live with the
/// number of enemies remaining. Lives as a real, editable object under the Canvas.
public class ObjectiveHint : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [TextArea] [SerializeField] private string message = "";
    [Tooltip("Battle scene: poll the BattleManager and show enemies-remaining.")]
    [SerializeField] private bool pollBattle = false;

    private BattleManager bm;
    private float pollTimer;

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        Apply();
    }

    private void Start()
    {
        if (pollBattle) bm = FindFirstObjectByType<BattleManager>();
    }

    private void Update()
    {
        if (!pollBattle || label == null) return;
        pollTimer -= Time.deltaTime;
        if (pollTimer > 0f) return;
        pollTimer = 0.5f;
        if (bm == null) bm = FindFirstObjectByType<BattleManager>();
        int n = bm != null ? CountAlive(bm.Enemies) : 0;
        label.text = n > 0 ? $"Goal:  defeat the enemies  —  {n} left"
                           : "Goal:  advance to clear the village";
    }

    public void SetMessage(string m) { message = m; pollBattle = false; Apply(); }

    private void Apply()
    {
        if (label != null && !string.IsNullOrEmpty(message)) label.text = message;
    }

    private static int CountAlive(IReadOnlyList<BattleAgent> list)
    {
        if (list == null) return 0;
        int c = 0;
        for (int i = 0; i < list.Count; i++) if (list[i] != null && list[i].IsAlive) c++;
        return c;
    }
}
