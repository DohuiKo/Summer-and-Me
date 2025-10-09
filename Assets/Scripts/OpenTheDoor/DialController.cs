using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

public class DialController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // 💡 Inspector에서 연결: 회전시킬 바늘 Rect Transform (자기 자신)
    private RectTransform needleRect;

    // 💡 Inspector에서 연결: 투명도가 낮아져야 할 배경 침대 Image (BedModalImage)
    public Image backgroundBedImage;

    // 💡 Inspector에서 연결: 검은색으로 보이는 휴대폰 Image 
    public Image phoneBlackScreenImage; 
    
    // 💡 바늘의 총 회전 각도 제한 (0도 ~ 720도)
    [Tooltip("바늘이 회전할 수 있는 최대 누적 각도")]
    public float maxRotationAngle = 720f; // 총 720도로 변경

    [Tooltip("드래그 움직임에 대한 바늘 회전 속도")]
    public float dragSensitivity = 1.0f; 

    // 현재 누적된 회전 각도
    private float currentAngle = 0f;
    private Vector2 startDragPosition;
    
    // 각 단계의 완료 각도 정의
    private const float PHASE_1_END = 360f; // 침대 페이드 아웃 완료 시점
    private const float PHASE_2_END = 720f; // 검은 화면 페이드 인 완료 시점

    void Start()
    {
        needleRect = GetComponent<RectTransform>();
        
        // 초기 투명도 설정: 침대는 불투명, 검은 화면은 투명
        SetAlpha(backgroundBedImage, 1f);
        SetAlpha(phoneBlackScreenImage, 0f); 
    }

    // Alpha 값을 설정하는 헬퍼 함수
    private void SetAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color color = img.color;
            // 알파 값은 0.0f에서 1.0f 사이로 제한
            color.a = Mathf.Clamp(alpha, 0f, 1f); 
            img.color = color;
        }
    }

    // 마우스 클릭 시작 시 호출 
    public void OnPointerDown(PointerEventData eventData)
    {
        // 💡 수정된 부분: RectTransformUtility 호출 인수를 명시적으로 채움
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(), 
            eventData.position, 
            eventData.pressEventCamera, 
            out startDragPosition);
    }

    // 마우스 드래그 중 호출 (바늘 회전 및 투명도 로직)
    public void OnDrag(PointerEventData eventData)
    {
        // 1. 회전 각도 계산 
        Vector2 currentDragPosition;
        
        // 💡 수정된 부분: RectTransformUtility 호출 인수를 명시적으로 채움
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(), 
            eventData.position, 
            eventData.pressEventCamera, 
            out currentDragPosition);
            
        Vector2 center = needleRect.anchoredPosition; 
        float startAngle = Mathf.Atan2(startDragPosition.y - center.y, startDragPosition.x - center.x) * Mathf.Rad2Deg;
        float currentAngleRad = Mathf.Atan2(currentDragPosition.y - center.y, currentDragPosition.x - center.x) * Mathf.Rad2Deg;
        float angleDifference = currentAngleRad - startAngle;

        if (angleDifference > 180) angleDifference -= 360;
        if (angleDifference < -180) angleDifference += 360;
        
        float adjustedAngleChange = angleDifference * dragSensitivity;

        // 2. 누적 각도 업데이트 및 제한 (최대 720도)
        currentAngle = Mathf.Clamp(currentAngle - adjustedAngleChange, 0f, maxRotationAngle);
        
        // 3. 바늘 회전 적용
        needleRect.localRotation = Quaternion.Euler(0, 0, -currentAngle);
        
        // 4. 2단계 투명도 조절 실행
        UpdateAlphaStates();

        if (currentAngle >= maxRotationAngle)
        {
            Debug.Log("다이얼 조작 완료! 검은 휴대폰 화면이 나타났습니다.");
        }

        startDragPosition = currentDragPosition;
    }

    // 핵심 로직: 누적 각도에 따른 2단계 투명도 계산 (변경 없음)
    private void UpdateAlphaStates()
    {
        // 1단계: 침대 페이드아웃 (0도 ~ 360도)
        float bedAlpha;
        if (currentAngle <= PHASE_1_END)
        {
            float progress = currentAngle / PHASE_1_END; 
            bedAlpha = 1f - progress;
        } else {
            bedAlpha = 0f;
        }
        SetAlpha(backgroundBedImage, bedAlpha);

        // 2단계: 검은 화면 페이드인 (360도 ~ 720도)
        float blackScreenAlpha = 0f;
        if (currentAngle >= PHASE_1_END && currentAngle <= PHASE_2_END)
        {
            float phaseDuration = PHASE_2_END - PHASE_1_END;
            float phaseProgress = (currentAngle - PHASE_1_END) / phaseDuration;
            blackScreenAlpha = phaseProgress;
        } else if (currentAngle > PHASE_2_END) {
            blackScreenAlpha = 1f;
        }
        SetAlpha(phoneBlackScreenImage, blackScreenAlpha);
    }
}