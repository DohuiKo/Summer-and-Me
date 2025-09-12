using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class UICrossfadeSprite : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.35f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
    public bool setNativeSize = false;

    Image main;      // 원본 Image (이 오브젝트에 붙어있는)
    Image overlay;   // 런타임에 자동 생성되는 전환용

    bool playing;

    void Awake()
    {
        main = GetComponent<Image>();
        EnsureOverlay();
    }

    void EnsureOverlay()
    {
        if (overlay != null) return;

        // 오버레이 오브젝트 런타임 생성
        var go = new GameObject("Overlay (auto)", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(transform, worldPositionStays:false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = main.rectTransform.pivot;

        overlay = go.GetComponent<Image>();
        overlay.raycastTarget = false;                 // 클릭 방해 안 하도록
        overlay.color = new Color(1,1,1,0);            // 완전 투명
        overlay.enabled = true;
        go.SetActive(false);                           // 기본은 비활성
    }

    public void SwitchToSprite(Sprite next)
    {
        if (!next || playing) return;
        StartCoroutine(CoCrossfade(next));
    }

    IEnumerator CoCrossfade(Sprite next)
    {
        playing = true;
        EnsureOverlay();

        // 준비
        overlay.gameObject.SetActive(true);
        overlay.sprite = next;
        if (setNativeSize) overlay.SetNativeSize();

        Color mc = main.color;
        Color oc = overlay.color;
        float startMainA = mc.a;

        overlay.color = new Color(oc.r, oc.g, oc.b, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float e = curve.Evaluate(Mathf.Clamp01(t / duration));

            main.color    = new Color(mc.r, mc.g, mc.b, Mathf.Lerp(startMainA, 0f, e));
            overlay.color = new Color(oc.r, oc.g, oc.b, Mathf.Lerp(0f, 1f, e));
            yield return null;
        }

        // 확정 및 초기화
        main.sprite = next;
        if (setNativeSize) main.SetNativeSize();

        main.color    = new Color(mc.r, mc.g, mc.b, startMainA);
        overlay.color = new Color(oc.r, oc.g, oc.b, 0f);
        overlay.gameObject.SetActive(false);

        playing = false;
    }
}
