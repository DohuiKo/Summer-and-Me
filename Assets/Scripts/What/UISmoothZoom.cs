using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System; // System.Action을 사용하기 위해 추가

public class UISmoothZoom : MonoBehaviour
{
    [Header("Refs (Assign in Inspector)")]
    public RectTransform viewport;
    public RectTransform content;

    [Header("Zoom Limits")]
    public float minScale = 1f;
    public float maxScale = 4f;

    [Header("Wheel / Animate")]
    public float wheelSensitivity = 0.2f;
    public float defaultPadding = 40f;
    public float defaultDuration = 0.35f;

    private Coroutine running;
    private bool isZoomLocked = false;

    #region Unchanged AutoWire
    void Awake() { AutoWire(); }
    void OnValidate() { AutoWire(); }

    void AutoWire()
    {
        if (!viewport)
        {
            var rt = GetComponent<RectTransform>();
            if (rt) viewport = rt;
            if (!viewport)
            {
                var sr = GetComponentInParent<ScrollRect>();
                if (sr && sr.viewport) viewport = sr.viewport;
            }
        }
        if (!content)
        {
            var sr = GetComponentInParent<ScrollRect>();
            if (sr && sr.content) content = sr.content;
            if (!content && viewport && viewport.childCount > 0)
                content = viewport.GetChild(0) as RectTransform;
        }
    }
    #endregion

    // [수정 1] 마지막에 onComplete 콜백 매개변수 추가
    public void ZoomToRectLocal(Rect rectLocal, float padding = -1f, float duration = -1f, Vector2 offset = default, float zoomScale = 1.0f, Action onComplete = null)
    {
        if (!CheckReady()) return;
        if (padding < 0) padding = defaultPadding;
        if (duration < 0) duration = defaultDuration;

        isZoomLocked = true;

        Vector2 center = rectLocal.center;
        Bounds b = new Bounds(center, rectLocal.size);

        if (running != null) StopCoroutine(running);
        // [수정 2] 코루틴을 시작할 때 onComplete 콜백을 함께 넘겨줌
        running = StartCoroutine(ZoomToBoundsCo(b, padding, duration, offset, zoomScale, onComplete));
    }

    public void ResetZoom()
    {
        if (!CheckReady()) return;
        if (running != null) StopCoroutine(running);

        running = null;
        content.localScale = Vector3.one;
        content.anchoredPosition = Vector2.zero;
        isZoomLocked = false;
    }
    
    // [수정 3] 마지막에 onComplete 콜백 매개변수 추가
    IEnumerator ZoomToBoundsCo(Bounds boundsLocal, float padding, float duration, Vector2 offset, float zoomScale, Action onComplete)
    {
        Vector2 viewSize = viewport.rect.size - new Vector2(padding * 2f, padding * 2f);
        viewSize = new Vector2(Mathf.Max(1f, viewSize.x), Mathf.Max(1f, viewSize.y));

        float perfectFitScaleX = viewSize.x / Mathf.Max(1e-4f, boundsLocal.size.x);
        float perfectFitScaleY = viewSize.y / Mathf.Max(1e-4f, boundsLocal.size.y);
        float perfectFitScale = Mathf.Min(perfectFitScaleX, perfectFitScaleY);

        float targetScale = Mathf.Lerp(minScale, perfectFitScale, zoomScale);
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        Vector3 startScale = content.localScale;
        Vector2 startPos = content.anchoredPosition;

        Vector2 targetCenter = (Vector2)boundsLocal.center;
        Vector2 pivotOffset = new Vector2(
            content.rect.width * (0.5f - content.pivot.x),
            content.rect.height * (0.5f - content.pivot.y)
        );
        Vector2 targetPos = ((-targetCenter + pivotOffset) * targetScale) + offset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(1e-4f, duration);
            float k = Mathf.SmoothStep(0f, 1f, t);
            content.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, k);
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, k);
            yield return null;
        }

        content.localScale = Vector3.one * targetScale;
        content.anchoredPosition = targetPos;
        running = null;

        // [수정 4] 줌 애니메이션이 완전히 끝난 후, 전달된 콜백 함수를 실행
        onComplete?.Invoke();
    }

    void Update()
    {
        if (!viewport || !content || isZoomLocked) return;
        
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.001f)
        {
            float current = content.localScale.x;
            float next = Mathf.Clamp(current * (1f + wheel * wheelSensitivity), minScale, maxScale);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, Input.mousePosition, null, out Vector2 cursorLocal))
            {
                Vector2 before = (cursorLocal - content.anchoredPosition) / current;
                content.localScale = Vector3.one * next;
                Vector2 after = before * next;
                content.anchoredPosition = cursorLocal - after;
            }
            else
            {
                content.localScale = Vector3.one * next;
            }
        }
    }
    
    #region Unchanged CheckReady
    bool CheckReady()
    {
        if (!viewport || !content)
        {
            Debug.LogError("[UISmoothZoom] viewport/content가 비어 있습니다.", this);
            return false;
        }
        return true;
    }
    #endregion
}