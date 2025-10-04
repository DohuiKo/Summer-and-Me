using UnityEngine;
using UnityEngine.UI;

public class BedStateChanger : MonoBehaviour
{
    // 💡 Inspector에서 연결할 4가지 상태 Sprite 배열 (Element 0부터 3까지)
    public Sprite[] bedStates;

    // 💡 마지막 상태에 나타날 오브젝트 (Hierarchy에서 HiddenPhoneObj를 연결하세요)
    public GameObject hiddenObject; 

    // 💡 현재 침대 상태를 저장하는 변수 (0, 1, 2, 3)
    private int currentStateIndex = 0;

    // 💡 이미지 컴포넌트 참조 (이 스크립트가 붙은 오브젝트의 Image 컴포넌트)
    private Image bedImage;

    void Start()
    {
        bedImage = GetComponent<Image>();

        // 1. 상태 배열이 비어있으면 에러를 출력하고 함수 종료
        if (bedStates.Length == 0)
        {
            Debug.LogError("BedStateChanger: bedStates 배열에 스프라이트가 등록되지 않았습니다.");
            return;
        }

        // 2. 초기 이미지를 첫 번째 상태(Index 0)로 설정
        bedImage.sprite = bedStates[0];
        
        // 3. 시작 시 숨김 오브젝트가 확실히 비활성화되어 있도록 설정
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(false);
        }
    }

    // 💡 침대 버튼 클릭 시 호출되는 메인 함수
    public void ChangeState()
    {
        // 🚨 상태 잠금 로직: 마지막 상태(Index 3)에 이미 도달했는지 확인
        // bedStates.Length - 1 은 배열의 마지막 인덱스입니다. (이 경우 4 - 1 = 3)
        if (currentStateIndex >= bedStates.Length - 1)
        {
            Debug.Log("침대 탐색 완료! 마지막 상태에 도달하여 더 이상 상태를 변경할 수 없습니다.");
            return; // 상태 변경 로직 중단
        }

        // 상태 인덱스를 1 증가시킵니다. (0 -> 1 -> 2 -> 3)
        currentStateIndex++;

        // 새로운 상태 이미지로 변경합니다.
        bedImage.sprite = bedStates[currentStateIndex];

        Debug.Log($"침대 상태 변경: 상태 {currentStateIndex + 1} ({bedStates[currentStateIndex].name})");

        // 💡 마지막 상태(Index 3) 도달 시 휴대폰 오브젝트 활성화
        if (currentStateIndex == bedStates.Length - 1)
        {
            if (hiddenObject != null)
            {
                // 숨겨진 휴대폰 오브젝트를 활성화하여 화면에 보이게 합니다.
                hiddenObject.SetActive(true);
                Debug.Log("최종 미니 게임 완료: 숨겨진 휴대폰 오브젝트를 발견했습니다!");
            }
        }
    }
}