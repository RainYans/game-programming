using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// The "How to Play" manual — a turning picture-book overlay that teaches the core loop first
/// (plant -> harvest -> grow stronger -> raid) and then the controls. Auto-opens ONCE on the first
/// farm visit (before the step-by-step onboarding), and is re-openable from the pause menu.
///
/// LAYOUT IS DATA IN THE SCENE: this controller only swaps the per-page title, the loop-page sprite
/// + paragraph, and which controls "block" is visible. It never moves or resizes anything — so any
/// position/size tweaks you make to Illustration / Body / Title / the controls blocks in the
/// Inspector are kept. Loop illustrations are assigned per index in `pageIllustrations`.
public class ManualBookController : MonoBehaviour
{
    private class PageDef
    {
        public string title;
        public string body;          // loop-page paragraph (null on controls pages)
        public int controlsBlock;    // -1 = loop page; >=0 = index into controlsBlocks
        public PageDef(string t, string b) { title = t; body = b; controlsBlock = -1; }
        public PageDef(string t, int block) { title = t; body = null; controlsBlock = block; }
        public bool IsControls => controlsBlock >= 0;
    }

    // Page order + copy. Loop pages 0..3 take their picture from pageIllustrations[index].
    // Controls pages point at a pre-laid block in the scene (block 0 = Farm, block 1 = Battle);
    // their key-cap chips + descriptions live in those scene objects so the layout is fully editable.
    private static readonly PageDef[] Pages =
    {
        new PageDef("Grow Your Monsters",
            "Your farm is where monsters are born. Step onto tilled soil and press E to plant a " +
            "monster seed — it sprouts and grows on its own over time."),
        new PageDef("Harvest the Squad",
            "When a monster is fully grown, harvest it to add it to your roster. Tip: a monster left " +
            "hungry hits harder in battle — but is more fragile too. Feed it wisely."),
        new PageDef("Grow Stronger",
            "Spend resources at the Shop for seeds and combat items, and at the Lab to upgrade a " +
            "strain's power. Build the squad you want before you march out."),
        new PageDef("March to War",
            "When you're ready, head to the War Camp, pick a city on the map, deploy your squad and " +
            "raid it. Win to reclaim the land and earn resources — then do it all again."),
        new PageDef("Farm Controls", 0),
        new PageDef("Battle Controls", 1),
    };

    static readonly Color DotOff = new Color(0.62f, 0.50f, 0.34f, 0.55f);
    static readonly Color DotOn = new Color(0.95f, 0.80f, 0.28f, 1f);
    static readonly Color DotDone = new Color(0.36f, 0.66f, 0.30f, 1f);

    [Header("Illustrations (assign an in-game screenshot per loop page, index 0..3)")]
    [SerializeField] private Sprite[] pageIllustrations = new Sprite[6];
    [Tooltip("Optional SECOND image per loop page. When a page's entry here is set, that page shows " +
             "two images side by side (left = pageIllustrations, right = this) instead of one big one.")]
    [SerializeField] private Sprite[] pageIllustrationsRight = new Sprite[6];

    [Header("UI refs (wired in the scene)")]
    [SerializeField] private GameObject root;          // toggled overlay
    [SerializeField] private CanvasGroup canvasGroup;  // fade
    [SerializeField] private Image illustration;       // single big image (one-image pages)
    [SerializeField] private GameObject illustrationPairRoot; // shown on two-image pages
    [SerializeField] private Image illustrationLeft;
    [SerializeField] private Image illustrationRight;
    [SerializeField] private TMP_Text titleText;       // per-page title
    [SerializeField] private TMP_Text bodyText;        // loop-page paragraph
    [SerializeField] private GameObject[] controlsBlocks; // 0 = Farm controls, 1 = Battle controls
    [SerializeField] private RectTransform dotsParent; // holds one dot Image per page
    [SerializeField] private RectTransform contentRoot; // animated on turn
    [SerializeField] private CanvasGroup contentGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextLabel;       // "Next >" / "Begin" / "Close"
    [SerializeField] private Button prevButton;
    [SerializeField] private Button skipButton;        // skip-to-end (auto) / close (review)

