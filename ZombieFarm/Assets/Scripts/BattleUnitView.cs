using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text hpLabel;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (hpLabel == null) hpLabel = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(Color color, string unitName, int maxHp)
    {
        if (image == null) image = GetComponent<Image>();
        if (hpLabel == null) hpLabel = GetComponentInChildren<TMP_Text>();
        image.color = color;
        if (nameLabel != null) nameLabel.text = unitName;
        SetHp(maxHp);
    }

    public void SetHp(int hp)
    {
        if (hpLabel != null) hpLabel.text = Mathf.Max(0, hp).ToString();
    }

    public void PlayHit()
    {
        StopAllCoroutines();
        StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        if (image == null) yield break;
        Color original = image.color;
        image.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        image.color = original;
    }

    public void Die() => gameObject.SetActive(false);
}
