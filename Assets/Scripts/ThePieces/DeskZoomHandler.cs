using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class DeskZoomHandler : MonoBehaviour
{
    private RectTransform rectTransform;

    // Inspector에서 설정할 확대 목표 스케일 (예: 2배 확대)
    public float targetScale = 2.0f; 
    
    // 줌 인/아웃에 걸리는 시간
    public float zoomDuration = 1.5f;

    // 🌟 확대 시 초점을 맞출 이미지 내의 정규화된 오프셋 (0.0 ~ 1.0)
    // (0.5, 0.5)가 정중앙이며, (0.3, 0.5)는 중앙에서 살짝 좌측입니다.
    public Vector2 zoomFocusPoint = new Vector2(0.3f, 0.5f); 

    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    private bool isZoomed = false; // 현재 확대 상태 플래그

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 초기 위치와 크기 저장
            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("DeskZoomHandler는 RectTransform 컴포넌트가 필요합니다. UI 오브젝트에 부착되었는지 확인하세요.");
        }
    }

    // Button의 OnClick() 영역에서 호출해야 할 함수
    public void StartDeskZoom()
    {
        // 줌 인/아웃 토글 로직
        float scaleTarget = isZoomed ? 1.0f : targetScale;
        
        // 목표 위치 계산: 줌 아웃 시에는 원래 위치로, 줌 인 시에는 초점 계산 위치로 설정
        Vector2 positionTarget = isZoomed 
            ? originalPosition 
            : CalculateZoomPosition(targetScale, zoomFocusPoint);
        
        StartCoroutine(AnimateDeskScale(scaleTarget, positionTarget, zoomDuration));
        isZoomed = !isZoomed; // 상태 전환
    }

    // 오브젝트의 스케일과 위치를 애니메이션하는 코루틴
    IEnumerator AnimateDeskScale(float targetScaleValue, Vector2 targetPositionValue, float duration)
    {
        Vector3 startScale = rectTransform.localScale;
        Vector2 startPosition = rectTransform.anchoredPosition;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            // 🌟 SmoothStep 이징 함수 적용: 부드러운 시작과 종료를 만듭니다.
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 1. 스케일 보간 (확대/축소)
            rectTransform.localScale = Vector3.Lerp(startScale, new Vector3(targetScaleValue, targetScaleValue, 1f), smoothT);
            
            // 2. 위치 보간 (확대 시 초점을 맞추기 위함)
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPositionValue, smoothT);
            
            yield return null;
        }
        
        // 최종 값으로 설정하여 정확히 목표치에 도달하도록 합니다.
        rectTransform.localScale = new Vector3(targetScaleValue, targetScaleValue, 1f);
        rectTransform.anchoredPosition = targetPositionValue;
        
        Debug.Log($"책상 스케일 애니메이션 완료! 현재 스케일: {targetScaleValue}");
    }
    
    // 확대 시 초점이 화면 중앙에 오도록 위치를 계산하는 함수
    private Vector2 CalculateZoomPosition(float scale, Vector2 focusPointNormalized)
    {
        // 1. 이미지 내에서 초점을 맞출 픽셀 지점을 RectTransform 기준으로 계산합니다.
        // 이 지점은 (Pivot)을 기준으로 상대적인 위치를 나타냅니다.
        // focusPointNormalized는 0~1 값 (예: 0.3, 0.5)
        Vector2 focusPixelPoint = new Vector2(
            rectTransform.rect.width * (focusPointNormalized.x - rectTransform.pivot.x),
            rectTransform.rect.height * (focusPointNormalized.y - rectTransform.pivot.y)
        );
        
        // 2. 확대 전/후 초점 지점의 상대적 이동 거리 계산
        // 확대 전의 오프셋 (원래 스케일 기준)
        Vector2 offsetBeforeZoom = Vector2.Scale(focusPixelPoint, originalScale);
        
        // 확대 후의 오프셋 (목표 스케일 기준)
        Vector2 offsetAfterZoom = focusPixelPoint * scale;

        // 3. 최종 이동해야 할 위치 계산
        // 원래 위치(originalPosition)에서 (offsetAfterZoom - offsetBeforeZoom) 만큼 반대 방향으로 이동해야 
        // 초점이 고정된 것처럼 보입니다.
        Vector2 targetPosition = originalPosition - (offsetAfterZoom - offsetBeforeZoom);

        return targetPosition;
    }
}