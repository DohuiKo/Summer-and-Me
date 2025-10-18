using UnityEngine;

public class ZoomModalController : MonoBehaviour
{
    public UISmoothZoom zoomController;
    public GameObject modalWindow;
    
    public RectTransform targetObject; // 줌할 대상을 인스펙터에서 지정할 변수

    // 모달창을 띄우는 실제 함수
    private void ShowMyModal()
    {
        modalWindow.SetActive(true);
    }

    public void ZoomAndShowModal()
    {
        // 줌할 대상이 설정되지 않았으면 오류를 방지하기 위해 실행하지 않음
        if (targetObject == null)
        {
            Debug.LogError("줌할 대상(Target Object)이 지정되지 않았습니다!");
            return;
        }

        zoomController.ZoomToRectLocal(targetObject.rect, onComplete: ShowMyModal);
    }
}