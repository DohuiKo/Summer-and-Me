using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요합니다.
using UnityEngine.EventSystems;

public class ClickToSwapImage : MonoBehaviour, IPointerClickHandler
{
    [Header("클릭 설정")]
    [Tooltip("몇 번째 클릭에 반응할지 설정합니다.")]
    public int requiredClicks = 5;

    [Header("이미지 교체 설정")]
    [Tooltip("교체될 대상인 휴대폰 화면의 Image 컴포넌트를 연결하세요.")]
    public Image phoneScreenImage;

    [Tooltip("교체할 '흐릿한' 스프라이트를 연결하세요.")]
    public Sprite blurrySprite;

    private int currentClickCount = 0;
    private bool hasSwapped = false; // 중복 실행을 방지하기 위한 플래그

    public void OnPointerClick(PointerEventData eventData)
    {
        // 이미지가 교체되었다면 더 이상 아무것도 하지 않습니다.
        if (hasSwapped) return;

        currentClickCount++;
        Debug.Log($"클릭! 현재 횟수: {currentClickCount}");

        // 클릭 횟수가 설정한 값에 도달했는지 확인합니다.
        if (currentClickCount >= requiredClicks)
        {
            // phoneScreenImage 변수와 blurrySprite 변수가 비어있지 않은지 확인합니다.
            if (phoneScreenImage != null && blurrySprite != null)
            {
                Debug.Log("5번째 클릭! 이미지를 흐릿한 버전으로 교체합니다.");
                
                // 여기가 핵심! Image 컴포넌트의 sprite를 흐릿한 스프라이트로 교체합니다.
                phoneScreenImage.sprite = blurrySprite;
                
                hasSwapped = true; // 교체 완료 플래그를 true로 설정
            }
            else
            {
                Debug.LogError("Phone Screen Image 또는 Blurry Sprite가 연결되지 않았습니다!");
            }
        }
    }
}