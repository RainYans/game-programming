using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// One selectable farm/level card in the level-select panel. This is a REAL, inspectable
/// GameObject (not generated at runtime): drop a MissionData into `mission`, set whether it is
/// available from the start or gated behind `unlockAfter`, and the child UI bits are auto-found
/// by name. LevelSelectPanel (class CityMapPanel) reads these to fill text + lock state.
///
/// To add a level: duplicate a Card GameObject in the hierarchy, assign its `mission`, and add it
/// to the panel's Cards list.
public class LevelCard : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The mission this card raids. Leave empty for a 'Coming soon' placeholder card.")]
    public MissionData mission;

    [Tooltip("Playable from the very first time the level-select opens.")]
    public bool availableAtStart = true;

    [Tooltip("If not available-at-start, this card unlocks only once ALL these missions are cleared.")]
    public MissionData[] unlockAfter = new MissionData[0];

    [Header("UI refs (auto-found by child name if left empty)")]
    public Button raidButton;
    public Image thumbnail;
    public TMP_Text titleText;
    public TMP_Text hintText;
    public TMP_Text rewardText;
    public TMP_Text statusText;
    public GameObject lockOverlay;

    /// Resolve any unset UI references by child name. Safe to call repeatedly.
    public void AutoBind()
    {
        if (raidButton == null) { var t = transform.Find("RaidBtn"); if (t != null) raidButton = t.GetComponent<Button>(); }
        if (thumbnail == null) { var t = transform.Find("Thumb"); if (t != null) thumbnail = t.GetComponent<Image>(); }
        if (titleText == null) { var t = transform.Find("Title"); if (t != null) titleText = t.GetComponent<TMP_Text>(); }
        if (hintText == null) { var t = transform.Find("Hint"); if (t != null) hintText = t.GetComponent<TMP_Text>(); }
        if (rewardText == null) { var t = transform.Find("Reward"); if (t != null) rewardText = t.GetComponent<TMP_Text>(); }
        if (statusText == null) { var t = transform.Find("Status"); if (t != null) statusText = t.GetComponent<TMP_Text>(); }
        if (lockOverlay == null) { var t = transform.Find("LockOverlay"); if (t != null) lockOverlay = t.gameObject; }
    }
}
