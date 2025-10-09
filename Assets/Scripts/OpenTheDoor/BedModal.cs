using UnityEngine;

public class BedModal : MonoBehaviour
{
    // Inspector에서 연결할 모달 패널 오브젝트 (BadModal)
    public GameObject modalPanelObject;

    // 💡 모달창이 열릴 때 함께 숨길 오브젝트들의 배열
    public GameObject[] objectsToHide; 

    // 침대 버튼 클릭 시 호출될 함수 (모달 띄우기)
    public void OpenModal()
    {
        if (modalPanelObject != null)
        {
            // BadModal 오브젝트를 활성화하여 화면에 표시
            modalPanelObject.SetActive(true);
            Debug.Log("BedModal: 모달창 열림.");
        }

        // 💡 추가된 로직: 숨겨야 할 오브젝트들을 모두 비활성화
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // 모달창 내 '닫기' 버튼 클릭 시 호출될 함수
    public void CloseModal()
    {
        if (modalPanelObject != null)
        {
            // BadModal 오브젝트를 비활성화하여 화면에서 숨김
            modalPanelObject.SetActive(false);
            Debug.Log("BedModal: 모달창 닫힘.");
        }

        // 💡 추가된 로직: 숨겼던 오브젝트들을 다시 활성화
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}