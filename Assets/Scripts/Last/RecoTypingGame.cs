using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RecoTypingGame : MonoBehaviour
{
    [Header("텍스트 오브젝트")]
    public TextMeshProUGUI targetText;
    public TMP_InputField playerInput;
    public TextMeshProUGUI accumulatedText;

    [Header("페이드용")]
    public CanvasGroup textPanelCanvas;    // RecoTextPanel
    public CanvasGroup recodingCanvas;     // RecodingPanel
    public Image blackOverlay;             // 검정 페이드 이미지 (화면 전체 덮기)

    [Header("타이핑 구성")]
    [TextArea(3, 10)]
    public List<string> sentenceParts = new List<string>
    {
        "네가 떠난 이후,",
        "이 방에서",
        "나는 계속",
        "너의 흔적만을",
        "재연했다."
    };

    [Header("타이밍 설정")]
    public float delayBetweenWords = 0.3f;
    public float delayBeforeFade = 1.2f;
    public float fadeDuration = 1.5f;

    private int currentIndex = 0;
    private bool waitingForInput = false;

    void Start()
    {
        // 초기 텍스트 설정
        targetText.text = "";
        accumulatedText.text = "";
        playerInput.text = "";

        playerInput.onValueChanged.AddListener(OnInputChanged);

        textPanelCanvas.alpha = 1f;
        recodingCanvas.alpha = 0f;
        recodingCanvas.gameObject.SetActive(false);
        if (blackOverlay != null)
            blackOverlay.color = new Color(0, 0, 0, 0);

        StartCoroutine(ShowNextWord());
    }

    IEnumerator ShowNextWord()
    {
        while (currentIndex < sentenceParts.Count)
        {
            string currentWord = sentenceParts[currentIndex];
            targetText.text = currentWord;
            playerInput.text = "";
            waitingForInput = true;

            yield return new WaitUntil(() => waitingForInput == false);

            // 누적 표시 (줄바꿈 반영)
            if (accumulatedText.text == "")
                accumulatedText.text = currentWord;
            else
                accumulatedText.text += "\n" + currentWord;

            targetText.text = "";
            yield return new WaitForSeconds(delayBetweenWords);
            currentIndex++;
        }

        yield return new WaitForSeconds(delayBeforeFade);
        StartCoroutine(FadeTransition());
    }

    void OnInputChanged(string input)
    {
        if (!waitingForInput) return;
        if (input.Trim() == sentenceParts[currentIndex])
        {
            waitingForInput = false;
        }
    }

    IEnumerator FadeTransition()
    {
        // 🔥 1단계: 텍스트 패널이 천천히 어두워지며 사라짐
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            textPanelCanvas.alpha = 1f - normalized; // RecoTextPanel 사라짐
            if (blackOverlay != null)
                blackOverlay.color = new Color(0, 0, 0, normalized * 0.9f); // 검정 오버레이 덮임

            yield return null;
        }

        textPanelCanvas.gameObject.SetActive(false);
        recodingCanvas.gameObject.SetActive(true);
        recodingCanvas.alpha = 0f;

        yield return new WaitForSeconds(0.3f); // 여운 타이밍

        // 🔥 2단계: RecodingPanel이 서서히 나타나며 오버레이가 사라짐
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            recodingCanvas.alpha = normalized;
            if (blackOverlay != null)
                blackOverlay.color = new Color(0, 0, 0, 0.9f - normalized * 0.9f); // 점점 밝아짐

            yield return null;
        }

        if (blackOverlay != null)
            blackOverlay.color = new Color(0, 0, 0, 0); // 완전 투명
    }
}
