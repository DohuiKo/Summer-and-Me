using UnityEngine;
using UnityEngine.UI;

public class StopAudioWhenPageLeft : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;          // 상위 ScrollRect (비워두면 자동 탐색)
    public RectTransform viewport;         // 보이는 영역(ScrollRect.viewport)
    public RectTransform targetPage;       // page_01의 RectTransform
    public AudioSource audioSource;        // 지하철 소리 AudioSource

    [Header("Trigger")]
    public bool triggerAtCenter = true;          // 중앙 기준 판정
    [Range(0f, 0.5f)] public float centerTolerance = 0.10f;  // 중앙 허용오차(페이지 높이 비율)
    [Range(0f, 1f)] public float minVisibleRatio = 0.4f;     // triggerAtCenter=false일 때 사용

    [Header("Behavior")]
    public bool stopOnLeave = true;               // 페이지에서 벗어나면 Stop
    public bool playWhenEnterAgain = false;       // 다시 들어오면 자동 Play

    [Header("Start Options")]
    public bool requireEnterOnceBeforeStop = true; // ★ ‘한 번 들어온 뒤’부터 Stop 허용
    public bool autoPlayIfStartInside = true;      // 시작 시 이미 안에 있으면 자동 재생
    public float initialCheckDelay = 0.02f;        // 첫 판정 지연(레이아웃 안정화용)

    Canvas canvas; Camera uiCam;
    bool hasEverBeenInside = false;

    void Awake()
    {
        if (!scrollRect)  scrollRect  = GetComponentInParent<ScrollRect>(true);
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!targetPage)  targetPage  = transform as RectTransform;   // 이 스크립트를 page_01에 붙인 경우
        if (!audioSource) audioSource = GetComponent<AudioSource>();  // 같은 GO에 있으면 자동

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrolled);
        // 첫 프레임 이후에 판정(레이아웃/안커 계산 안정화)
        Invoke(nameof(DeferredFirstCheck), initialCheckDelay);
    }
    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    void DeferredFirstCheck()
    {
        bool inside = IsInside();
        if (inside)
        {
            hasEverBeenInside = true;
            if (autoPlayIfStartInside && audioSource && !audioSource.isPlaying)
                audioSource.Play();
        }
        Evaluate(); // 현재 상태 반영
    }

    void LateUpdate() { Evaluate(); }
    void OnScrolled(Vector2 _) { Evaluate(); }

    void Evaluate()
    {
        if (!viewport || !targetPage || !audioSource) return;

        bool inside = IsInside();

        if (inside)
        {
            hasEverBeenInside = true;
            if (playWhenEnterAgain && !audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            // ★ 처음 들어온 적이 있어야만 멈춘다
            if (stopOnLeave && (!requireEnterOnceBeforeStop || hasEverBeenInside))
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }
    }

    bool IsInside()
    {
        if (triggerAtCenter) return IsCentered();
        return GetVisibleRatio() >= minVisibleRatio;
    }

    // ───────── 중앙/가시율 판정 ─────────
    bool IsCentered()
    {
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(targetPage);
        Vector2 vpCenter = rv.center;

        if (!rt.Contains(vpCenter)) return false;

        float dy = Mathf.Abs(vpCenter.y - rt.center.y);
        float tolPx = rt.height * Mathf.Max(0f, centerTolerance);
        return dy <= tolPx;
    }

    float GetVisibleRatio()
    {
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(targetPage);
        if (rt.width <= 0f || rt.height <= 0f) return 0f;

        float xMin = Mathf.Max(rv.xMin, rt.xMin);
        float yMin = Mathf.Max(rv.yMin, rt.yMin);
        float xMax = Mathf.Min(rv.xMax, rt.xMax);
        float yMax = Mathf.Min(rv.yMax, rt.yMax);
        float iw = Mathf.Max(0, xMax - xMin);
        float ih = Mathf.Max(0, yMax - yMin);
        return (iw * ih) / (rt.width * rt.height);
    }

    Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        for (int i = 0; i < 4; i++)
            c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);

        float xMin = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        float xMax = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        float yMin = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        float yMax = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
