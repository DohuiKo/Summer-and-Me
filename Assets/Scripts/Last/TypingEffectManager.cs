using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TypingEffectManager : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("타이핑될 실제 TMP_Text 오브젝트")]
    public TMP_Text targetText;

    [Tooltip("전체 문장 (자동 타이핑으로 출력할 내용)")]
    [TextArea(3, 10)]
    public string fullText;

    [Tooltip("글자 타이핑 속도 (초 단위)")]
    public float typingSpeed = 0.12f;

    [Header("Panels (전환 대상)")]
    [Tooltip("현재 패널 (타이핑 패널)")]
    public CanvasGroup currentPanel;
    [Tooltip("다음에 열릴 Recoding 패널")]
    public CanvasGroup recodingPanel;

    [Header("Events")]
    [Tooltip("타이핑이 전부 끝났을 때 실행될 이벤트")]
    public UnityEvent onTypingComplete;

    private Coroutine typingRoutine;
    private bool isTyping = false;

    // 외부 호출용
    public void StartTyping()
    {
        if (targetText == null)
        {
            Debug.LogError("[TypingEffectManager] ❌ targetText가 연결되지 않았습니다.");
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
        }

        StartCoroutine(StartTypingRoutine());
        Debug.Log("✏️ StartTyping() 실행됨");
    }

    private IEnumerator StartTypingRoutine()
    {
        yield return null;

        isTyping = true;
        targetText.text = "";

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        onTypingComplete?.Invoke();

        Debug.Log("[TypingEffectManager] ✅ 타이핑 완료 → 패널 전환 시작");

        // ✅ 타이핑 완료 후 전환 실행
        StartCoroutine(FadeToRecodingPanel());
    }

    /// <summary>
    /// 🔄 패널 전환: 타이핑 패널 닫고 RecodingPanel 열기
    /// </summary>
    private IEnumerator FadeToRecodingPanel()
    {
        if (currentPanel == null || recodingPanel == null)
        {
            Debug.LogError("[TypingEffectManager] ❌ CanvasGroup 참조가 누락되었습니다.");
            yield break;
        }

        // RecodingPanel 준비
        recodingPanel.gameObject.SetActive(true);
        recodingPanel.alpha = 0f;

        float duration = 1.2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            currentPanel.alpha = Mathf.Lerp(1f, 0f, a);
            recodingPanel.alpha = Mathf.Lerp(0f, 1f, a);

            yield return null;
        }

        // 상태 정리
        currentPanel.alpha = 0f;
        currentPanel.gameObject.SetActive(false);
        recodingPanel.alpha = 1f;

        Debug.Log("🎬 RecoTextPanel → RecodingPanel 전환 완료");
    }
}
