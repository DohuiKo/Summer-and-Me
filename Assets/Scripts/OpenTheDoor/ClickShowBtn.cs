using UnityEngine;

public class ClickShowBtn : MonoBehaviour
{
    // Inspector 창에서 나타나게 할 Unlock 버튼을 연결할 변수입니다.
    public GameObject unlockButton;

    // '가을이' 버튼의 OnClick() 이벤트에서 호출할 함수입니다.
    public void ShowUnlockButton()
    {
        // unlockButton 변수에 연결된 게임 오브젝트를 활성화(화면에 보이게) 합니다.
        if (unlockButton != null)
        {
            unlockButton.SetActive(true);
            Debug.Log("가을이 클릭! Unlock 버튼을 활성화합니다.");
        }
    }
}