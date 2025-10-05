using UnityEngine;
using UnityEngine.UI;

public class UICactus : MonoBehaviour
{
    [Header("Refs")]
    public Image cactusImage;          // 선인장 이미지 (UI Image)
    public Button scrollUnlockButton;  // 스크롤락 해제 버튼 (비활성화 상태로 시작)

    [Header("Stack Settings")]
    public int maxStack = 3;           // 최대로 쌓일 스택 수
    private int stackCount = 0;        // 현재 스택 수

    [Header("Color Settings")]
    public Color grayColor = Color.gray;   // 처음(흑백)
    public Color fullColor = Color.white;  // 완전 복원 색상
    public float colorFadeSpeed = 0.3f;    // 색이 부드럽게 돌아오는 속도 (초)

    void Start()
    {
        if (scrollUnlockButton)
            scrollUnlockButton.gameObject.SetActive(false); // 처음엔 버튼 비활성화

        UpdateCactusColorInstant();
    }

    public void AddStack(int amount)
    {
        stackCount += amount;
        stackCount = Mathf.Clamp(stackCount, 0, maxStack);

        Debug.Log($"🌵 Cactus stack = {stackCount}");

        StopAllCoroutines();
        StartCoroutine(FadeCactusColor());

        // 완전히 복원되면 스크롤 해제 버튼 활성화
        if (stackCount >= maxStack && scrollUnlockButton)
        {
            scrollUnlockButton.gameObject.SetActive(true);
        }
    }

    System.Collections.IEnumerator FadeCactusColor()
    {
        if (!cactusImage) yield break;

        float t = Mathf.InverseLerp(1, maxStack, stackCount);
        Color targetColor = Color.Lerp(grayColor, fullColor, t);
        Color startColor = cactusImage.color;
        float elapsed = 0f;

        while (elapsed < colorFadeSpeed)
        {
            cactusImage.color = Color.Lerp(startColor, targetColor, elapsed / colorFadeSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cactusImage.color = targetColor;
    }

    void UpdateCactusColorInstant()
    {
        if (!cactusImage) return;
        float t = Mathf.InverseLerp(1, maxStack, stackCount);
        cactusImage.color = Color.Lerp(grayColor, fullColor, t);
    }
}
