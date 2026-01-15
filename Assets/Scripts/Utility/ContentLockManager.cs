using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContentLockManager : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform target;
    public bool useChildGraphicCenter = true;
    public string[] excludeGraphicNameContains = new string[] { "modal", "layer", "bg", "background", "modeload", "mode", "load" };
    public string lockTargetName = "LockTarget";

    [Header("Behavior")]
    // 0_prolog ?? ÏºúÏßê 
    // SummerRoom ?? Í∫ºÏßê 
    public bool lockOnCenter = false; 
    public bool unlockManually = true;
    public bool lockOncePerPage = true;

    [Header("Trigger (Center-based)")]
    public bool triggerAtCenter = true;
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f;
    public bool usePixelTolerance = true;
    public float centerTolerancePx = 20f;

    [Header("Center Tuning")]
    public bool autoTightenCenterTolerance = true;
    public float autoCenterTolerancePx = 5f;
    public float autoCenterToleranceRatio = 0.01f;

    [Header("UI Refs")]
    public GameObject unlockButton; 
    public float fadeDuration = 1f;

    [Header("Snap to Center (Optional)")]
    public bool snapOnApproach = false;
    public bool snapOnlyInProlog = true;
    public bool snapUseVisibleRatio = true;
    [Range(0.05f, 1f)] public float snapThreshold = 0.25f;
    public float snapDuration = 0.35f;
    public bool snapStopVelocity = true;
    public bool snapDisableInertia = true;
    public bool autoEnableSnapInProlog = true;

    [Header("Snap Tuning")]
    public bool autoTuneSnap = true;
    public float autoSnapThreshold = 0.65f;
    public float autoSnapDuration = 0.6f;

    [Header("Auto Snap (Chapter 4)")]
    public bool autoEnableSnapInChapter4 = true;

    [Header("Scroll Tuning")]
    public bool autoTuneScrollSensitivity = true;
    public float autoScrollSensitivity = 1.5f;

    [Header("Scroll Bounds")]
    public bool autoClampScroll = true;
    public float autoElasticity = 0f;

    [Header("Auto Lock (Prolog Only)")]
    public bool autoEnableLockInProlog = true;

    [Header("LockTarget")]
    public bool autoCenterLockTarget = true;

    [Header("Debug")]
    public bool debugLog = false;
    public float debugInterval = 0.5f;

    // ?¥Î? ?ÅÌÉú
    private bool isLocked = false;
    private bool centerArmed = true;
    private Coroutine snapCo;
    private bool snapSaved = false;
    private bool snapPrevInertia = false;
    private bool pendingLockAfterSnap = false;
    private float nextDebugTime = 0f;
    private bool snapArmed = true;
    private bool hasLockedOnce = false;
    private bool unlockRequested = false;
    private Button unlockBtnComponent;
    private bool lockedByThis = false;
    private bool lockedContentPosSaved = false;
    private Vector2 lockedContentPos;
    private Coroutine lockEnforceCo;
    private bool layoutReady = false;

    // ??[Ï∂îÍ????†Ïùº??ÏΩîÎìú] 
    // Í∏∞Ï°¥ isLocked Î≥Ä?òÎ? ?∏Î??êÏÑú '?ΩÍ∏∞Îß? Í∞Ä?•ÌïòÍ≤??? 
    // Î°úÏßÅ?êÎäî ?ÑÌ? ?ÅÌñ•??Ï£ºÏ? ?äÏúº???àÏã¨?òÏÑ∏??
    public bool IsLocked => isLocked;

    // ScrollRect ?ÅÌÉú ?Ä??Î≥µÏõê
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
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrolled);

        if (unlockButton && unlockBtnComponent == null)
            unlockBtnComponent = unlockButton.GetComponent<Button>();
        if (unlockBtnComponent)
            unlockBtnComponent.onClick.AddListener(UnlockContent);

        if (SceneManager.GetActiveScene().name == "0_prolog" && !lockOncePerPage)
            lockOncePerPage = true;

        if (autoEnableSnapInProlog && snapOnlyInProlog && SceneManager.GetActiveScene().name == "0_prolog")
            snapOnApproach = true;

        if (autoEnableSnapInChapter4)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "4_what_i_say" || sceneName == "4_mirror")
            {
                snapOnApproach = true;
                snapOnlyInProlog = false;
            }
        }

        if (autoEnableLockInProlog && SceneManager.GetActiveScene().name == "0_prolog")
            AutoConfigurePrologLocks();

        if (autoCenterLockTarget)
            CenterLockTarget();

        if (autoTightenCenterTolerance)
        {
            if (usePixelTolerance)
                centerTolerancePx = autoCenterTolerancePx;
            else
                centerTolerance = autoCenterToleranceRatio;
        }

        if (autoTuneSnap)
        {
            snapThreshold = autoSnapThreshold;
            snapDuration = autoSnapDuration;
        }

        if (autoTuneScrollSensitivity && scrollRect)
            scrollRect.scrollSensitivity = autoScrollSensitivity;

        if (autoClampScroll && scrollRect)
        {
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.elasticity = autoElasticity;
        }
        
        // SummerRoom (lockOnCenter=false, unlockManually=true) Ï¥àÍ∏∞ ??
        if (!lockOnCenter && unlockManually)
        {
            StartCoroutine(DelayedLock()); 
        }

        layoutReady = false;
        StartCoroutine(DelayedEvaluate());
    }
    
    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
        if (unlockBtnComponent)
            unlockBtnComponent.onClick.RemoveListener(UnlockContent);
    }

    void LateUpdate()
    {
        if (!isLocked || !scrollRect) return;
        if (scrollRect.enabled || scrollRect.vertical || scrollRect.horizontal || scrollRect.inertia)
        {
            scrollRect.enabled = false;
            scrollRect.vertical = false;
            scrollRect.horizontal = false;
            scrollRect.inertia = false;
            scrollRect.velocity = Vector2.zero;
        }
        if (lockedContentPosSaved && scrollRect.content)
            scrollRect.content.anchoredPosition = lockedContentPos;
    }
    
    // SummerRoom ?¨Ïùò ?àÏ†ï?ÅÏù∏ Ï¥àÍ∏∞ ?†Í∏à???ÑÌïú ÏΩîÎ£®??
    IEnumerator DelayedLock()
    {
        yield return null; 
        LockScroll();
    }

    IEnumerator DelayedEvaluate()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        layoutReady = true;
        Evaluate();
    }

    void OnScrolled(Vector2 _) => Evaluate();

    // ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä Core ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä
    void Evaluate()
    {
        if (!layoutReady) return;
        if (!viewport || !target) return;
        if (lockOncePerPage && hasLockedOnce && !isLocked) return;

        if (debugLog && Time.unscaledTime >= nextDebugTime)
        {
            float visibleRatio = GetVisibleRatio();
            bool centered = IsCentered();
            RectTransform effective = GetEffectiveTarget();
            string targetName = target ? target.name : "null";
            string effectiveName = effective ? effective.name : "null";
            Debug.Log($"[ContentLockManager] locked={isLocked} lockOnce={lockOncePerPage} hasLockedOnce={hasLockedOnce} lockOnCenter={lockOnCenter} triggerAtCenter={triggerAtCenter} vis={visibleRatio:F2} centered={centered} snap={(snapCo != null)} target={targetName} effectiveTarget={effectiveName}", this);
            nextDebugTime = Time.unscaledTime + Mathf.Max(0.1f, debugInterval);
        }

        if (snapArmed && ShouldSnap())
        {
            if (snapCo == null && !isLocked)
            {
                BeginSnap();
                pendingLockAfterSnap = lockOnCenter || triggerAtCenter;
                snapArmed = false;
                snapCo = StartCoroutine(SnapToCenter());
            }
        }
        else if (!snapArmed && ShouldRearmSnap())
        {
            snapArmed = true;
        }

        // ?òÎèô ?¥Ï†ú Î™®Îìú?êÏÑú???†Í∏à ?ÅÌÉú?????§ÌÅ¨Î°§ÎßÅ Ï§??†Í∏à Î°úÏßÅ???§Ìñâ?òÏ? ?äÏäµ?àÎã§.
        if (unlockManually && isLocked) return; 
        
        // Ï§ëÏïô ?ÑÎã¨ ???†Í∏à Í∏∞Îä• (?§ÌÅ¨Î°§ÎßÅ Ï§??ôÏûë)
        if (triggerAtCenter || lockOnCenter)
        {
            bool centered = IsCentered();

            if (centered)
            {
                bool canLockNow = centerArmed && !isLocked;
                if (canLockNow) LockScroll();
            }
            else
            {
                centerArmed = true;
            }
        }
    }
    
    // ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä Visibility / Center ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä
    bool IsCentered()
    {
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!scrollRect || !scrollRect.content || !viewport || !effectiveTarget) return false;

        float dy = GetCenterDistancePx(effectiveTarget);
        float tolPx = usePixelTolerance
            ? Mathf.Max(0f, centerTolerancePx)
            : effectiveTarget.rect.height * Mathf.Max(0f, centerTolerance);
        return dy <= tolPx;
    }

    float GetCenterDistancePx(RectTransform effectiveTarget)
    {
        Vector2 vpLocal;
        Vector2 tgtLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollRect.content,
            RectTransformUtility.WorldToScreenPoint(uiCam, viewport.TransformPoint(viewport.rect.center)),
            uiCam,
            out vpLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollRect.content,
            RectTransformUtility.WorldToScreenPoint(uiCam, effectiveTarget.TransformPoint(effectiveTarget.rect.center)),
            uiCam,
            out tgtLocal);
        return Mathf.Abs(vpLocal.y - tgtLocal.y);
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

    // ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä Lock / Unlock ?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä?Ä
    void LockScroll()
    {
        if (!scrollRect || isLocked) return;

        if (snapCo != null)
        {
            StopCoroutine(snapCo);
            snapCo = null;
            EndSnap();
        }
        pendingLockAfterSnap = false;

        // ?ÑÏû¨ ?§ÌÅ¨Î°??§Ï†ï ?Ä??
        prevEnabled = scrollRect.enabled;
        prevVertical = scrollRect.vertical;
        prevHorizontal = scrollRect.horizontal;
        prevInertia = scrollRect.inertia;
        saved = true;

        // ?§ÌÅ¨Î°?ÎπÑÌôú?±Ìôî (?†Í∏à)
        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
        scrollRect.velocity = Vector2.zero;

        if (scrollRect.content)
        {
            // Force content to the exact center before locking.
            scrollRect.content.anchoredPosition = GetContentPositionForCenter(scrollRect.content);
            lockedContentPos = scrollRect.content.anchoredPosition;
            lockedContentPosSaved = true;
        }

        isLocked = true;
        lockedByThis = true;
        if (lockEnforceCo == null)
            lockEnforceCo = StartCoroutine(EnforceLock());
        centerArmed = false;
        hasLockedOnce = true;
        if (debugLog)
            Debug.Log($"[ContentLockManager] LOCK by {name} target={(target ? target.name : "null")}", this);

        if (unlockRequested)
        {
            UnlockContent();
            return;
        }
    }

    // Î≤ÑÌäº OnClick ?¥Î≤§?∏Ïóê ÏßÅÏ†ë ?∞Í≤∞???®Ïàò
    public void UnlockContent() 
    {
        // UnlockContentÍ∞Ä ?∏Ï∂ú?òÎ©¥ ÏΩîÎ£®?¥ÏùÑ ?úÏûë?òÏó¨ ?àÏ†Ñ?òÍ≤å ?¥Ï†ú Î∞?ÎπÑÌôú?±Ìôî
        if (!isLocked)
        {
            if (!IsCentered()) return;
            unlockRequested = true;
            if (debugLog)
                Debug.Log($"[ContentLockManager] UNLOCK requested before lock by {name} target={(target ? target.name : "null")}", this);
            return;
        }
        StartCoroutine(UnlockAndDisableCoroutine());
    }

    IEnumerator UnlockAndDisableCoroutine()
    {
        yield return null; // ???ÑÎ†à???ÄÍ∏?

        if (!scrollRect || !isLocked) yield break;

        // ?Ä?•Îêú ?§Ï†ï Î≥µÏõê (?§ÌÅ¨Î°??†Í∏à ?¥Ï†ú Î°úÏßÅ)
        if (saved)
        {
            scrollRect.enabled = prevEnabled;
            scrollRect.vertical = prevVertical;
            scrollRect.horizontal = prevHorizontal; 
            scrollRect.inertia = prevInertia;
        }
        else 
        {
            scrollRect.enabled = true;
            scrollRect.vertical = true;
            scrollRect.horizontal = true;
            scrollRect.inertia = true;
        }
        isLocked = false;
        hasLockedOnce = true;
        unlockRequested = false;
        lockedContentPosSaved = false;
        if (lockedByThis)
        {
            scrollRect.enabled = true;
            scrollRect.vertical = true;
            lockedByThis = false;
        }
        if (lockEnforceCo != null)
        {
            StopCoroutine(lockEnforceCo);
            lockEnforceCo = null;
        }
        if (debugLog)
            Debug.Log($"[ContentLockManager] UNLOCK by {name} target={(target ? target.name : "null")}", this);
        
        // ‚≠??µÏã¨: ?§ÌÅ¨Î°??†Í∏à ?¥Ï†ú ?????§ÌÅ¨Î¶ΩÌä∏ Ïª¥Ìè¨?åÌä∏Î•?ÎπÑÌôú?±Ìôî?òÏó¨ ?¨Ïû†Í∏àÏùÑ ?ÅÍµ¨ Ï§ëÎã®
        // Keep this component enabled so later pages can lock again.
    }

    // Prologue ?¨Ïùò ?§Î•∏ ?§ÌÅ¨Î¶ΩÌä∏ ?∏Ìôò?±ÏùÑ ?ÑÌïú ?®Ïàò
    IEnumerator EnforceLock()
    {
        var wait = new WaitForEndOfFrame();
        while (isLocked && scrollRect)
        {
            yield return wait;
            scrollRect.enabled = false;
            scrollRect.vertical = false;
            scrollRect.horizontal = false;
            scrollRect.inertia = false;
            scrollRect.velocity = Vector2.zero;
            if (lockedContentPosSaved && scrollRect.content)
                scrollRect.content.anchoredPosition = lockedContentPos;
        }
        lockEnforceCo = null;
    }

    public void ShowUnlockButton()
    {
        if (unlockButton != null)
        {
            unlockButton.SetActive(true); 
        }
    }

    void AutoConfigurePrologLocks()
    {
        lockOnCenter = true;
        triggerAtCenter = true;
    }

    bool ShouldSnap()
    {
        if (!snapOnApproach) return false;
        if (snapOnlyInProlog && SceneManager.GetActiveScene().name != "0_prolog") return false;
        if (!scrollRect || !scrollRect.content || !viewport || !GetEffectiveTarget()) return false;
        if (!triggerAtCenter && !lockOnCenter) return false;

        if (snapUseVisibleRatio)
        {
            RectTransform effectiveTarget = GetEffectiveTarget();
            float area = effectiveTarget ? Mathf.Abs(effectiveTarget.rect.width * effectiveTarget.rect.height) : 0f;
            if (area > 1f)
                return GetVisibleRatio() >= Mathf.Clamp01(snapThreshold);
        }

        RectTransform targetRt = GetEffectiveTarget();
        float dy = GetCenterDistancePx(targetRt);
        float thresholdPx = Mathf.Clamp01(snapThreshold) * viewport.rect.height;
        return dy <= thresholdPx;
    }

    bool ShouldRearmSnap()
    {
        if (!snapOnApproach) return false;
        if (!scrollRect || !scrollRect.content || !viewport || !GetEffectiveTarget()) return false;

        if (snapUseVisibleRatio)
        {
            RectTransform effectiveTarget = GetEffectiveTarget();
            float area = effectiveTarget ? Mathf.Abs(effectiveTarget.rect.width * effectiveTarget.rect.height) : 0f;
            if (area > 1f)
                return GetVisibleRatio() < Mathf.Clamp01(snapThreshold);
        }

        RectTransform targetRt = GetEffectiveTarget();
        float dy = GetCenterDistancePx(targetRt);
        float thresholdPx = Mathf.Clamp01(snapThreshold) * viewport.rect.height;
        return dy > thresholdPx;
    }

    float GetVisibleRatio()
    {
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!effectiveTarget) return 0f;
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(effectiveTarget);
        if (rt.width <= 0f || rt.height <= 0f) return 0f;

        float xMin = Mathf.Max(rv.xMin, rt.xMin);
        float yMin = Mathf.Max(rv.yMin, rt.yMin);
        float xMax = Mathf.Min(rv.xMax, rt.xMax);
        float yMax = Mathf.Min(rv.yMax, rt.yMax);

        float iw = Mathf.Max(0f, xMax - xMin);
        float ih = Mathf.Max(0f, yMax - yMin);
        float interArea = iw * ih;
        float targetArea = rt.width * rt.height;
        return targetArea > 0f ? (interArea / targetArea) : 0f;
    }

    void BeginSnap()
    {
        if (!scrollRect) return;
        if (snapStopVelocity) scrollRect.velocity = Vector2.zero;

        if (snapDisableInertia && !snapSaved)
        {
            snapSaved = true;
            snapPrevInertia = scrollRect.inertia;
            scrollRect.inertia = false;
        }
    }

    void EndSnap()
    {
        if (!scrollRect) return;
        if (snapSaved)
        {
            scrollRect.inertia = snapPrevInertia;
            snapSaved = false;
        }
    }

    IEnumerator SnapToCenter()
    {
        if (!scrollRect || !scrollRect.content || !viewport || !GetEffectiveTarget())
        {
            snapCo = null;
            yield break;
        }

        RectTransform content = scrollRect.content;
        Vector2 start = content.anchoredPosition;
        Vector2 targetPos = GetContentPositionForCenter(content);

        float t = 0f;
        float dur = Mathf.Max(0.05f, snapDuration);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            content.anchoredPosition = Vector2.Lerp(start, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        content.anchoredPosition = targetPos;
        EndSnap();
        if (pendingLockAfterSnap && !isLocked && (lockOnCenter || triggerAtCenter) && !(lockOncePerPage && hasLockedOnce))
        {
            pendingLockAfterSnap = false;
            LockScroll();
        }
        snapCo = null;
    }

    Vector2 GetContentPositionForCenter(RectTransform content)
    {
        Vector3 vpCenterW = viewport.TransformPoint(viewport.rect.center);
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!effectiveTarget) return content.anchoredPosition;
        Vector3 tgtCenterW = effectiveTarget.TransformPoint(effectiveTarget.rect.center);

        Vector2 vpLocal;
        Vector2 tgtLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content, RectTransformUtility.WorldToScreenPoint(uiCam, vpCenterW), uiCam, out vpLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content, RectTransformUtility.WorldToScreenPoint(uiCam, tgtCenterW), uiCam, out tgtLocal);

        Vector2 delta = vpLocal - tgtLocal;
        return content.anchoredPosition + new Vector2(0f, delta.y);
    }

    RectTransform GetEffectiveTarget()
    {
        RectTransform namedTarget = FindLockTarget();
        if (namedTarget) return namedTarget;
        if (!useChildGraphicCenter || !target) return target;

        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);
        RectTransform best = null;
        float bestArea = 0f;

        foreach (Graphic graphic in graphics)
        {
            if (!graphic) continue;
            RectTransform rt = graphic.rectTransform;
            if (!rt) continue;
            if (rt == target) continue;
            if (ShouldExcludeGraphic(rt.name)) continue;
            if (!HasRenderableSource(graphic)) continue;
            Rect r = rt.rect;
            float area = Mathf.Abs(r.width * r.height);
            if (area <= 0f) continue;
            if (area > bestArea)
            {
                bestArea = area;
                best = rt;
            }
        }

        return best ? best : target;
    }

    RectTransform FindLockTarget()
    {
        if (!target || string.IsNullOrEmpty(lockTargetName)) return null;
        RectTransform[] children = target.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform child in children)
        {
            if (child && child.name == lockTargetName)
                return child;
        }
        return null;
    }

    void CenterLockTarget()
    {
        RectTransform lt = FindLockTarget();
        if (!lt || !target) return;
        lt.anchorMin = new Vector2(0.5f, 0.5f);
        lt.anchorMax = new Vector2(0.5f, 0.5f);
        lt.pivot = new Vector2(0.5f, 0.5f);
        lt.anchoredPosition = Vector2.zero;
        lt.sizeDelta = target.rect.size;
    }

    bool ShouldExcludeGraphic(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (excludeGraphicNameContains == null || excludeGraphicNameContains.Length == 0) return false;
        string lower = name.ToLowerInvariant();
        foreach (string token in excludeGraphicNameContains)
        {
            if (string.IsNullOrEmpty(token)) continue;
            if (lower.Contains(token.ToLowerInvariant())) return true;
        }
        return false;
    }

    bool HasRenderableSource(Graphic graphic)
    {
        Image image = graphic as Image;
        if (image != null)
            return image.sprite != null;

        RawImage raw = graphic as RawImage;
        if (raw != null)
            return raw.texture != null;

        return true;
    }
}