    [Header("Input freeze (auto-resolved)")]
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [SerializeField] private float fadeTime = 0.22f;

    private int index;
    private bool isOpen, turning;
    private bool reviewMode;
    private bool ownsInput;
    private Coroutine fadeCo;

    private void Awake()
    {
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        if (canvasGroup == null && root != null) canvasGroup = root.GetComponent<CanvasGroup>();
        if (nextButton != null) { nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(Advance); }
        if (prevButton != null) { prevButton.onClick.RemoveAllListeners(); prevButton.onClick.AddListener(Back); }
        if (skipButton != null) { skipButton.onClick.RemoveAllListeners(); skipButton.onClick.AddListener(SkipOrClose); }
        HideImmediate();
    }

    private void Start()
    {
        if (!TutorialState.ManualSeen) OpenAuto();
    }

    public void OpenAuto()
    {
        reviewMode = false;
        ownsInput = true;
        SetFarmInput(false);
        Open();
    }

    public void OpenManual()
    {
        reviewMode = true;
        ownsInput = false;
        Open();
    }

    /// Opened from the on-screen "?" help button while the farm is live underneath — review mode,
    /// but it owns the input freeze (and restores it on close) so the avatar doesn't wander behind.
    public void OpenHelp()
    {
        reviewMode = true;
        ownsInput = true;
        SetFarmInput(false);
        Open();
    }

