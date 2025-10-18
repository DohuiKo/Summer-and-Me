using UnityEngine;
using UnityEngine.EventSystems;

public class FoldingArea : MonoBehaviour, IDropHandler
{
    private LaundryItem currentItem = null;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public LaundryItem GetCurrentItem()
    {
        return currentItem;
    }

    public void SetCurrentItem(LaundryItem item)
    {
        currentItem = item;
    }

    public void ClearCurrentItem()
    {
        currentItem = null;
    }

    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }

    // [새로 추가됨] IDropHandler 인터페이스 구현
    public void OnDrop(PointerEventData eventData)
    {
        // 만약 이 공간에 이미 빨래가 있다면 아무것도 하지 않음
        if (currentItem != null) return;
        
        // 드롭된 오브젝트가 있다면
        if (eventData.pointerDrag != null)
        {
            // 드롭된 오브젝트의 LaundryItem 스크립트를 가져옴
            LaundryItem droppedItem = eventData.pointerDrag.GetComponent<LaundryItem>();
            if (droppedItem != null)
            {
                // LaundryItem에게 "너 여기 놓였어!" 라고 알려줌
                droppedItem.OnPlacedInArea(this);
            }
        }
    }
}