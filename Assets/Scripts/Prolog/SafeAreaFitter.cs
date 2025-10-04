using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafe;

    void Awake() { rt = GetComponent<RectTransform>(); Apply(); }
    void OnEnable() { Apply(); }

    void Update()  // 해상도/회전 변경 추적
    {
        if (Screen.safeArea != lastSafe) Apply();
    }

    void Apply()
    {
        var sa = Screen.safeArea; lastSafe = sa;
        var size = new Vector2(Screen.width, Screen.height);

        Vector2 anchorMin = sa.position;
        Vector2 anchorMax = sa.position + sa.size;
        anchorMin.x /= size.x; anchorMin.y /= size.y;
        anchorMax.x /= size.x; anchorMax.y /= size.y;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
