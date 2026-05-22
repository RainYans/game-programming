using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattlePlayer : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform playerRow;
    [SerializeField] private Transform enemyRow;
    [SerializeField] private BattleUnitView unitViewPrefab;
    [SerializeField] private DeployController deployController;
    [SerializeField] private TMP_Text resultLabel;

    [SerializeField] private float stepDelay = 0.4f;
    [SerializeField] private Color playerColor = new Color(0.4f, 0.8f, 0.4f);
    [SerializeField] private Color enemyColor = new Color(0.8f, 0.4f, 0.4f);

    private readonly Dictionary<int, BattleUnitView> views = new Dictionary<int, BattleUnitView>();

    private void Awake()
    {
        if (deployController == null) deployController = Object.FindFirstObjectByType<DeployController>();
    }

    private void OnEnable()
    {
        if (deployController != null) deployController.BattleResolved += OnBattleResolved;
    }

    private void OnDisable()
    {
        if (deployController != null) deployController.BattleResolved -= OnBattleResolved;
    }

    private void OnBattleResolved(BattleResult result, List<BattleUnit> player, List<BattleUnit> enemy, string resultMessage)
    {
        ClearViews();
        if (resultLabel != null) resultLabel.text = string.Empty;

        SpawnTeam(player, playerRow, playerColor);
        SpawnTeam(enemy, enemyRow, enemyColor);

        StopAllCoroutines();
        StartCoroutine(Replay(result.Log, resultMessage));
    }

    private void SpawnTeam(List<BattleUnit> team, Transform row, Color color)
    {
        if (row == null || unitViewPrefab == null) return;
        foreach (BattleUnit u in team)
        {
            BattleUnitView view = Instantiate(unitViewPrefab, row);
            view.Setup(color, u.Name, u.MaxHp);
            views[u.Id] = view;
        }
    }

    private IEnumerator Replay(List<BattleEvent> log, string resultMessage)
    {
        foreach (BattleEvent ev in log)
        {
            if (ev.Type == BattleEventType.Attack)
            {
                if (views.TryGetValue(ev.TargetId, out BattleUnitView target))
                {
                    target.SetHp(ev.TargetHpAfter);
                    target.PlayHit();
                }
                yield return new WaitForSeconds(stepDelay);
            }
            else if (ev.Type == BattleEventType.Death)
            {
                if (views.TryGetValue(ev.TargetId, out BattleUnitView dead)) dead.Die();
                yield return new WaitForSeconds(stepDelay * 0.5f);
            }
        }

        if (resultLabel != null) resultLabel.text = resultMessage;
    }

    private void ClearViews()
    {
        foreach (BattleUnitView v in views.Values)
            if (v != null) Destroy(v.gameObject);
        views.Clear();
    }
}
