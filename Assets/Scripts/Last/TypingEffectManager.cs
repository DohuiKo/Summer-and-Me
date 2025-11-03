using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TypingEffectManager : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text targetText;
    [TextArea(3, 10)] public string fullText;
    public float typingSpeed = 0.12f;

    [Header("Panels")]
    public CanvasGroup currentPanel;
    public CanvasGroup recodingPanel;

    [Header("Events")]
    public UnityEvent onTypingComplete;

    private Coroutine typingRoutine;
    private bool isTyping = false;

    public void StartTyping()
    {
        if (targetText == null)
        {
            Debug.LogError("[TypingEffectManager] ❌ targetText 누락");
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
        }

        typingRoutine = StartCoroutine(TypingRoutine());
        Debug.Log("✏️ StartTyping() 실행됨");
    }

    private IEnumerator TypingRoutine()
    {
        isTyping = true;
        targetText.text = "";

        // 🔊 타이핑 시작할 때 1회 사운드 재생
        Chap5SoundManager.Instance?.PlayContinueTypingSFX();

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        onTypingComplete?.Invoke();
        Debug.Log("[TypingEffectManager] ✅ 타이핑 완료 → 패널 전환 시작");
        StartCoroutine(FadeToRecodingPanel());
    }

    private IEnumerator FadeToRecodingPanel()
    {
        if (currentPanel == null || recodingPanel == null)
        {
            Debug.LogError("[TypingEffectManager] ❌ CanvasGroup 누락");
            yield break;
        }

        recodingPanel.gameObject.SetActive(true);
        recodingPanel.alpha = 0f;

        float duration = 1.2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            currentPanel.alpha = 1f - a;
            recodingPanel.alpha = a;
            yield return null;
        }

        currentPanel.alpha = 0f;
        currentPanel.gameObject.SetActive(false);
        recodingPanel.alpha = 1f;

        Debug.Log("🎬 RecoTextPanel → RecodingPanel 전환 완료");
    }
}
