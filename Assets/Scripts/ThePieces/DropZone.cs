using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // 다른 오브젝트가 이 UI 위에 드롭되었을 때 호출됩니다.
    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 오브젝트가 드래그 가능한 아이템인지 확인합니다.
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem != null)
        {
            Debug.Log(eventData.pointerDrag.name + " 아이템을 가방에 챙겼습니다.");

            // InventoryManager에게 아이템이 수집되었다고 알립니다.
            InventoryManager.instance.ItemCollected();

            // [선택사항] 아이템의 새 부모를 이 '가방'으로 설정해서 관리할 수 있습니다.
            draggableItem.transform.SetParent(this.transform);

            // --- [추가된 코드] ---
            // 드롭된 아이템 오브젝트를 비활성화하여 화면에서 보이지 않게 합니다.
            draggableItem.gameObject.SetActive(false);
            // --------------------
        }
    }
}