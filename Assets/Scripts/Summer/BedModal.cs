using UnityEngine;

public class BedModal : MonoBehaviour
{
    // 💡 Inspector에서 연결할 모달 패널 오브젝트 (BadModal)
    public GameObject modalPanelObject;

    // 💡 침대 버튼 클릭 시 호출될 함수 (모달 띄우기)
    public void OpenModal()
    {
        if (modalPanelObject != null)
        {
            // BadModal 오브젝트를 활성화하여 화면에 표시
            modalPanelObject.SetActive(true);
            Debug.Log("BedModal: 모달창 열림.");
        }
    }

    // 💡 모달창 내 '닫기' 버튼 클릭 시 호출될 함수
    public void CloseModal()
    {
        if (modalPanelObject != null)
        {
            // BadModal 오브젝트를 비활성화하여 화면에서 숨김
            modalPanelObject.SetActive(false);
            Debug.Log("BedModal: 모달창 닫힘.");
        }
    }
}