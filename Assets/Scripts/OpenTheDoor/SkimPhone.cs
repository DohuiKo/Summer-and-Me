using UnityEngine;

public class SkimPhone : MonoBehaviour
{
    // 💡 Inspector에서 연결: 확대된 휴대폰 미니 게임 패널
    public GameObject phoneMiniGamePanel;

    // 💡 Inspector에서 연결: 다이얼 그룹의 부모 오브젝트 (PhoneDial)
    public GameObject dialGroupObject;

    // 💡 새로 추가: 다이얼 바늘 오브젝트 (needle)를 직접 연결합니다.
    public GameObject needleObject; 

    // 💡 휴대폰을 클릭했을 때 호출될 함수
    public void StartPhoneView()
    {
        // 1. 작은 휴대폰 오브젝트(자기 자신)를 숨깁니다.
        gameObject.SetActive(false);
        Debug.Log("SkimPhone: 작은 휴대폰 숨김.");

        // 2. 확대된 휴대폰 미니 게임 패널 활성화
        if (phoneMiniGamePanel != null)
        {
            phoneMiniGamePanel.SetActive(true);
            Debug.Log("SkimPhone: 휴대폰 미니 게임 화면 띄움.");
        }
        else
        {
            Debug.LogError("SkimPhone: phoneMiniGamePanel이 연결되지 않았습니다!");
        }

        // 3. 다이얼 그룹 (PhoneDial) 활성화
        if (dialGroupObject != null)
        {
            dialGroupObject.SetActive(true);
            Debug.Log("SkimPhone: 다이얼 그룹 활성화 완료.");
        }
        
        // 4. 💡 문제 해결 조치: 자식(needle)을 명시적으로 한 번 더 활성화
        // 부모가 켜진 직후 자식도 확실히 켜지도록 강제합니다.
        if (needleObject != null)
        {
            needleObject.SetActive(true);
            Debug.Log("SkimPhone: 바늘(needle) 오브젝트 강제 활성화 완료.");
        }
    }
}