using UnityEngine;
using UnityEngine.UI;

public class SpriteToggler : MonoBehaviour
{
    // Inspector에서 교체할 모든 스프라이트를 순서대로 할당합니다.
    public Sprite[] spritesToCycle; 
    
    // ⭐ 새로 추가: 문이 열렸을 때 활성화할 버튼 오브젝트
    public GameObject unlockButton; 

    private Image targetImage;
    private int currentSpriteIndex = 0;

    void Start()
    {
        targetImage = GetComponent<Image>();
        if (spritesToCycle.Length > 0 && targetImage != null)
        {
            targetImage.sprite = spritesToCycle[currentSpriteIndex];
            
            // ⭐ 초기: 잠금 해제 버튼을 비활성화 상태로 확인
            if(unlockButton != null)
            {
                unlockButton.SetActive(false);
            }
        }
    }

    public void CycleSprite()
    {
        // 다음 인덱스로 이동합니다.
        currentSpriteIndex = (currentSpriteIndex + 1) % spritesToCycle.Length;

        // 스프라이트 교체
        if (targetImage != null)
        {
            targetImage.sprite = spritesToCycle[currentSpriteIndex];
        }

        // ⭐ 핵심 로직: 문이 열린 상태(두 번째 스프라이트, 인덱스 1)일 때 버튼 활성화
        // (배열의 0번이 닫힌 문, 1번이 열린 문이라고 가정)
        if (currentSpriteIndex == 1 && unlockButton != null)
        {
            unlockButton.SetActive(true); // 버튼 등장
        }
    }
}