using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeUpToNext : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;
    public RectTransform viewport;              // 없으면 자동 탐색
    public RectTransform content;               // 없으면 자동 탐색(ScrollRect.content → Viewport/Content → Viewport)

    [Header("Pages")]
    public List<RectTransform> pages = new List<RectTransform>(); // 비워두면 content의 직속 자식 자동 수집
    public int startIndex = 0;
    public float snapDuration = 0.25f;

    int current = -1;
    Coroutine snapCo;

    void Awake()
    {
        ResolveRefs();
        if (pages.Count == 0) CollectPages();
    }

    void Start()
    {
        // 레이아웃이 늦게 잡히는 케이스 보완
        if (pages.Count == 0) { ResolveRefs(); CollectPages(); }

        if (pages.Count == 0)
        {
            Debug.LogError("[SwipeUpToNext] pages is EMPTY. Assign ScrollRect.content or place pages under content/viewport.");
            return;
        }

        current = Mathf.Clamp(startIndex, 0, pages.Count - 1);
        GoTo(current, instant: true);
    }

    void ResolveRefs()
    {
        // 1) ScrollRect
        if (!scrollRect)
        {
            scrollRect = GetComponentInParent<ScrollRect>(true);
            if (!scrollRect)
            {
#if UNITY_2023_1_OR_NEWER
                // 새 API (비활성 오브젝트 포함)
                scrollRect = UnityEngine.Object.FindFirstObjectByType<ScrollRect>(FindObjectsInactive.Include);
#else
                // 구버전 호환
                scrollRect = UnityEngine.Object.FindObjectOfType<ScrollRect>(true);
#endif
            }
        }

        // 2) Viewport
        if (!viewport)
        {
            if (scrollRect && scrollRect.viewport) viewport = scrollRect.viewport;
            else if (scrollRect)
            {
                var vp = scrollRect.transform.Find("Viewport");
                if (vp) viewport = vp as RectTransform;
            }
        }

        // 3) Content
        if (!content)
        {
            if (scrollRect && scrollRect.content) content = scrollRect.content;
            else if (viewport)
            {
                // Viewport/Content 우선, 없으면 Viewport의 유일 자식, 그것도 없으면 Viewport 자체
                RectTransform pick = null;
                for (int i = 0; i < viewport.childCount; i++)
                {
                    var ch = viewport.GetChild(i) as RectTransform;
                    if (string.Equals(ch.name, "Content", StringComparison.OrdinalIgnoreCase))
                    {
                        pick = ch; break;
                    }
                }
                if (!pick && viewport.childCount == 1)
                    pick = viewport.GetChild(0) as RectTransform;

                content = pick ? pick : viewport;
            }
        }

        // ScrollRect.content가 비어 있으면 채워줌(스크롤 동작 보장)
        if (scrollRect && content && scrollRect.content != content)
            scrollRect.content = content;

        // 디버깅 원하면 주석 해제
        // Debug.Log($"[SwipeUpToNext] refs → SR:{(scrollRect?scrollRect.name:"null")}  VP:{(viewport?viewport.name:"null")}  CT:{(content?content.name:"null")}");
    }

    void CollectPages()
    {
        pages.Clear();
        if (!content)
        {
            Debug.LogError("[SwipeUpToNext] content is NULL. Set it or assign ScrollRect.content.");
            return;
        }

        // content의 '직접 자식'만 페이지로 간주
        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i) as RectTransform;
            if (child && child != content) pages.Add(child);
        }
        // Debug.Log($"[SwipeUpToNext] Found {pages.Count} pages under '{content.name}'.");
    }

    public void Next()  => GoTo(current + 1);
    public void Prev()  => GoTo(current - 1);

    public void GoTo(int index, bool instant = false)
    {
        if (pages == null || pages.Count == 0)
        {
            Debug.LogError("[SwipeUpToNext] GoTo called but pages is empty.");
            return;
        }

        if (index < 0 || index >= pages.Count)
            index = Mathf.Clamp(index, 0, pages.Count - 1); // 안전 가드

        current = index;

        // 세로 스크롤 기준: 각 page의 anchoredPosition.y 사용
        Vector2 targetPos = content.anchoredPosition;
        targetPos.y = -pages[current].anchoredPosition.y;

        if (snapCo != null) StopCoroutine(snapCo);
        if (instant) content.anchoredPosition = targetPos;
        else snapCo = StartCoroutine(SmoothMove(targetPos, snapDuration));
    }

    System.Collections.IEnumerator SmoothMove(Vector2 target, float duration)
    {
        Vector2 start = content.anchoredPosition;
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            content.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        content.anchoredPosition = target;
        snapCo = null;
    }
}
