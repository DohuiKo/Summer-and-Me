using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 처리를 위해 필수입니다.

// 드래그 중인 아이템이 다른 UI의 클릭을 방해하지 않도록 CanvasGroup이 필요합니다.
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition; // 드래그 실패 시 돌아갈 원래 위치
    private Transform startParent; // 드래그 실패 시 돌아갈 원래 부모

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

        // 드래그를 시작했을 때 한 번 호출됩니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 드래그하는 동안 화면 맨 위에 보이도록 Canvas의 자식으로 잠시 옮깁니다.
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // ✅ 클릭 사운드 재생 추가
        if (Chap2SoundManager.Instance != null)
            Chap2SoundManager.Instance.PlayMouseClick();
    }

    // 드래그하는 동안 매 프레임 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 커서를 따라 아이템이 움직이게 합니다.
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    // 드래그를 끝냈을 때(마우스 버튼을 뗐을 때) 한 번 호출됩니다.
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true; // 다시 클릭을 막도록 되돌립니다.

        // 만약 드롭에 성공해서 부모가 바뀌지 않았다면 (즉, 가방 위에 놓이지 않았다면)
        if (transform.parent == transform.root)
        {
            // 원래 위치와 부모로 되돌아갑니다.
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
        }
    }



}