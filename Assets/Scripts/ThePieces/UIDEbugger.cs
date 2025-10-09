using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI; // GraphicRaycaster를 위해 추가

public class UIDebugger : MonoBehaviour
{
    // 인스펙터에서 Canvas를 연결해주세요.
    public GraphicRaycaster graphicRaycaster;
    
    // 인스펙터에서 EventSystem을 연결해주세요.
    public EventSystem eventSystem;

    void Update()
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        // 마우스 아래에 UI 요소가 있다면
        if (results.Count > 0)
        {
            // 가장 위에 있는 UI 요소의 이름을 출력합니다.
            Debug.Log("마우스 아래 UI: " + results[0].gameObject.name);
        }
    }
}