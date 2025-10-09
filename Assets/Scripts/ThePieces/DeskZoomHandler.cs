using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class DeskZoomHandler : MonoBehaviour
{
    private RectTransform rectTransform;

    // OnlyTape에 붙어있는 Fader 스크립트 참조
    public TapeVideoFader tapeVideoFader; 

    public float targetScale = 2.0f; 
    public float zoomDuration = 1.5f;

    // 확대 시 초점을 맞출 이미지 내의 정규화된 오프셋 (예: 중앙에서 살짝 좌측)
    public Vector2 zoomFocusPoint = new Vector2(0.3f, 0.5f); 

    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    private bool isZoomed = false; // 현재 확대 상태 플래그

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("DeskZoomHandler: RectTransform 컴포넌트가 필요합니다.");
        }
    }

    // Button의 OnClick()에서 호출되는 함수
    public void StartDeskZoom()
    {
        // 줌 인/아웃 토글 로직
        float scaleTarget = isZoomed ? 1.0f : targetScale;
        
        // 목표 위치 계산
        Vector2 positionTarget = isZoomed 
            ? originalPosition 
            : CalculateZoomPosition(targetScale, zoomFocusPoint);
        
        // 애니메이션 코루틴 시작
        StartCoroutine(AnimateDeskScale(scaleTarget, positionTarget, zoomDuration));
        
        // 줌 인 시작 시 (isZoomed가 false일 때) 영상 재생 및 페이드인 시작
        if (!isZoomed && scaleTarget > 1.0f) 
        {
            if (tapeVideoFader != null)
            {
                tapeVideoFader.StartFadeAndPlay();
                Debug.Log("OnlyTape 영상 페이드인/재생 시작.");
            }
        }
        
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
            
            // 🌟 SmoothStep 이징 적용
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            rectTransform.localScale = Vector3.Lerp(startScale, new Vector3(targetScaleValue, targetScaleValue, 1f), smoothT);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPositionValue, smoothT);
            
            yield return null;
        }
        
        // 최종 값으로 설정
        rectTransform.localScale = new Vector3(targetScaleValue, targetScaleValue, 1f);
        rectTransform.anchoredPosition = targetPositionValue;
        
        Debug.Log($"책상 스케일 애니메이션 완료! 현재 스케일: {targetScaleValue}");
    }
    
    // 확대 시 초점이 화면 중앙에 오도록 위치를 계산하는 함수
    private Vector2 CalculateZoomPosition(float scale, Vector2 focusPointNormalized)
    {
        // focusPixelPoint는 (Pivot)을 기준으로 상대적인 픽셀 위치를 나타냅니다.
        Vector2 focusPixelPoint = new Vector2(
            rectTransform.rect.width * (focusPointNormalized.x - rectTransform.pivot.x),
            rectTransform.rect.height * (focusPointNormalized.y - rectTransform.pivot.y)
        );
        
        // 확대 전/후 초점 지점의 상대적 이동 거리 계산
        Vector2 offsetBeforeZoom = Vector2.Scale(focusPixelPoint, originalScale);
        Vector2 offsetAfterZoom = focusPixelPoint * scale;

        // 최종 이동해야 할 위치 계산
        Vector2 targetPosition = originalPosition - (offsetAfterZoom - offsetBeforeZoom);

        return targetPosition;
    }
}