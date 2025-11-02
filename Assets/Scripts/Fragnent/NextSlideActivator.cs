using UnityEngine;
using UnityEngine.UI;

public class NextSlideActivator : MonoBehaviour
{
    [Header("활성화할 오브젝트")]
    public GameObject nextSlideArrow;

    [Header("대기 시간 (초)")]
    public float delay = 3f;

    [Header("뷰포트 (ScrollRect 뷰포트 또는 Canvas 루트)")]
    public RectTransform viewport;

    private RectTransform rectTransform;
    private bool hasStarted = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (nextSlideArrow != null)
            nextSlideArrow.SetActive(false);
    }

    void Update()
    {
        if (!hasStarted && IsCenteredInViewport())
        {
            hasStarted = true;
            StartCoroutine(ActivateAfterDelay());
        }
    }

    bool IsCenteredInViewport()
    {
        if (viewport == null || rectTransform == null)
            return false;

        // 오브젝트의 월드 중심 좌표
        Vector3 worldCenter = rectTransform.TransformPoint(rectTransform.rect.center);

        // 뷰포트의 월드 중심 좌표
        Vector3 viewportCenter = viewport.TransformPoint(viewport.rect.center);

        // 거리 계산 (UI 공간)
        float distance = Vector3.Distance(worldCenter, viewportCenter);

        // 중앙 근처 판정 (픽셀 단위)
        return distance < 100f;   // 100px 이하일 때 중앙으로 인식
    }

    System.Collections.IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        nextSlideArrow.SetActive(true);
    }
}
