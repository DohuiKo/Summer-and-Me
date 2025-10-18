using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LaundryPile : MonoBehaviour, IPointerClickHandler
{
    [Header("빨래 세트들")]
    [SerializeField] private LaundrySet[] laundrySets;

    [Header("빨래 프리팹")]
    [SerializeField] private GameObject laundryItemPrefab;

    [Header("스폰 위치")]
    [SerializeField] private Transform spawnParent;

    [Header("게임 오브젝트 참조")]
    [SerializeField] private FoldingArea foldingArea;

    private int currentSetIndex = 0;

    [System.Serializable]
    public class LaundrySet
    {
        public string name;
        public Sprite[] states;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LaundryGameManager.Instance != null && LaundryGameManager.Instance.IsGameCleared())
            return;

        // 접는 공간에 이미 아이템이 있으면 스폰하지 않음
        if (foldingArea != null && foldingArea.GetCurrentItem() != null)
            return;

        SpawnLaundry(eventData);
    }

    private void SpawnLaundry(PointerEventData eventData)
    {
        if (laundryItemPrefab == null || laundrySets == null || laundrySets.Length == 0) return;

        LaundrySet currentSet = laundrySets[currentSetIndex % laundrySets.Length];

        // 프리팹 생성
        GameObject laundryObj = Instantiate(laundryItemPrefab, spawnParent);
        RectTransform rectTransform = laundryObj.GetComponent<RectTransform>();

        // 클릭 지점에 배치
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            spawnParent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;

        // 초기화
        LaundryItem laundryItem = laundryObj.GetComponent<LaundryItem>();
        if (laundryItem != null)
            laundryItem.Initialize(currentSet.states, foldingArea);

        currentSetIndex++;

        // 안내 갱신
        if (LaundryGameManager.Instance != null)
            LaundryGameManager.Instance.UpdateInstructionText();
    }

    // 필요 시 외부에서 사용
    public Transform GetSpawnParent() => spawnParent;
}
