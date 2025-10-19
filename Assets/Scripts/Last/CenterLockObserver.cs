// CenterLockObserver.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CenterLockObserver : MonoBehaviour
{
    [Header("Refs (CLM과 동일하게 연결)")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform target;

    [Header("Center Judge")]
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f;

    [Tooltip("중앙을 처음 감지했을 때 1회 호출")]
    public UnityEvent onCenteredOnce;

    bool fired;
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
        fired = false;
    }

    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    void OnScrolled(Vector2 _) { TryFire(); }
    void Update() { TryFire(); } // 드래그 멈춘 프레임에도 판정되도록

    void TryFire()
    {
        if (fired || !viewport || !target) return;
        if (IsCentered())
        {
            fired = true;
            onCenteredOnce?.Invoke();
        }
    }

    // ── CLM과 동일 기준(뷰포트 중앙Y가 타겟 내부에 있고, 중앙Y-타겟중심Y <= tolPx)
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
        for (int i = 0; i < 4; i++)
            c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);
        float xMin = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        float xMax = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        float yMin = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        float yMax = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
