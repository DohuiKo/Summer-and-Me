using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverLockAfterClick : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Mode")]
    public bool useColorTint = false;   // true: 색상, false: 스프라이트

    [Header("Sprite Swap (useColorTint=false)")]
    public Sprite normalSprite;
    public Sprite highlightSprite;

    [Header("Color Tint (useColorTint=true)")]
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.9f);

    private Image img;
    private bool highlightEnabled = true; // ← 클릭 전에는 하이라이트 허용

    void Awake()
    {
        img = GetComponent<Image>();
        if (!img) img = gameObject.AddComponent<Image>();

        var btn = GetComponent<Button>();
        if (btn) btn.transition = Selectable.Transition.None;

        // normalSprite 미지정 시 현재 스프라이트를 원본으로 사용
        if (!useColorTint && normalSprite == null && img.sprite != null)
            normalSprite = img.sprite;

        ApplyNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightEnabled) ApplyHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 항상 Normal로. (여기서 highlightEnabled 값을 바꾸지 않음)
        ApplyNormal();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 순간부터 하이라이트 완전 금지(잠금)
        highlightEnabled = false;
        ApplyNormal();
    }

    // ───────── 수동 해제/재허용 API (필요할 때 외부에서 호출) ─────────
    public void EnableHighlight()  { highlightEnabled = true; }
    public void DisableHighlight() { highlightEnabled = false; ApplyNormal(); }

    // ───────────────────── 내부 적용 함수 ─────────────────────
    private void ApplyNormal()
    {
        if (useColorTint) img.color = normalColor;
        else if (normalSprite) img.sprite = normalSprite;
    }

    private void ApplyHighlight()
    {
        if (useColorTint) img.color = highlightColor;
        else if (highlightSprite) img.sprite = highlightSprite;
    }
}
