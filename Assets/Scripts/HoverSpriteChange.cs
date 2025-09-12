using UnityEngine;

public class HoverSpriteChange : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer target;   // 대상 렌더러 (없으면 자동 할당)
    public Sprite defaultSprite;
    public Sprite hoverSprite;

    [Header("Behavior")]
    public bool lockOnClick = true; // 클릭 시 이후 hover 무시
    public Behaviour[] disableOnClickComponents; // 클릭 후 끌 스크립트들(선택)

    private bool isLocked = false;

    private void Reset()
    {
        target = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (target == null) target = GetComponent<SpriteRenderer>();
        if (target != null && defaultSprite != null)
            target.sprite = defaultSprite;
    }

    private void OnMouseEnter()
    {
        if (isLocked) return;
        if (target != null && hoverSprite != null)
            target.sprite = hoverSprite;
    }

    private void OnMouseExit()
    {
        if (isLocked) return;
        if (target != null && defaultSprite != null)
            target.sprite = defaultSprite;
    }

    private void OnMouseDown()
    {
        // 클릭 시 기본으로 되돌리고 잠금
        if (target != null && defaultSprite != null)
            target.sprite = defaultSprite;

        if (lockOnClick)
        {
            isLocked = true;

            // (선택) 기존 hover 관련 스크립트들 비활성화
            if (disableOnClickComponents != null)
            {
                foreach (var c in disableOnClickComponents)
                    if (c != null) c.enabled = false;
            }
        }
    }

    // 외부에서 잠금 해제하고 싶을 때 호출
    public void Unlock()
    {
        isLocked = false;
        if (target != null && defaultSprite != null)
            target.sprite = defaultSprite;
    }
}
