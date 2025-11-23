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

    [Header("중앙 감지 설정")]
    [Range(0f, 0.5f)] public float centerTolerance = 0.05f;

    // --- 내부 상태 변수 ---
    private bool isReadyToPlay = false;
    private bool isLocked = false;
    private bool hasFinished = false;

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

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (videoPlayer)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoEnded;
        }
        
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
        if (unlockButton) unlockButton.onClick.RemoveListener(ManualUnlock);
    }

    void Update()
    {
        if (!isReadyToPlay && !hasFinished)
        {
            if (IsCentered())
            {
                isReadyToPlay = true;
                LockScroll();
            }
        }

        if (isReadyToPlay && !hasFinished)
        {
            if (Input.GetKey(KeyCode.W))
            {
                if (!videoPlayer.isPlaying) videoPlayer.Play();
            }
            else
            {
                if (videoPlayer.isPlaying) videoPlayer.Pause();
            }
        }
    }
    
    // 동영상 재생이 끝나면 호출될 함수
    void OnVideoEnded(VideoPlayer vp)
    {
        hasFinished = true;
        isReadyToPlay = false;

        if (unlockButton)
        {
            unlockButton.gameObject.SetActive(true);
        }
        
        // ✨ 변경점: 여기서 UnlockScroll()을 호출하지 않습니다!
        // 스크롤은 계속 잠겨있는 상태를 유지합니다.
    }
    
    // 잠금 해제 버튼을 '눌렀을 때만' 호출될 함수
    public void ManualUnlock()
    {
        // ✨ 여기서 스크롤 잠금을 해제합니다.
        UnlockScroll();

        // 버튼은 역할을 다했으니 다시 숨깁니다.
        if (unlockButton)
        {
            unlockButton.gameObject.SetActive(false);
        }
    }

    // ───────────────── 아래는 기존 유틸리티 함수 (수정 없음) ─────────────────
#region Utility Functions
    bool IsCentered()
    {
        if (!viewport || !target) return false;
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
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            for (int i = 0; i < 4; i++) c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);
        }
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
    }
#endregion
}