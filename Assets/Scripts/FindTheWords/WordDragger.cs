// WordDragger.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WordDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 단어가 드롭된 후 돌아갈 부모 위치를 저장하는 변수
    public Transform parentToReturnTo = null;
    
    // 드래그 시 UI가 다른 UI 뒤로 숨지 않도록 도와주는 컴포넌트
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // 게임이 시작될 때 CanvasGroup 컴포넌트를 자동으로 추가하고 저장합니다.
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 드래그가 "시작"될 때 딱 한 번 호출되는 함수
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 원래 단어가 있던 컨테이너(부모)를 기억해둡니다.
        parentToReturnTo = this.transform.parent;
        // 드래그하는 동안에는 최상위 캔버스의 자식으로 잠시 이동시켜 화면 맨 앞에 보이게 합니다.
        this.transform.SetParent(this.transform.root);
        // 드래그 중에는 마우스 이벤트가 통과되도록 하여 아래에 있는 DropZone이 이벤트를 받을 수 있게 합니다.
        canvasGroup.blocksRaycasts = false;
    }

    // 드래그하는 "동안" 계속해서 호출되는 함수
    public void OnDrag(PointerEventData eventData)
    {
        // 단어의 위치를 마우스 커서 위치로 계속 업데이트합니다.
        this.transform.position = eventData.position;
    }

    // 드래그가 "끝났을" 때 딱 한 번 호출되는 함수
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝나면 DropZone에 의해 새로 지정된 부모(parentToReturnTo)로 돌아갑니다.
        this.transform.SetParent(parentToReturnTo);
        // 마우스 이벤트를 다시 정상적으로 받도록 설정합니다.
        canvasGroup.blocksRaycasts = true;
    }

    // 드래그 대신 "클릭"했을 때 호출되는 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        // [수정됨] GameManager.instance를 WordGameManager.instance로 변경
        WordGameManager.instance.MoveWordOnClick(this.gameObject);
    }
}