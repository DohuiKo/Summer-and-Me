using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoOnVisibleLock : MonoBehaviour
{
    [Header("필수 참조")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform target;
    public VideoPlayer videoPlayer;

    [Header("W 키 재생 기능")]
    public Button unlockButton; // NextDownArrow 버튼을 연결

    [Header("Hold-to-Play")]
    public RectTransform holdTarget;
    public float holdPulseScale = 1.003f;
    public float holdPulseSpeed = 0.7f;

    [Header("Snap to Center")]
    public bool snapOnApproach = true;
    [Range(0.05f, 1f)] public float snapThreshold = 0.25f;
    public float snapDuration = 0.35f;
    public bool snapStopVelocity = true;
    public bool snapDisableInertia = true;

    [Header("External Lock")]
    public ContentLockManager contentLock;

    [Header("Debug")]
    public bool debugLog = false;
    public float debugInterval = 0.5f;

    [Header("중앙 감지 설정")]
    [Range(0f, 0.5f)] public float centerTolerance = 0.05f;
    public bool usePixelTolerance = true;
    public float centerTolerancePx = 20f;
    public bool useChildGraphicCenter = true;
    public string[] excludeGraphicNameContains = new string[] { "modal", "layer", "bg", "background", "modeload", "mode", "load" };
    public string lockTargetName = "LockTarget";

    [Header("Lock Detection")]
    [Tooltip("true = visible ratio, false = center tolerance")]
    public bool useVisibleRatio = true;
    [Tooltip("When using visible ratio, also require center alignment for locking")]
    public bool requireCenterForLock = true;
    [Range(0.05f, 1f)] public float visibleThreshold = 0.5f;
    public bool lockOncePerPage = true;
    public bool resetLockWhenOutOfView = true;
    [Range(0f, 0.2f)] public float resetVisibleRatio = 0.02f;

    [Header("End Fallback")]
    [Tooltip("Seconds before we allow manual unlock even if video end event didn't fire. 0 = disabled")]
    public float unlockTimeout = 0f;
    [Tooltip("End check tolerance (seconds) for manual end detection")]
    public float endTimeEpsilon = 0.05f;

    // --- 내부 상태 변수 ---
    private bool isReadyToPlay = false;
    private bool isLocked = false;
    private bool hasFinished = false;
    private float lockStartTime = -1f;
    private bool pendingPlay = false;
    private Vector3 holdBaseScale = Vector3.one;
    private float nextDebugTime = 0f;
    private Coroutine snapCo;
    private bool snapSaved = false;
    private bool snapPrevInertia = false;
    private bool pendingLockAfterSnap = false;
    private bool snapArmed = true;
    private bool hasLockedOnce = false;

    // ScrollRect 상태 저장/복원용 변수
    private bool prevEnabled, prevVertical, prevHorizontal, prevInertia;
    private bool saved = false;

    private Canvas canvas;
    private Camera uiCam;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true);
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!target) target = transform as RectTransform;
        if (!videoPlayer) videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0_prolog")
            useChildGraphicCenter = true;
        else
            useChildGraphicCenter = false;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0_prolog" && !lockOncePerPage)
            lockOncePerPage = true;

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (videoPlayer)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        if (!contentLock) contentLock = GetComponentInParent<ContentLockManager>(true);
        if (!holdTarget && target) holdTarget = target;
        if (holdTarget) holdBaseScale = holdTarget.localScale;
        
        if (unlockButton)
        {
            unlockButton.gameObject.SetActive(false);
            // ✨ 버튼 클릭 시 ManualUnlock 함수를 호출하도록 미리 연결
            unlockButton.onClick.AddListener(ManualUnlock); 
        }
    }

    void OnDestroy()
    {
        if (videoPlayer) videoPlayer.loopPointReached -= OnVideoEnded;
        if (videoPlayer) videoPlayer.prepareCompleted -= OnVideoPrepared;
        if (unlockButton) unlockButton.onClick.RemoveListener(ManualUnlock);
    }

    void Update()
    {
        if (debugLog && Time.unscaledTime >= nextDebugTime)
        {
            bool isHolding = Input.GetKey(KeyCode.W) || Input.GetMouseButton(0);
            bool extLocked = IsExternallyLocked();
            float visibleRatio = GetVisibleRatio();
            bool centered = IsCentered();
            string clipName = (videoPlayer != null && videoPlayer.clip != null) ? videoPlayer.clip.name : "null";
            string source = (videoPlayer != null) ? videoPlayer.source.ToString() : "null";
            Debug.Log($"[PlayVideoOnVisibleLock] ready={isReadyToPlay} locked={isLocked} extLocked={extLocked} vis={visibleRatio:F2} centered={centered} snap={(snapCo != null)} finished={hasFinished} hold={isHolding} once={lockOncePerPage} lockedOnce={hasLockedOnce} prepared={(videoPlayer != null && videoPlayer.isPrepared)} playing={(videoPlayer != null && videoPlayer.isPlaying)} source={source} clip={clipName} time={(videoPlayer != null ? videoPlayer.time.ToString("F2") : "n/a")} len={(videoPlayer != null ? videoPlayer.length.ToString("F2") : "n/a")}");
            nextDebugTime = Time.unscaledTime + Mathf.Max(0.1f, debugInterval);
        }

        if (resetLockWhenOutOfView && !isLocked && !hasFinished && GetVisibleRatio() <= resetVisibleRatio)
            hasLockedOnce = false;

        if (!isReadyToPlay && !hasFinished && !(lockOncePerPage && hasLockedOnce))
        {
            if (snapOnApproach && snapCo == null && snapArmed && ShouldSnap())
            {
                BeginSnap();
                pendingLockAfterSnap = true;
                snapArmed = false;
                snapCo = StartCoroutine(SnapToCenter());
            }
            else if (snapOnApproach && !snapArmed && ShouldRearmSnap())
            {
                snapArmed = true;
            }

            if (IsExternallyLocked())
            {
                isReadyToPlay = true;
                lockStartTime = Time.unscaledTime;
            }
            else if (ShouldLock())
            {
                isReadyToPlay = true;
                LockScroll();
                hasLockedOnce = true;
                lockStartTime = Time.unscaledTime;
            }
        }

        if (isReadyToPlay && !hasFinished)
        {
            bool isHolding = Input.GetKey(KeyCode.W) || Input.GetMouseButton(0);

            if (isHolding)
            {
                if (videoPlayer != null)
                {
                    if (videoPlayer.source == VideoSource.VideoClip && videoPlayer.clip == null)
                    {
                        Debug.LogWarning("[PlayVideoOnVisibleLock] VideoPlayer clip is null. Assign a clip or change source.", this);
                        pendingPlay = false;
                    }
                    if (!videoPlayer.isPrepared)
                    {
                        if (!pendingPlay)
                        {
                            videoPlayer.Prepare();
                            pendingPlay = true;
                        }
                    }
                    else if (!videoPlayer.isPlaying)
                    {
                        pendingPlay = false;
                        videoPlayer.Play();
                    }
                }
            }
            else
            {
                pendingPlay = false;
                if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Pause();
            }

            UpdateHoldPulse(isHolding);

            if (ShouldForceFinish())
                SetFinished();
        }
    }
    
    // 동영상 재생이 끝나면 호출될 함수
    void OnVideoEnded(VideoPlayer vp)
    {
        SetFinished();
        
        // ✨ 변경점: 여기서 UnlockScroll()을 호출하지 않습니다!
        // 스크롤은 계속 잠겨있는 상태를 유지합니다.
    }
    
    // 잠금 해제 버튼을 '눌렀을 때만' 호출될 함수
    public void ManualUnlock()
    {
        // ✨ 여기서 스크롤 잠금을 해제합니다.
        UnlockScroll();
        hasLockedOnce = true;

        // 버튼은 역할을 다했으니 다시 숨깁니다.
        if (unlockButton)
        {
            unlockButton.gameObject.SetActive(false);
        }
    }

    // ───────────────── 아래는 기존 유틸리티 함수 (수정 없음) ─────────────────
