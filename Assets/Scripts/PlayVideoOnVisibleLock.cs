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
    // ✨ 변경점: 영상 종료 후 활성화할 'NextDownArrow' 오브젝트를 직접 연결합니다.
    public GameObject nextDownArrow; 

    [Header("중앙 감지 설정")]
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f;

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

        // ✨ 시작할 때 NextDownArrow를 비활성화 상태로 만듭니다.
        if (nextDownArrow != null)
        {
            nextDownArrow.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer)
        {
            videoPlayer.loopPointReached -= OnVideoEnded;
        }
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
                if (!videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                }
            }
            else
            {
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Pause();
                }
            }
        }
    }
    
    // 영상 재생이 끝나면 호출될 함수
    void OnVideoEnded(VideoPlayer vp)
    {
        hasFinished = true;
        isReadyToPlay = false;

        // ✨ 변경점: NextDownArrow 오브젝트를 직접 활성화!
        if (nextDownArrow != null)
        {
            nextDownArrow.SetActive(true);
        }

        // 스크롤 잠금은 자동으로 해제해 줍니다.
        UnlockScroll();
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