using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Image 컴포넌트를 위해 추가

[RequireComponent(typeof(Image))]
public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("이 슬롯에 맞는 퍼즐 조각의 ID를 설정하세요.")]
    public int targetPieceID;

    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 오브젝트가 PuzzlePiece인지 확인합니다.
        PuzzlePiece droppedPiece = eventData.pointerDrag.GetComponent<PuzzlePiece>();
        
        // 올바른 조각이 드롭되었다면
        if (droppedPiece != null && droppedPiece.pieceID == targetPieceID)
        {
            Debug.Log("정답! " + targetPieceID + "번 조각을 맞췄습니다.");
            
            // 조각을 슬롯의 중앙에 정확히 위치시킵니다.
            droppedPiece.GetComponent<RectTransform>().anchoredPosition = this.GetComponent<RectTransform>().anchoredPosition;
            
            // 더 이상 움직이지 않도록 조각을 잠급니다.
            droppedPiece.LockPiece();
            
            // 퍼즐 매니저에게 조각이 맞춰졌다고 알립니다.
            PuzzleManager.instance.PiecePlaced();
        }
    }
}