// DropZone.cs
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class WordDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogWarning("[DropZone] eventData is null");
            return;
        }

        // 드롭한 객체
        var draggedObj = eventData.pointerDrag;
        if (draggedObj == null)
        {
            Debug.LogWarning("[DropZone] pointerDrag is null");
            return;
        }

        Debug.Log($"{draggedObj.name} 이(가) {gameObject.name} 에 드롭됨.");

        // WordDragger 컴포넌트 찾기
        var d = draggedObj.GetComponent<WordDragger>();
        if (d != null)
        {
            // 부모를 이 영역으로
            d.parentToReturnTo = this.transform;
        }
        else
        {
            Debug.LogError($"[DropZone] 드롭된 오브젝트({draggedObj.name})에 WordDragger가 없습니다!");
        }
    }
}
