using UnityEngine;
using TMPro;
using System.Collections;

public class FloatText : MonoBehaviour
{
    public float floatAmplitude = 10f;
    public float floatSpeed = 1f;
    public float horizontalAmplitude = 5f;
    public float alphaSpeed = 0.8f;

    private RectTransform rect;
    private TextMeshProUGUI text;
    private Vector2 startPos;
    private float timeOffset;
    private bool isActive = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        text = GetComponent<TextMeshProUGUI>();
        startPos = rect.anchoredPosition;
        timeOffset = Random.Range(0f, 10f);
        SetAlpha(0f);
    }

    void Update()
    {
        if (!isActive) return;

        float t = Time.time + timeOffset;

        // 위치 변화
        float newY = startPos.y + Mathf.Sin(t * floatSpeed) * floatAmplitude;
        float newX = startPos.x + Mathf.Cos(t * floatSpeed * 0.7f) * horizontalAmplitude;
        rect.anchoredPosition = new Vector2(newX, newY);

        // 알파 변화
        if (text != null)
        {
            float alpha = Mathf.PingPong(t * alphaSpeed, 1f);
            var color = text.color;
            color.a = Mathf.Lerp(0.3f, 1f, alpha);
            text.color = color;
        }
    }

    // ✅ 퍼즐매니저가 호출하는 함수
    public void Activate()
    {
        Debug.Log($"🌬️ {gameObject.name} FloatText 활성화됨"); 
        isActive = true;
        StartCoroutine(FadeIn());
        
    }

    private IEnumerator FadeIn()
    {
        Debug.Log($"✏️ {gameObject.name} 페이드인 시작");
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            SetAlpha(Mathf.Lerp(0f, 1f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(1f);
    }

    private void SetAlpha(float value)
    {
        if (text == null) return;
        var color = text.color;
        color.a = value;
        text.color = color;
    }
    void OnEnable() => SetAlpha(1f);


}
