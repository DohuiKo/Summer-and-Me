// 파일 이름이 반드시 WordDropZone.cs 이어야 합니다.
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class WordDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
        {
            Debug.LogWarning("[WordDropZone] 드롭 이벤트 데이터가 유효하지 않습니다.");
            return;
        }

        GameObject draggedObject = eventData.pointerDrag;
        Debug.Log($"'{draggedObject.name}'이(가) '{gameObject.name}'에 드롭됨.");

        // 드롭된 오브젝트에서 WordDragger 컴포넌트를 찾습니다.
        WordDragger wordDragger = draggedObject.GetComponent<WordDragger>();
        if (wordDragger != null)
        {
            // WordDragger가 돌아갈 부모를 이 DropZone으로 설정합니다.
            // OnEndDrag에서 이 부모를 기준으로 배치됩니다.
            wordDragger.parentToReturnTo = this.transform;
        }
        else
        {
            Debug.LogError($"[WordDropZone] 드롭된 오브젝트('{draggedObject.name}')에 WordDragger가 없습니다!");
        }
    }
}