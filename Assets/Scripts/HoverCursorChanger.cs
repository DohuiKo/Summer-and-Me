using UnityEngine;

public class HoverCursorChanger : MonoBehaviour
{
    [Header("Cursor Textures")]
    public Texture2D hoverCursor;          // 오브젝트에 마우스 올렸을 때 커서
    public Vector2 hotspot = Vector2.zero; // 클릭 지점 (보통 (0,0) 또는 이미지 중심)
    
    private Texture2D defaultCursor;       // 기본 커서 (null이면 시스템 기본 사용)
    private Vector2 defaultHotspot = Vector2.zero;

    void Start()
    {
        // 기본 커서 저장 (null = 시스템 기본 커서)
        defaultCursor = null;
    }

    void OnMouseEnter()
    {
        // 마우스가 오브젝트에 올라갔을 때
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        // 마우스가 오브젝트에서 벗어났을 때
        Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
    }
}
