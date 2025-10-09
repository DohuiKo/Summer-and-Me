using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // 다른 스크립트에서 쉽게 접근할 수 있도록 static instance를 만듭니다 (싱글톤).
    public static InventoryManager instance;

    [Tooltip("수집해야 할 총 아이템의 개수입니다.")]
    public int totalItemsToCollect = 6;
    
    [Tooltip("모든 아이템을 수집했을 때 나타날 '스크롤 잠금 해제' 버튼입니다.")]
    public GameObject scrollUnlockButton;

    private int itemsCollected = 0; // 현재까지 수집한 아이템 개수

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시에는 버튼을 비활성화합니다.
        if (scrollUnlockButton != null)
        {
            scrollUnlockButton.SetActive(false);
        }
    }

    // DropZone에서 아이템이 드롭될 때마다 이 함수를 호출합니다.
    public void ItemCollected()
    {
        itemsCollected++;
        Debug.Log("아이템 수집: " + itemsCollected + " / " + totalItemsToCollect);

        // 모든 아이템을 수집했는지 확인합니다.
        if (itemsCollected >= totalItemsToCollect)
        {
            CompleteCollection();
        }
    }

    private void CompleteCollection()
    {
        Debug.Log("모든 아이템 수집 완료!");
        if (scrollUnlockButton != null)
        {
            scrollUnlockButton.SetActive(true);
        }
    }
}