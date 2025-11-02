using UnityEngine;

public class PopupPanel : MonoBehaviour
{
    // 'X' 닫기 버튼에 연결
    public void ClosePopup()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트(팝업 패널)를 비활성화합니다.
        gameObject.SetActive(false);
    }
}