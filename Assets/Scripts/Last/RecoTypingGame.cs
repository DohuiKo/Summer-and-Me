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
    public float delayBeforeFade = 1.2f;  // ❗ 이름 변경 (delayBeforeComplete -> delayBeforeFade)
    public float fadeDuration = 1.5f;   // ❗ 페이드 시간을 위한 변수 추가

    private int currentIndex = 0;
    private bool waitingForInput = false;

    void Start()
    {
        targetText.text = "";
        accumulatedText.text = "";
        playerInput.text = "";

        playerInput.onValueChanged.AddListener(OnInputChanged);

        textPanelCanvas.alpha = 1f;

        if (scrollUnlockButton != null)
            scrollUnlockButton.SetActive(false);

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

            if (accumulatedText.text == "")
                accumulatedText.text = currentWord;
            else
                accumulatedText.text += "\n" + currentWord;

            targetText.text = "";
            yield return new WaitForSeconds(delayBetweenWords);
            currentIndex++;
        }

        // ❗ 타이핑 완료 후 페이드 전 딜레이
        yield return new WaitForSeconds(delayBeforeFade); 
        
        OnTypingGameFinished();
    }

    void OnInputChanged(string input)
    {
        if (!waitingForInput) return;
        
        if (input == sentenceParts[currentIndex]) 
        {
            waitingForInput = false;
        }
    }

    // ❗ 게임 완료 시 호출될 함수 (수정됨)
    void OnTypingGameFinished()
    {
        Debug.Log("타이핑 게임 완료! 페이드 아웃 시작.");

        // ❗ 입력창과 타겟 텍스트 즉시 비활성화
        playerInput.gameObject.SetActive(false);
        targetText.text = ""; 

        // ❗ 페이드 아웃 및 버튼 활성화 코루틴 시작
        StartCoroutine(FadeOutAndFinish());
    }

    // ❗ 페이드 아웃 후 버튼을 활성화하는 코루틴 (새로 추가)
    IEnumerator FadeOutAndFinish()
    {
        float t = 0f;
        float startAlpha = textPanelCanvas.alpha; // 현재 알파값 (1f)

        // ❗ textPanelCanvas의 알파값을 0으로 천천히 변경
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;
            textPanelCanvas.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        textPanelCanvas.alpha = 0f;
        textPanelCanvas.gameObject.SetActive(false); // ❗ 패널 자체를 비활성화

        Debug.Log("페이드 아웃 완료. 스크롤락 해제 버튼 활성화.");

        // ❗ 페이드 아웃이 모두 끝난 후 스크롤락 버튼 활성화
        if (scrollUnlockButton != null)
        {
            scrollUnlockButton.SetActive(true);
        }
    }
}