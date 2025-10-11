using UnityEngine;
using TMPro;          // 카운터 텍스트 쓰는 경우

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("설정/상태")]
    [SerializeField] private int totalItemsToCollect = 0; // 인스펙터에 "Total Items To Collect"
    [SerializeField] private int currentCount = 0;

    [Header("UI(선택)")]
    public TextMeshProUGUI counterText;
    public GameObject nextButton;
    public bool autoUnlockOnComplete = false;   // 자동 언락 켜고 싶을 때만 체크

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }

        if (nextButton) nextButton.SetActive(false);
        UpdateUI();
    }

    // ✅ WaveRevealManager가 총합을 넘겨줄 때 호출하는 메서드
    public void SetTotalItemsToCollect(int value)
    {
        totalItemsToCollect = Mathf.Max(0, value);
        // 이미 담은 개수가 목표보다 크면 클램프
        currentCount = Mathf.Clamp(currentCount, 0, totalItemsToCollect);
        UpdateUI();
    }

    // DropZone에서 아이템을 담을 때 호출
    public void ItemCollected()
    {
        currentCount = Mathf.Clamp(currentCount + 1, 0, Mathf.Max(totalItemsToCollect, currentCount + 1));
        UpdateUI();

        if (totalItemsToCollect > 0 && currentCount >= totalItemsToCollect)
        {
            if (autoUnlockOnComplete && nextButton)
                nextButton.SetActive(true);
            // 자동 언락을 안 쓰는 경우, 여기서는 신호만 내보내고
            // 모달 닫힘 등에서 다음 단계로 넘어가세요.
        }
    }

    private void UpdateUI()
    {
        if (counterText)
            counterText.text = $"{currentCount} / {totalItemsToCollect}";
    }
}
