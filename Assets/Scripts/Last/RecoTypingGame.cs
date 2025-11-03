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
    public CanvasGroup textPanelCanvas;

    [Header("게임 완료")]
    public GameObject scrollUnlockButton;

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
        targetText.text = "";
        accumulatedText.text = "";
        playerInput.text = "";

        // ✅ OnSubmit은 반드시 하나만 연결
        playerInput.onSubmit.AddListener(OnInputSubmit);

        // ✅ onValueChanged는 실시간 입력 감지용
        playerInput.onValueChanged.AddListener(OnInputChanged);

        textPanelCanvas.alpha = 1f;

        if (scrollUnlockButton != null)
            scrollUnlockButton.SetActive(false);

        StartCoroutine(ShowNextWord());
    }

    void Update()
    {
        // ✅ 한글 입력기에서 엔터 인식 안 되는 문제 보완
        if (waitingForInput && Input.GetKeyDown(KeyCode.Return))
        {
            OnInputSubmit(playerInput.text);
        }
    }

    IEnumerator ShowNextWord()
    {
        while (currentIndex < sentenceParts.Count)
        {
            string currentWord = sentenceParts[currentIndex];
            targetText.text = currentWord;
            playerInput.text = "";
            waitingForInput = true;
            
            // ⏳ 입력 대기 (OnInputSubmit에서 waitingForInput이 false가 될 때까지)
            yield return new WaitUntil(() => waitingForInput == false);

            // ✅ 누적 텍스트 추가
            // (참고: currentWord 대신 sentenceParts[currentIndex]를 사용해야
            // OnInputSubmit이 먼저 실행되어 currentIndex가 증가해도 안전합니다.)
            if (accumulatedText.text == "")
                accumulatedText.text = sentenceParts[currentIndex];
            else
                accumulatedText.text += "\n" + sentenceParts[currentIndex];

            targetText.text = "";
            yield return new WaitForSeconds(delayBetweenWords);
            currentIndex++; // 다음 단어로 인덱스 증가
        }

        yield return new WaitForSeconds(delayBeforeFade);
        OnTypingGameFinished();
    }

    void OnInputChanged(string input)
    {
        if (!waitingForInput) return;

        // ▼▼▼▼▼ 수정된 부분 ▼▼▼▼▼
        // 실시간 자동 제출 로직을 제거 (주석 처리)
        /*
        if (input.Trim() == sentenceParts[currentIndex])
        {
            waitingForInput = false;
        }
        */
        // ▲▲▲▲▲ 수정된 부분 ▲▲▲▲▲
    }

    // ✅ 엔터 또는 Submit 호출 시 실행
    public void OnInputSubmit(string text)
    {
        if (!waitingForInput) return; // 이미 처리되었으면 중복 실행 방지

        string trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed)) return; // 빈 값 입력 방지

        if (trimmed == sentenceParts[currentIndex])
        {
            Debug.Log($"✅ 정답 입력됨: {trimmed}");
            waitingForInput = false; // <-- 정답일 때만 '입력 대기' 상태 해제
        }
        else
        {
            Debug.Log($"❌ 오답 입력: {trimmed}");
            // 오답일 경우, waitingForInput을 false로 바꾸지 않아
            // 코루틴이 넘어가지 않고 계속 입력을 기다림
        }

        // 입력창 초기화 및 다시 포커스
        playerInput.text = "";
        playerInput.ActivateInputField();
    }

    void OnTypingGameFinished()
    {
        Debug.Log("✅ 타이핑 게임 완료! 페이드 아웃 시작.");

        playerInput.gameObject.SetActive(false);
        targetText.text = "";

        StartCoroutine(FadeOutAndFinish());
    }

    IEnumerator FadeOutAndFinish()
    {
        float t = 0f;
        float startAlpha = textPanelCanvas.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;
            textPanelCanvas.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        textPanelCanvas.alpha = 0f;
        textPanelCanvas.gameObject.SetActive(false);

        Debug.Log("🌿 페이드 아웃 완료. 스크롤락 해제 버튼 활성화.");

        if (scrollUnlockButton != null)
            scrollUnlockButton.SetActive(true);
    }
}