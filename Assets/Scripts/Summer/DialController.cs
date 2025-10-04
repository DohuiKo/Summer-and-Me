using UnityEngine;
using UnityEngine.EventSystems;

// IPointerDownHandler: 클릭 시작 감지
// IDragHandler: 드래그 중인 상태 감지
public class DialController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // 💡 Inspector에서 연결: 회전시킬 바늘 Rect Transform (자기 자신)
    private RectTransform needleRect;

    // 💡 바늘의 총 회전 각도 제한 (0도 ~ 360도)
    [Tooltip("바늘이 회전할 수 있는 최대 각도 (예: 360)")]
    public float maxRotationAngle = 360f; 

    // 현재 누적된 회전 각도 (0도에서 maxRotationAngle까지 증가)
    private float currentAngle = 0f;
    private Vector2 startDragPosition;

    void Start()
    {
        // 스크립트가 붙은 오브젝트의 Rect Transform 컴포넌트를 가져옵니다.
        needleRect = GetComponent<RectTransform>();
    }

    // 마우스 클릭 시작 시 호출 (드래그 시작점을 기록)
    public void OnPointerDown(PointerEventData eventData)
    {
        // 드래그 시작 위치 기록 (스크린 좌표를 UI 내부의 로컬 좌표로 변환)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(), 
            eventData.position, 
            eventData.pressEventCamera, 
            out startDragPosition);
    }

    // 마우스 드래그 중 호출 (바늘 회전 로직)
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentDragPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(), 
            eventData.position, 
            eventData.pressEventCamera, 
            out currentDragPosition);

        // 1. 회전 중심점 설정 (바늘의 피벗 위치를 중심으로 계산)
        Vector2 center = needleRect.anchoredPosition; 
        
        // 2. 각도 계산 (시작점 -> 중심, 현재점 -> 중심)
        float startAngle = Mathf.Atan2(startDragPosition.y - center.y, startDragPosition.x - center.x) * Mathf.Rad2Deg;
        float currentAngleRad = Mathf.Atan2(currentDragPosition.y - center.y, currentDragPosition.x - center.x) * Mathf.Rad2Deg;
        
        // 회전 변화량 계산
        float angleDifference = currentAngleRad - startAngle;

        // 각도 차이를 -180도 ~ 180도 범위로 정규화
        if (angleDifference > 180) angleDifference -= 360;
        if (angleDifference < -180) angleDifference += 360;
        
        // 3. 누적 각도 업데이트 및 제한
        // 시계 방향 회전을 위해 angleDifference를 뺌
        currentAngle = Mathf.Clamp(currentAngle - angleDifference, 0f, maxRotationAngle);
        
        // 4. 바늘 회전 적용 (Z축 회전)
        // currentAngle이 0에서 maxRotationAngle로 증가할 때, Z 회전은 -currentAngle을 사용해야 시계 방향으로 돌아갑니다.
        needleRect.localRotation = Quaternion.Euler(0, 0, -currentAngle);
        
        Debug.Log($"현재 회전 각도: {currentAngle:F1}도");

        // 드래그 시작 위치를 현재 위치로 업데이트하여 다음 드래그에 대비
        startDragPosition = currentDragPosition;
    }
}