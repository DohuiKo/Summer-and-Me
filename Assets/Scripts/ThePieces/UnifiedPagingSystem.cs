using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnifiedPagingSystem : MonoBehaviour
{
    [Header("필수 연결 항목")]
    public ScrollRect scrollRect;

    [Header("페이지 설정")]
    public int totalPages = 3;
    public float pageThreshold = 0.1f;
    public float moveDuration = 0.5f;

    private bool isLocked = false;
    private bool isMovingByButton = false;

    void Update()
    {
        if (isMovingByButton) return;

        float currentPos = scrollRect.horizontalNormalizedPosition;

        for (int i = 0; i < totalPages; i++)
        {
            float pagePos = (totalPages > 1) ? (float)i / (totalPages - 1) : 0;
            if (Mathf.Abs(currentPos - pagePos) < pageThreshold)
            {
                if (!isLocked)
                {
                    scrollRect.horizontal = false;
                    isLocked = true;
                }
                return;
            }
        }

        if (isLocked)
        {
            scrollRect.horizontal = true;
            isLocked = false;
        }
    }

    public void MoveToNextPage()
    {
        if (isMovingByButton) return;

        int currentPageIndex = Mathf.RoundToInt(scrollRect.horizontalNormalizedPosition * (totalPages - 1));
        int targetPageIndex = currentPageIndex + 1;

        if (targetPageIndex >= totalPages) return;

        StartCoroutine(SmoothMoveCoroutine(targetPageIndex));
    }

    private IEnumerator SmoothMoveCoroutine(int targetPageIndex)
    {
        isMovingByButton = true;
        isLocked = false;
        scrollRect.horizontal = true;

        float targetPosition = (float)targetPageIndex / (totalPages - 1.0f);
        float elapsedTime = 0f;
        float startPosition = scrollRect.horizontalNormalizedPosition;

        while (elapsedTime < moveDuration)
        {
            float newPosition = Mathf.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            scrollRect.horizontalNormalizedPosition = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = targetPosition;
        
        isMovingByButton = false;
    }
}