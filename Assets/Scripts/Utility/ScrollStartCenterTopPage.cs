using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollStartCenterTopPage : MonoBehaviour
{
    [Header("Refs (비워두면 자동 찾음)")]
    public ScrollRect scroll;
    public RectTransform viewport;
    public RectTransform content;

    [Tooltip("가장 위 페이지(없으면 자동으로 최상단 페이지 검색)")]
    public RectTransform topPage;

    [Tooltip("시작 정렬 시 관성 잠깐 껐다가 다시 켜기")]
    public bool tempDisableInertia = true;

    IEnumerator Start()
    {
        if (!scroll)   scroll   = GetComponent<ScrollRect>();
        if (!viewport) viewport = scroll ? scroll.viewport : GetComponent<RectTransform>();
        if (!content)  content  = scroll ? scroll.content  : null;

        if (!scroll || !viewport || !content) yield break;

        // 레이아웃 계산 완료 대기
        yield return null;  // 1프레임
        yield return null;  // 2프레임 (안전)

        // topPage 미지정 시, Content의 자식들 중 "세로로 가장 위"에 있는 애를 찾음
        if (!topPage)
        {
            float bestY = float.NegativeInfinity;
            RectTransform best = null;
            for (int i = 0; i < content.childCount; i++)
            {
                var rt = content.GetChild(i) as RectTransform;
                if (!rt) continue;
                // 월드 좌표 기준 Y가 가장 큰 것을 "가장 위"로 간주
                float y = rt.TransformPoint(rt.rect.center).y;
                if (y > bestY) { bestY = y; best = rt; }
            }
            topPage = best;
        }

        if (!topPage) yield break;

        bool prevInertia = scroll.inertia;
        if (tempDisableInertia) scroll.inertia = false;

        // viewport 중앙과 topPage 중앙을 일치시키도록 content 위치 보정
        Vector2 vpCenterInContent   = (Vector2)content.InverseTransformPoint(viewport.TransformPoint(viewport.rect.center));
        Vector2 pageCenterInContent = (Vector2)content.InverseTransformPoint(topPage.TransformPoint(topPage.rect.center));
        Vector2 delta = pageCenterInContent - vpCenterInContent;
        content.anchoredPosition -= delta;

        // 보정 후 한 프레임 쉬고 관성 복구
        yield return null;
        if (tempDisableInertia) scroll.inertia = prevInertia;
    }
}