    private void Open()
    {
        if (root != null) root.SetActive(true);
        isOpen = true;
        index = 0;
        SetPage(0);
        FadeTo(1f);
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void Update()
    {
        if (!isOpen || turning) return;
        Keyboard k = Keyboard.current;
        Mouse m = Mouse.current;

        if (k != null && k.escapeKey.wasPressedThisFrame) { SkipOrClose(); return; }

        bool overButton = UnityEngine.EventSystems.EventSystem.current != null &&
                          UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        bool click = m != null && m.leftButton.wasPressedThisFrame && !overButton;
        bool key = k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
        if (click || key) Advance();
    }

    private void Advance()
    {
        if (turning) return;
        if (index >= Pages.Length - 1) { Finish(); return; }
        StartCoroutine(Turn(index + 1));
    }

    private void Back()
    {
        if (turning || index <= 0) return;
        StartCoroutine(Turn(index - 1));
    }

    private void SkipOrClose() => Finish();

    private void Finish()
    {
        TutorialState.ManualSeen = true;
        isOpen = false;
        if (ownsInput) SetFarmInput(true);
        FadeTo(0f, disableAtEnd: true);
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private IEnumerator Turn(int next)
    {
        turning = true;
        SfxManager.Play(SfxKind.ButtonClick);
        yield return Animate(1f, 0.2f, 0.16f);
        SetPage(next);
        yield return Animate(0.2f, 1f, 0.18f);
        turning = false;
    }

    private IEnumerator Animate(float a0, float a1, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float kk = t / dur;
            if (contentGroup != null) contentGroup.alpha = Mathf.Lerp(a0, a1, kk);
            yield return null;
        }
        if (contentGroup != null) contentGroup.alpha = a1;
    }

    private void SetPage(int i)
    {
        index = Mathf.Clamp(i, 0, Pages.Length - 1);
        PageDef p = Pages[index];

        if (titleText != null) titleText.text = p.title;

        if (p.IsControls)
        {
            if (illustration != null) illustration.enabled = false;
            if (bodyText != null) bodyText.gameObject.SetActive(false);
            ShowControlsBlock(p.controlsBlock);
        }
        else
        {
            ShowControlsBlock(-1);
            if (bodyText != null) { bodyText.gameObject.SetActive(true); bodyText.text = p.body; }

            Sprite a = (pageIllustrations != null && index < pageIllustrations.Length) ? pageIllustrations[index] : null;
            Sprite b = (pageIllustrationsRight != null && index < pageIllustrationsRight.Length) ? pageIllustrationsRight[index] : null;

            if (b != null)
            {
                if (illustration != null) illustration.enabled = false;
                if (illustrationPairRoot != null) illustrationPairRoot.SetActive(true);
                LayoutPair(a, b);
            }
            else
            {
                if (illustrationPairRoot != null) illustrationPairRoot.SetActive(false);
                SetImg(illustration, a);
            }
        }

        UpdateDots();
        if (prevButton != null) prevButton.gameObject.SetActive(index > 0);
        if (nextLabel != null)
            nextLabel.text = index >= Pages.Length - 1 ? (reviewMode ? "Close" : "Begin") : "Next  >";
    }

    private static void SetImg(Image img, Sprite s)
    {
        if (img == null) return;
        if (s != null) { img.sprite = s; img.color = Color.white; img.enabled = true; }
        else img.enabled = false;
    }

    /// Lay out the two page images at the SAME height (widths follow each image's own aspect, so
    /// nothing is stretched or cropped), centred as a pair with a small gap, inside the pair band.
    private void LayoutPair(Sprite a, Sprite b)
    {
        SetImg(illustrationLeft, a);
        SetImg(illustrationRight, b);
        if (illustrationLeft == null || illustrationRight == null || illustrationPairRoot == null) return;

        Canvas.ForceUpdateCanvases();
        RectTransform band = illustrationPairRoot.transform as RectTransform;
        if (band == null) return;
        Vector2 bs = band.rect.size;
        if (bs.x < 1f || bs.y < 1f) return;

        float aspA = (a != null && a.rect.height > 0) ? a.rect.width / a.rect.height : 1f;
        float aspB = (b != null && b.rect.height > 0) ? b.rect.width / b.rect.height : 1f;
        float gap = bs.x * 0.04f;

        float h = bs.y;                                   // start at full band height
        float totalW = h * aspA + gap + h * aspB;
        if (totalW > bs.x) h *= bs.x / totalW;            // shrink to fit width if needed
        float wA = h * aspA, wB = h * aspB;
        float total = wA + gap + wB;

        PlacePairImg(illustrationLeft.rectTransform, -total * 0.5f + wA * 0.5f, wA, h);
        PlacePairImg(illustrationRight.rectTransform, -total * 0.5f + wA + gap + wB * 0.5f, wB, h);
    }

    private static void PlacePairImg(RectTransform rt, float centerX, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(centerX, 0f);
    }

    /// Show only the requested controls block (or none when which < 0).
    private void ShowControlsBlock(int which)
    {
        if (controlsBlocks == null) return;
        for (int b = 0; b < controlsBlocks.Length; b++)
            if (controlsBlocks[b] != null) controlsBlocks[b].SetActive(b == which);
    }

    private void UpdateDots()
    {
        if (dotsParent == null) return;
        int i = 0;
        foreach (Transform child in dotsParent)
        {
            Image img = child.GetComponent<Image>();
            if (img == null) continue;
            img.color = i < index ? DotDone : (i == index ? DotOn : DotOff);
            i++;
        }
    }

    private void HideImmediate()
    {
        isOpen = false;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (root != null) root.SetActive(false);
    }

    private void FadeTo(float target, bool disableAtEnd = false)
    {
        if (canvasGroup == null) { if (disableAtEnd && root != null) root.SetActive(false); return; }
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(target, disableAtEnd));
    }

    private IEnumerator FadeRoutine(float target, bool disableAtEnd)
    {
        float from = canvasGroup.alpha, t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, target, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = target;
        if (disableAtEnd && target <= 0.01f && root != null) root.SetActive(false);
    }

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }
}
