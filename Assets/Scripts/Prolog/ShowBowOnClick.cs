using UnityEngine;

public class ShowBoxOnClick : MonoBehaviour
{
    [SerializeField] GameObject boxObject;     // Square 드래그
    [SerializeField] bool toggle = false;      // 다시 클릭하면 숨길지 여부
    [SerializeField] bool forceStartHidden = true;

    void Awake()
    {
        if (boxObject && forceStartHidden) boxObject.SetActive(false);
    }

    // 드래그로 클릭이 씹히는 걸 방지하려면 OnMouseUpAsButton이 더 안전
    void OnMouseUpAsButton()
    {
        if (!boxObject) return;

        if (toggle) boxObject.SetActive(!boxObject.activeSelf);
        else boxObject.SetActive(true);

        // Square가 맨 위로 오도록 권장
        var sr = boxObject.GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = 999;
    }
}
