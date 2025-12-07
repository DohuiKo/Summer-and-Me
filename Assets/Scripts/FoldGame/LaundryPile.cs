using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LaundryPile : MonoBehaviour, IPointerClickHandler
{
    // ───────────────────────────────────────────────
    // ■ 상태 데이터: Sprite + Size(선택적)
    // ───────────────────────────────────────────────
    [System.Serializable]
    public class LaundryStateData
    {
        public Sprite sprite;                        // 단계별 이미지
        public Vector2 size = Vector2.zero;          // 단계별 크기 (0,0이면 크기 변경 없음)
    }

    // ───────────────────────────────────────────────
    // ■ 빨래 세트 : 여러 단계(states)를 포함
    // ───────────────────────────────────────────────
    [System.Serializable]
    public class LaundrySet
    {
        public string name;
        public LaundryStateData[] states;            // ★ Sprite + Size 배열
    }

    // ───────────────────────────────────────────────
    //  인스펙터 필드
    // ───────────────────────────────────────────────
    [Header("빨래 세트들")]
    [SerializeField] private LaundrySet[] laundrySets;

    [Header("빨래 프리팹 (LaundryItem 포함된 프리팹)")]
    [SerializeField] private GameObject laundryItemPrefab;

    [Header("스폰 위치 (보통 Canvas 내부의 RectTransform)")]
    [SerializeField] private Transform spawnParent;

    [Header("게임 오브젝트 참조")]
    [SerializeField] private FoldingArea foldingArea;

    private int currentSetIndex = 0;


    // ───────────────────────────────────────────────
    //  클릭 시 빨래 생성
    // ───────────────────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클리어 후에는 생성 X
        if (LaundryGameManager.Instance != null && LaundryGameManager.Instance.IsGameCleared())
            return;

        // 접는 공간에 이미 아이템이 있으면 생성 X
        if (foldingArea != null && foldingArea.GetCurrentItem() != null)
            return;

        SpawnLaundry(eventData);
    }


    // ───────────────────────────────────────────────
    //  빨래 스폰 로직
    // ───────────────────────────────────────────────
    private void SpawnLaundry(PointerEventData eventData)
    {
        if (laundryItemPrefab == null || laundrySets == null || laundrySets.Length == 0)
            return;

        // 빨래 세트 선택
        LaundrySet currentSet = laundrySets[currentSetIndex % laundrySets.Length];

        // 프리팹 생성
        GameObject laundryObj = Instantiate(laundryItemPrefab, spawnParent);
        RectTransform rect = laundryObj.GetComponent<RectTransform>();

        // 클릭 지점으로 위치 이동
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            spawnParent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rect.anchoredPosition = localPoint;

        // 초기화 (LaundryItem.cs 의 Initialize 호출)
        LaundryItem item = laundryObj.GetComponent<LaundryItem>();
        if (item != null)
        {
            item.Initialize(currentSet.states, foldingArea);   // ★ size 포함된 states 전달
        }

        currentSetIndex++;

        // 안내 갱신
        LaundryGameManager.Instance?.UpdateInstructionText();
    }


    // 필요 시 외부에서 사용
    public Transform GetSpawnParent() => spawnParent;
}
