using UnityEngine;
using UnityEngine.UI;

public class SpriteToggler : MonoBehaviour
{
    // Inspector에서 교체할 모든 스프라이트를 순서대로 할당합니다.
    public Sprite[] spritesToCycle; 
    
    private Image targetImage;
    private int currentSpriteIndex = 0;

    void Start()
    {
        // 1. Image 컴포넌트 가져오기
        targetImage = GetComponent<Image>();

        // 2. 교체할 스프라이트 목록이 비어있지 않고, Image 컴포넌트가 있다면 초기 스프라이트 설정
        if (spritesToCycle.Length > 0 && targetImage != null)
        {
            targetImage.sprite = spritesToCycle[currentSpriteIndex];
        }
        else
        {
            Debug.LogError("Sprite Toggler에 스프라이트 목록이 비어있거나 Image 컴포넌트가 없습니다!", this);
        }
    }

    // Button 컴포넌트의 OnClick 이벤트에 연결할 범용 함수
    public void CycleSprite()
    {
        // 다음 인덱스로 이동합니다. 배열의 끝에 도달하면 0으로 리셋합니다.
        currentSpriteIndex = (currentSpriteIndex + 1) % spritesToCycle.Length;

        // 스프라이트 교체
        if (targetImage != null)
        {
            targetImage.sprite = spritesToCycle[currentSpriteIndex];
        }
    }
}