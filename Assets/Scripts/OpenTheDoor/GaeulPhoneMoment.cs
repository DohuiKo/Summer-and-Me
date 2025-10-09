using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GaeulPhoneMoment : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform character;          // 가을이 이미지(비우면 자기 RectTransform)
    public CanvasGroup fadeOverlay;          // 검은 오버레이(비우면 자동 생성)
    public ScrollRect scrollRect;            // Canvas/Scroll View 의 ScrollRect
    public RectTransform viewport;           // Scroll View/Viewport
    public RectTransform targetPage;         // 가을이 있는 페이지(예: PhoneFall)
    public RectTransform nextPage;           // (선택) 다음 페이지

    [Header("Trigger when centered")]
    public float centerTolerancePx = 40f;    // 뷰포트 중앙과 페이지 중앙의 허용 오차
    public bool autoScrollToNextOnUnlock = false;
    public float autoScrollTime = 0.6f;

    [Header("Shake (UI-safe)")]
    public float shakeDuration = 1.2f;
    public float shakePosAmplitude = 0.015f;   // 약 1~2px용 (아래에서 *100f 적용)
    public float shakeRotAmplitude = 1.2f;     // deg
    public AnimationCurve shakeDampen = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Dim Flicker")]
    public float dimPeak = 0.25f;
    public float dimIn = 0.25f;
    public float dimOut = 0.35f;

    [Header("Unlock FX (no scene change)")]
    public float pauseAfterShake = 0.4f;
    public float fadeOutTime = 0.5f;
    public float holdDarkTime = 0.15f;
    public float fadeBackTime = 0.5f;

    // ---- internal ----
    private bool arrivedPlayed = false;   // 중앙 도달 연출 1회만
    private bool unlocked = false;        // 클릭(언락) 여부
    private RectTransform rt;
    private Vector2 baseAnchoredPos;
    private Vector3 baseLocalEuler;

    void Awake()
    {
        rt = character ? character : GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>(); // 안전장치

        baseAnchoredPos = rt.anchoredPosition;
        baseLocalEuler = rt.localEulerAngles;

        // 오버레이 자동 생성
        if (!fadeOverlay)
        {
            var go = new GameObject("FadeOverlay", typeof(Canvas), typeof(CanvasGroup));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            fadeOverlay = go.GetComponent<CanvasGroup>();
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;

            var imgGO = new GameObject("BG", typeof(Image));
            imgGO.transform.SetParent(go.transform, false);
            var img = imgGO.GetComponent<Image>();
            img.color = Color.black;
            var frt = img.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
        }
    }

    void OnDisable()
    {
        if (rt)
        {
            rt.anchoredPosition = baseAnchoredPos;
            rt.localEulerAngles = baseLocalEuler;
        }
    }

    void Update()
    {
        // 페이지가 중앙에 왔는지 감지
        if (!arrivedPlayed && viewport && targetPage && IsPageCentered())
        {
            arrivedPlayed = true;
            StartCoroutine(OnArriveCentered());
        }
    }

    // Button OnClick에 연결해 사용
    public void OnTap()
    {
        if (!arrivedPlayed || unlocked) return;
        unlocked = true;
        StartCoroutine(UnlockSequence());
    }

    IEnumerator OnArriveCentered()
    {
        if (scrollRect) scrollRect.enabled = false; // 잠금
        StartCoroutine(DimFlicker());
        yield return StartCoroutine(ShakeOnce());   // 제자리 미세 떨림
        yield return new WaitForSeconds(pauseAfterShake);
        // 여기서 플레이어 클릭을 기다림
    }

    IEnumerator UnlockSequence()
    {
        // 살짝 어둡게 → 유지 → 다시 밝게
        fadeOverlay.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeOutTime) { fadeOverlay.alpha = Mathf.Lerp(0f, 0.6f, t / fadeOutTime); t += Time.deltaTime; yield return null; }
        fadeOverlay.alpha = 0.6f;

        yield return new WaitForSeconds(holdDarkTime);

        t = 0f;
        while (t < fadeBackTime) { fadeOverlay.alpha = Mathf.Lerp(0.6f, 0f, t / fadeBackTime); t += Time.deltaTime; yield return null; }
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;

        // 스크롤 락 해제
        if (scrollRect) scrollRect.enabled = true;

        // 옵션: 자동 스크롤로 다음 페이지로 넘기기
        if (autoScrollToNextOnUnlock && nextPage && viewport)
            yield return StartCoroutine(SmoothScrollTo(nextPage, autoScrollTime));
    }

    IEnumerator ShakeOnce()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            float k = shakeDampen.Evaluate(t / shakeDuration);

            float offX = (Random.value - 0.5f) * 2f * shakePosAmplitude * k * 100f; // px 느낌
            float offY = (Random.value - 0.5f) * 2f * shakePosAmplitude * k * 100f;

            rt.anchoredPosition = baseAnchoredPos + new Vector2(offX, offY);
            rt.localEulerAngles = new Vector3(0, 0, ((Random.value - 0.5f) * 2f) * shakeRotAmplitude * k);

            t += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = baseAnchoredPos;
        rt.localEulerAngles = baseLocalEuler;
    }

    IEnumerator DimFlicker()
    {
        float t = 0f;
        while (t < dimIn) { fadeOverlay.alpha = Mathf.Lerp(0f, dimPeak, t / dimIn); t += Time.deltaTime; yield return null; }
        fadeOverlay.alpha = dimPeak;
        t = 0f;
        while (t < dimOut) { fadeOverlay.alpha = Mathf.Lerp(dimPeak, 0f, t / dimOut); t += Time.deltaTime; yield return null; }
        fadeOverlay.alpha = 0f;
    }

    bool IsPageCentered()
    {
        // 세로 스크롤 기준: 뷰포트 중앙과 targetPage 중앙의 Y거리(px)
        Vector3 vpCenter = viewport.TransformPoint(viewport.rect.center);
        Vector3 pageCenter = targetPage.TransformPoint(targetPage.rect.center);
        float dist = Mathf.Abs(vpCenter.y - pageCenter.y);
        return dist <= centerTolerancePx;
    }

    IEnumerator SmoothScrollTo(RectTransform target, float duration)
    {
        var content = scrollRect.content;
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // target 중앙의 컨텐트 로컬 좌표
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, target.TransformPoint(target.rect.center), null, out localPoint);
        float targetCenterY = -localPoint.y; // 상단 기준 양수

        float desiredTop = targetCenterY - viewportHeight * 0.5f;
        desiredTop = Mathf.Clamp(desiredTop, 0f, Mathf.Max(0f, contentHeight - viewportHeight));

        float targetNormY = (contentHeight <= viewportHeight) ? 1f : 1f - (desiredTop / (contentHeight - viewportHeight));

        Vector2 start = scrollRect.normalizedPosition;
        Vector2 end = new Vector2(start.x, targetNormY);

        float t = 0f;
        while (t < duration)
        {
            scrollRect.normalizedPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
            t += Time.deltaTime;
            yield return null;
        }
        scrollRect.normalizedPosition = end;
    }
}
