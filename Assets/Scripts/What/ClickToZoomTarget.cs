using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToZoomTarget : MonoBehaviour, IPointerClickHandler
{
    [Header("Zoom Settings")]
    public UISmoothZoom zoom;           // UISmoothZoom 연결 (Viewport에 붙어있는 것)
    public RectTransform target;        // 비우면 자기 자신 RectTransform 사용
    
    [Tooltip("줌의 최종 위치를 보정합니다.")]
    public Vector2 zoomOffset;

    [Tooltip("줌 확대 배율을 조절합니다. 1 = 화면에 꽉 채움, 0.5 = 50%만 확대.")]
    [Range(0f, 2f)]
    public float zoomScale = 1.0f;
    
    public float padding = 40f;
    public float duration = 0.5f;

    // ----- ▼▼▼ 여기에 모달 관련 코드 추가 ▼▼▼ -----
    [Header("Modal Settings")]
    [Tooltip("줌 애니메이션이 끝난 후 활성화할 모달창을 연결하세요.")]
    public GameObject modalWindow;
    // ----- ▲▲▲ 코드 추가 완료 ▲▲▲ -----


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!zoom) { Debug.LogWarning("[ClickToZoomTarget] Zoom 참조 없음"); return; }
        
        var targetRect = target ? target : transform as RectTransform;
        if (!targetRect) { Debug.LogWarning("[ClickToZoomTarget] RectTransform 대상 없음"); return; }

        Rect localRect = GetLocalRectInContent(targetRect, zoom.content);
        
        // ----- ▼▼▼ onComplete 콜백을 사용하여 ShowModal 함수를 전달 ▼▼▼ -----
        // 줌 애니메이션이 끝나면 ShowModal 함수를 실행하라고 명령합니다.
        zoom.ZoomToRectLocal(localRect, padding, duration, zoomOffset, zoomScale, onComplete: ShowModal);
        // ----- ▲▲▲ 코드 수정 완료 ▲▲▲ -----
    }
    
    // ----- ▼▼▼ 모달을 활성화하는 함수 추가 ▼▼▼ -----
    private void ShowModal()
    {
        // modalWindow 변수에 게임 오브젝트가 할당되었는지 확인합니다.
        if (modalWindow != null)
        {
            // 할당되었다면 모달창을 활성화합니다.
            modalWindow.SetActive(true);
        }
    }
    // ----- ▲▲▲ 코드 추가 완료 ▲▲▲ -----

    private Rect GetLocalRectInContent(RectTransform target, RectTransform content)
    {
        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        Vector2 localPos_bl = content.InverseTransformPoint(worldCorners[0]);
        Vector2 localPos_tr = content.InverseTransformPoint(worldCorners[2]);

        return new Rect(localPos_bl.x, localPos_bl.y, localPos_tr.x - localPos_bl.x, localPos_tr.y - localPos_bl.y);
    }
}