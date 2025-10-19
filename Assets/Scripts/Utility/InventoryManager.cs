using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("설정/상태")]
    [SerializeField] private int totalItemsToCollect = 0; // 총 수집 개수
    [SerializeField] private int currentCount = 0;

    [Header("UI(선택)")]
    public TextMeshProUGUI counterText;
    public GameObject nextButton;
    public bool autoUnlockOnComplete = true;   // ✅ 자동 언락 기본 켜짐

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }

        if (nextButton) nextButton.SetActive(false);
        UpdateUI();
    }

    // 외부에서 총 개수 세팅 (예: WaveRevealManager 등)
    public void SetTotalItemsToCollect(int value)
    {
        totalItemsToCollect = Mathf.Max(0, value);
        currentCount = Mathf.Clamp(currentCount, 0, totalItemsToCollect);
        UpdateUI();
    }

    // DropZone에서 아이템을 담을 때 호출
    public void ItemCollected()
    {
        currentCount = Mathf.Clamp(currentCount + 1, 0, Mathf.Max(totalItemsToCollect, currentCount + 1));
        UpdateUI();

        // ✅ 수집 완료 체크
        if (totalItemsToCollect > 0 && currentCount >= totalItemsToCollect)
        {
            OnAllItemsCollected();
        }
    }

    // ✅ 모든 아이템을 다 모았을 때 실행
    private void OnAllItemsCollected()
    {
        Debug.Log($"✅ 모든 아이템 수집 완료 ({currentCount}/{totalItemsToCollect})");

        if (autoUnlockOnComplete && nextButton)
        {
            nextButton.SetActive(true);
            Debug.Log("➡ NextSlideArrow 버튼 활성화됨!");
        }

        // 필요 시: 모달 닫기, 사운드 재생, 연출 등 추가 가능
    }

    private void UpdateUI()
    {
        if (counterText)
            counterText.text = $"{currentCount} / {totalItemsToCollect}";
    }

    // 필요 시 인벤토리 리셋
    public void ResetInventory()
    {
        currentCount = 0;
        UpdateUI();

        if (nextButton)
            nextButton.SetActive(false);
    }
}
