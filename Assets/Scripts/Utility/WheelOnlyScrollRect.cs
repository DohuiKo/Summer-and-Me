using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WheelOnlyScrollRect : ScrollRect
{
    // 마우스 / 터치 드래그만 막고, 휠/키보드 스크롤은 그대로 두기

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // base.OnBeginDrag(eventData);  ← 호출 안 함
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // base.OnDrag(eventData);       ← 호출 안 함
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // base.OnEndDrag(eventData);    ← 호출 안 함
    }
}
