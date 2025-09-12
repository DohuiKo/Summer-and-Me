using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoOnVisibleLock : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;          // 상위 ScrollRect (비워두면 자동 탐색)
    public RectTransform viewport;         // ScrollRect.viewport
    public RectTransform target;           // 보이는 판정 대상(보통 이 RawImage)
    public VideoPlayer videoPlayer;        // 재생할 비디오

    [Header("Behavior")]
    [Range(0.05f, 1f)] public float visibleThreshold = 0.5f; // (중앙 트리거 미사용 시)
    public bool playOnlyOnce = false;      // 한 번만 재생
    public bool unlockAtEnd   = true;      // 영상이 끝나면 자동 언락

    [Header("Trigger (Center-based)")]
    public bool triggerAtCenter   = true;          // 중앙에서만 동작
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f; // 타겟 높이의 % 허용오차
    public bool lockWhenCentered  = true;          // 중앙에 오면 즉시 락
    public bool autoPlayOnCenter  = true;          // 중앙에 왔을 때 자동 재생

    [Header("Unlock Safety")]
    public bool unlockWhenPausedOrStopped = true;  // Pause/Stop에도 언락

    [Header("Relock policy")]
    public bool lockOnlyBeforeFirstPlay = true;    // 첫 재생 전까지만 락
    public bool unlockWhenLeaveCenter   = false;   // 중앙에서 벗어나면 자동 언락

    // 내부 상태
    bool hasPlayed = false;
    bool isLocked  = false;
    bool wasPlaying = false;   // 직전 프레임에 재생 중이었는지
    bool centerArmed = true;   // 언락 직후 재잠금 방지

    // ScrollRect 상태 저장/복원
    bool prevEnabled, prevVertical, prevHorizontal, prevInertia;
    bool saved = false;

    Canvas canvas; Camera uiCam;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true);
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!target)   target = transform as RectTransform;
        if (!videoPlayer) videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (videoPlayer)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.started += OnVideoStarted;
            if (unlockAtEnd) videoPlayer.loopPointReached += OnVideoEnded;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer)
        {
            videoPlayer.started -= OnVideoStarted;
            if (unlockAtEnd) videoPlayer.loopPointReached -= OnVideoEnded;
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

        // 재생이 멈추면(Pause/Stop 포함) 언락
        if (unlockWhenPausedOrStopped && isLocked && videoPlayer)
        {
            if (wasPlaying && !videoPlayer.isPlaying)
            {
                UnlockScroll();
                wasPlaying = false;
            }
        }
    }

    // ───────────────────── Core ─────────────────────
    void Evaluate()
    {
        if (!viewport || !target) return;

        bool centered = triggerAtCenter ? IsCentered()
                                        : GetVisibleRatio() >= visibleThreshold;

        if (centered)
        {
            // 언락 직후 재락 방지 + (선택) 첫 재생 전까지만 락
            bool canLockNow = lockWhenCentered && centerArmed && !isLocked
                              && (!lockOnlyBeforeFirstPlay || !hasPlayed);
            if (canLockNow) LockScroll();

            // 중앙에서 자동 재생(옵션)
            if (videoPlayer && autoPlayOnCenter)
            {
                if (!(playOnlyOnce && hasPlayed) && !videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                    hasPlayed = true;
                }
            }
        }
        else
        {
            // 중앙에서 벗어나면 다시 '잠글 준비'
            centerArmed = true;

            // 선택: 중앙에서 벗어나는 순간 자동 언락
            if (unlockWhenLeaveCenter && isLocked) UnlockScroll();
        }
    }

    void OnVideoStarted(VideoPlayer _) => wasPlaying = true;

    void OnVideoEnded(VideoPlayer _)
    {
        wasPlaying = false;
        if (unlockAtEnd && isLocked) UnlockScroll();
    }

    // ───────────────── Visibility / Center ──────────
    float GetVisibleRatio()
    {
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(target);
        if (rt.width <= 0f || rt.height <= 0f) return 0f;

        float xMin = Mathf.Max(rv.xMin, rt.xMin);
        float yMin = Mathf.Max(rv.yMin, rt.yMin);
        float xMax = Mathf.Min(rv.xMax, rt.xMax);
        float yMax = Mathf.Min(rv.yMax, rt.yMax);
        float iw = Mathf.Max(0, xMax - xMin);
        float ih = Mathf.Max(0, yMax - yMin);
        return (iw * ih) / (rt.width * rt.height);
    }

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

        // 상태 저장
        prevEnabled   = scrollRect.enabled;
        prevVertical  = scrollRect.vertical;
        prevHorizontal= scrollRect.horizontal;
        prevInertia   = scrollRect.inertia;
        saved = true;

        // 하드락
        scrollRect.enabled   = false;
        scrollRect.vertical  = false;
        scrollRect.horizontal= false;
        scrollRect.inertia   = false;
        scrollRect.velocity  = Vector2.zero;

        isLocked = true;
        centerArmed = false; // 언락 전까지는 다시 잠그지 않음
    }

    void UnlockScroll()
    {
        if (!scrollRect || !isLocked) return;

        if (saved)
        {
            scrollRect.enabled   = prevEnabled;
            scrollRect.vertical  = prevVertical;
            scrollRect.horizontal= prevHorizontal;
            scrollRect.inertia   = prevInertia;
        }
        else
        {
            scrollRect.enabled = true; scrollRect.vertical = true; scrollRect.inertia = true;
        }
        isLocked = false;
        // 다시 잠그려면: 중앙에서 벗어났다(→ centerArmed=true) 다시 중앙에 들어와야 함
    }

    // 외부 스킵 버튼 등에서 호출
    public void ForceUnlockAndStop()
    {
        if (videoPlayer) videoPlayer.Stop();
        UnlockScroll();
        wasPlaying = false;
    }
}