#region Utility Functions
    bool IsExternallyLocked()
    {
        return contentLock != null && contentLock.IsLocked;
    }

    bool ShouldSnap()
    {
        if (!scrollRect || !scrollRect.content || !viewport || !target) return false;
        if (useVisibleRatio)
            return GetVisibleRatio() >= Mathf.Clamp01(snapThreshold);
        return IsCentered();
    }

    bool ShouldLock()
    {
        if (!viewport || !target) return false;
        bool centered = IsCentered();
        if (centered) return true;
        if (!useVisibleRatio) return false;
        if (GetVisibleRatio() < Mathf.Clamp01(visibleThreshold)) return false;
        return !requireCenterForLock;
    }

    bool ShouldRearmSnap()
    {
        if (!viewport || !target) return false;
        if (useVisibleRatio)
            return GetVisibleRatio() < Mathf.Clamp01(snapThreshold);
        return !IsCentered();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        if (!pendingPlay) return;
        if (!isReadyToPlay || hasFinished) return;
        bool isHolding = Input.GetKey(KeyCode.W) || Input.GetMouseButton(0);
        if (isHolding && !vp.isPlaying)
        {
            pendingPlay = false;
            vp.Play();
        }
    }

    bool ShouldForceFinish()
    {
        if (hasFinished) return false;

        if (unlockTimeout > 0f && lockStartTime > 0f)
        {
            if (Time.unscaledTime - lockStartTime >= unlockTimeout)
                return true;
        }

        if (!videoPlayer || videoPlayer.clip == null) return false;
        double length = videoPlayer.length;
        if (length <= 0.1f) return false;

        return videoPlayer.time >= (length - endTimeEpsilon);
    }

    void SetFinished()
    {
        if (hasFinished) return;
        hasFinished = true;
        isReadyToPlay = false;
        hasLockedOnce = true;

        if (unlockButton)
            unlockButton.gameObject.SetActive(true);

        if (holdTarget)
            holdTarget.localScale = holdBaseScale;
    }

    void UpdateHoldPulse(bool isHolding)
    {
        if (!holdTarget) return;

        if (!isHolding)
        {
            holdTarget.localScale = holdBaseScale;
            return;
        }

        float t = Time.unscaledTime * holdPulseSpeed;
        float k = Mathf.Sin(t) * 0.5f + 0.5f;
        float scale = Mathf.Lerp(1f, holdPulseScale, k);
        holdTarget.localScale = holdBaseScale * scale;
    }

    bool IsCentered()
    {
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!viewport || !effectiveTarget) return false;
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(effectiveTarget);
        Vector2 vpCenter = rv.center;
        if (!rt.Contains(vpCenter)) return false;
        float dy = Mathf.Abs(vpCenter.y - rt.center.y);
        float tolPx = usePixelTolerance
            ? Mathf.Max(0f, centerTolerancePx)
            : rt.height * Mathf.Max(0f, centerTolerance);
        return dy <= tolPx;
    }

    float GetVisibleRatio()
    {
        Rect rv = GetScreenRect(viewport);
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!effectiveTarget) return 0f;
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

    Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        for (int i = 0; i < 4; i++) c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);
        float xMin = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        float xMax = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        float yMin = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        float yMax = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    void LockScroll()
    {
        if (!scrollRect || isLocked) return;
        isLocked = true;
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
        if (debugLog)
            Debug.Log($"[PlayVideoOnVisibleLock] LOCK by {name} target={(target ? target.name : "null")}", this);
    }

    void UnlockScroll()
    {
        if (!scrollRect || !isLocked) return;
        isLocked = false;
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
            scrollRect.inertia = true;
        }
        if (debugLog)
            Debug.Log($"[PlayVideoOnVisibleLock] UNLOCK by {name} target={(target ? target.name : "null")}", this);
    }
#endregion

    IEnumerator SnapToCenter()
    {
        if (!scrollRect || !scrollRect.content || !viewport || !target)
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
        if (pendingLockAfterSnap && !hasFinished && !isReadyToPlay && !(lockOncePerPage && hasLockedOnce))
        {
            pendingLockAfterSnap = false;
            if (IsCentered())
            {
                isReadyToPlay = true;
                if (!IsExternallyLocked())
                    LockScroll();
                hasLockedOnce = true;
                lockStartTime = Time.unscaledTime;
            }
        }
        snapCo = null;
    }

    Vector2 GetContentPositionForCenter(RectTransform content)
    {
        RectTransform effectiveTarget = GetEffectiveTarget();
        if (!effectiveTarget) return content.anchoredPosition;
        Vector3 vpCenterW = viewport.TransformPoint(viewport.rect.center);
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

    void BeginSnap()
    {
        if (!scrollRect) return;

        if (snapStopVelocity)
            scrollRect.velocity = Vector2.zero;

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
