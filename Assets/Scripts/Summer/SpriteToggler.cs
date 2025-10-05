using UnityEngine;
using UnityEngine.UI; // UI.Image를 사용하기 위해 필요합니다.

// 이 스크립트가 붙어있는 게임오브젝트에 Image 컴포넌트가 있다고 가정합니다.
[RequireComponent(typeof(Image))]
public class SpriteToggler : MonoBehaviour
{
    // [인스펙터 창에서 연결할 항목들]
    public Sprite[] spritesToCycle; // 순환시킬 스프라이트 목록 (이미지)
    public GameObject unlockButton;   // 마지막에 활성화할 버튼

    private Image imageComponent;     // 스프라이트를 바꿀 Image 컴포넌트
    private int currentIndex = 0;     // 현재 어떤 스프라이트를 보여주고 있는지 기억하는 변수
    private bool isCycleFinished = false; // 순환이 끝났는지 확인하는 변수

    void Start()
    {
        // 1. Image 컴포넌트 가져오기
        imageComponent = GetComponent<Image>();

        // 2. 시작할 때 첫 번째 스프라이트(Element 0)로 설정
        if (spritesToCycle.Length > 0)
        {
            imageComponent.sprite = spritesToCycle[0];
        }

        // 3. 시작할 때 Unlock 버튼은 반드시 비활성화 상태로 설정
        if (unlockButton != null)
        {
            unlockButton.SetActive(false);
        }
    }

    // 스프라이트를 바꾸고 싶을 때 이 함수를 호출하면 됩니다.
    // (예: 다른 버튼의 OnClick() 이벤트에 연결)
    public void DisplayNextSprite()
    {
        // 만약 순환이 이미 끝났다면, 아무것도 실행하지 않고 함수를 종료합니다.
        if (isCycleFinished)
        {
            return;
        }

        // 다음 스프라이트를 보여주기 위해 인덱스를 1 증가시킵니다.
        currentIndex++;

        // 현재 인덱스가 스프라이트 배열의 범위 안에 있는지 확인합니다.
        if (currentIndex < spritesToCycle.Length)
        {
            // Image 컴포넌트의 스프라이트를 다음 것으로 교체합니다.
            imageComponent.sprite = spritesToCycle[currentIndex];

            // ★★★ 핵심 로직 ★★★
            // 만약 현재 인덱스가 마지막 인덱스와 같다면,
            if (currentIndex == spritesToCycle.Length - 1)
            {
                // 1. 이제 순환이 끝났다고 표시하여 더 이상 바뀌지 않게 합니다.
                isCycleFinished = true;

                // 2. Unlock 버튼을 활성화시킵니다.
                if (unlockButton != null)
                {
                    unlockButton.SetActive(true);
                }
            }
        }
    }
}