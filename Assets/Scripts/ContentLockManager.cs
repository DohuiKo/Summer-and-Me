using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContentLockManager : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform target;

    [Header("Behavior")]
    public bool lockOnCenter = true;
    public bool unlockManually = true;

    [Header("Trigger (Center-based)")]
    public bool triggerAtCenter = true;
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f;

    [Header("UI Refs")]
    public GameObject unlockButton;
    public float fadeDuration = 1f;

    // 내부 상태
    private bool isLocked = false;
    private bool centerArmed = true;
    private CanvasGroup unlockButtonCanvasGroup;

    // ScrollRect 상태 저장/복원
    bool prevEnabled, prevVertical, prevHorizontal, prevInertia;
    bool saved = false;

    Canvas canvas;
    Camera uiCam;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true);
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!target) target = transform as RectTransform;

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (unlockButton != null)
        {
            unlockButtonCanvasGroup = unlockButton.GetComponent<CanvasGroup>();
            if (unlockButtonCanvasGroup == null)
            {
                unlockButtonCanvasGroup = unlockButton.AddComponent<CanvasGroup>();
            }
            
            unlockButtonCanvasGroup.alpha = 0f;
            unlockButtonCanvasGroup.interactable = false;
            unlockButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrolled);
        Invoke(nameof(Evaluate), 0.05f);
    }
    
    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    void OnScrolled(Vector2 _) => Evaluate();

    void LateUpdate()
    {
        if (scrollRect) Evaluate();
    }

    // ───────────────────── Core ─────────────────────
    void Evaluate()
    {
        if (!viewport || !target) return;

        bool centered = triggerAtCenter ? IsCentered() : false;

        if (centered)
        {
            bool canLockNow = lockOnCenter && centerArmed && !isLocked;
            if (canLockNow) LockScroll();
        }
        else
        {
            centerArmed = true;
        }
    }
    
    // ───────────────── Visibility / Center ──────────
    bool IsCentered()
    {
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(target);

        Vector2 vpCenter = rv.center;
        if (!rt.Contains(vpCenter)) return false;

        float dy = Mathf.Abs(vpCenter.y - rt.center.y);
        float tolPx = rt.height * Mathf.Max(0f, centerTolerance);
        return dy <= tolPx;
    }

    Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        for (int i=0;i<4;i++) c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);
        float xMin = Mathf.Min(c[0].x,c[1].x,c[2].x,c[3].x);
        float xMax = Mathf.Max(c[0].x,c[1].x,c[2].x,c[3].x);
        float yMin = Mathf.Min(c[0].y,c[1].y,c[2].y,c[3].y);
        float yMax = Mathf.Max(c[0].y,c[1].y,c[2].y,c[3].y);
        return new Rect(xMin,yMin,xMax-xMin,yMax-yMin);
    }

    // ───────── Lock / Unlock ─────────
    void LockScroll()
    {
        if (!scrollRect || isLocked) return;

        prevEnabled = scrollRect.enabled;
        prevVertical = scrollRect.vertical;
        prevHorizontal = scrollRect.horizontal;
        prevInertia = scrollRect.inertia;
        saved = true;

        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
        scrollRect.velocity = Vector2.zero;

        isLocked = true;
        centerArmed = false;
    }

    public void UnlockScroll()
    {
        if (!scrollRect || !isLocked) return;

        if (saved)
        {
            scrollRect.enabled = prevEnabled;
            scrollRect.vertical = prevVertical;
            scrollRect.horizontal = prevVertical;
            scrollRect.inertia = prevInertia;
        }
        else
        {
            scrollRect.enabled = true;
            scrollRect.vertical = true;
            scrollRect.inertia = true;
        }
        isLocked = false;
        
        if (unlockButtonCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(unlockButtonCanvasGroup, 0f, fadeDuration, false));
        }
    }

    public void ShowUnlockButton()
    {
        if (unlockButtonCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(unlockButtonCanvasGroup, 1f, fadeDuration, true));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration, bool enableOnComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        if (!enableOnComplete)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (canvasGroup.gameObject != null)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;

        if (enableOnComplete)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (!enableOnComplete && canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }
}