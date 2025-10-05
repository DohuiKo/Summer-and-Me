using UnityEngine;

public class PhotoModal : MonoBehaviour
{
    // Inspector에서 연결할 모달 패널 오브젝트
    public GameObject modalPanelObject;

    // 모달창이 열릴 때 함께 숨길 오브젝트들의 배열
    public GameObject[] objectsToHide;

    void Start()
    {
        // 게임 시작 시 모달은 항상 닫혀 있도록 설정
        if (modalPanelObject != null)
        {
            modalPanelObject.SetActive(false);
        }
    }

    // '사진'을 클릭하는 역할을 할 버튼에서 호출할 함수
    public void OpenModal()
    {
        // 모달(modalPanelObject)을 활성화하여 화면에 표시
        if (modalPanelObject != null)
        {
            modalPanelObject.SetActive(true);
            Debug.Log("PhotoModal: 모달창 열림.");
        }

        // 숨겨야 할 오브젝트들을 모두 비활성화
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // 모달창 내 '닫기' 버튼에서 호출할 함수 (public이어야 합니다)
    public void CloseModal()
    {
        // 모달(modalPanelObject)을 비활성화하여 화면에서 숨김
        if (modalPanelObject != null)
        {
            modalPanelObject.SetActive(false);
            Debug.Log("PhotoModal: 모달창 닫힘.");
        }

        // 숨겼던 오브젝트들을 다시 활성화
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}