using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoOnVisible : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;          // 상위 ScrollRect. 비워두면 자동 탐색
    public RectTransform viewport;         // 보이는 영역(보통 ScrollRect.viewport)
    public RectTransform target;           // 이 오브젝트(비워두면 자동=자기 RectTransform)
    public VideoPlayer videoPlayer;        // 이 영상 재생기 (RenderTexture 출력)

    [Header("Behavior")]
    [Range(0.05f, 1f)] public float visibleThreshold = 0.5f; // '타겟의 몇 %가 보이면' 재생?
    public bool playOnlyOnce = false;      // 한 번만 재생
    public bool pauseWhenHidden = true;    // 화면에서 사라지면 Pause(=true) / Stop(=false)
    public bool stopWhenHidden = false;    // pauseWhenHidden=false일 때만 사용

    bool hasPlayed = false;
    Canvas canvas;
    Camera uiCam;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true);
        if (!viewport)   viewport   = scrollRect ? scrollRect.viewport : transform as RectTransform;
        if (!target)     target     = transform as RectTransform;
        if (!videoPlayer) videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (videoPlayer) videoPlayer.playOnAwake = false;
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrolled);
        // 처음에도 한 번 판정
        Invoke(nameof(CheckAndPlay), 0.05f);
    }

    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    void OnScrolled(Vector2 _)
    {
        CheckAndPlay();
    }

    void LateUpdate()
    {
        // 관성 스크롤 시 프레임별로 값이 변하므로 안전하게 체크
        if (scrollRect) CheckAndPlay();
    }

    void CheckAndPlay()
    {
        if (!videoPlayer || !viewport || !target) return;

        bool visibleEnough = GetVisibleRatio() >= visibleThreshold;

        if (visibleEnough)
        {
            if (playOnlyOnce && hasPlayed) return;
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
                hasPlayed = true;
            }
        }
        else
        {
            if (stopWhenHidden)       videoPlayer.Stop();
            else if (pauseWhenHidden) videoPlayer.Pause();
        }
    }

    float GetVisibleRatio()
    {
        // 타겟/뷰포트의 화면 좌표 사각형 계산
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(target);

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
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
            corners[i] = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);

        float xMin = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float xMax = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float yMin = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float yMax = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
