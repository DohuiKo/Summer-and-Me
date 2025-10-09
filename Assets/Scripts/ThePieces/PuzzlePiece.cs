using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("각 퍼즐 조각의 고유 ID입니다. 0, 1, 2, 3... 과 같이 겹치지 않게 설정하세요.")]
    public int pieceID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling(); // 드래그 시 가장 위에 보이게 함
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        
        // 드롭에 성공하지 못하면 원래 위치로 돌아갑니다.
        // (성공 여부는 PuzzleSlot에서 처리하므로 여기서는 부모가 바뀌었는지 등으로 확인할 필요가 없습니다.)
        // 실제로는 PuzzleSlot에서 성공 시 위치를 고정시켜주므로, 이 코드는 거의 호출되지 않습니다.
        // 하지만 만약을 대비해 남겨둡니다.
        if (Vector2.Distance(rectTransform.anchoredPosition, startPosition) > 1f) // 약간의 오차범위
        {
           // 이 부분은 슬롯에서 성공적으로 처리되지 않았을 때만 실행되도록,
           // 혹은 아예 슬롯에 놓이지 않았을 때 시작 위치로 돌려보내는 로직으로 사용할 수 있습니다.
           // 지금 구조에서는 슬롯이 모든 것을 처리하므로 비워둬도 괜찮습니다.
        }
    }

    // 슬롯에 성공적으로 맞춰졌을 때 호출될 함수
    public void LockPiece()
    {
        // 맞춰진 조각은 더 이상 드래그할 수 없도록 이 스크립트를 비활성화합니다.
        enabled = false;
    }
}