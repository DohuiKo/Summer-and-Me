using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RecodingGameManager : MonoBehaviour
{
    [Header("Waveform")]
    public RectTransform waveformWrapper;
    public GameObject barPrefab;
    public float scrollSpeed = 80f;

    [Header("Waveform Auto Fit")]
    [Tooltip("파형의 가로폭을 자동 감지해서 막대 수/속도를 보정합니다.")]
    public float baseWidth = 400f;
    public float desiredBarSpacing = 2f;
    private int barCount;

    [Header("UI References")]
    public GameObject messageOverlay;
    public Button playButton;
    public Image playButtonIcon;
    public Button speedButton;
    public TMP_Text timerText;
    public GameObject flatLine;
    public GameObject deletePopup;
    public GameObject popupOverlay;
    public RectTransform container;

    [Header("Typing Panels")]
    [Tooltip("RecodingPanel (녹음 패널)")]
    public GameObject recodingPanel; 

    [Tooltip("RecoTextPanel (타이핑 패널)")]
    public GameObject recoTextPanel; 

    [Header("Button Sprites")]
    public Sprite playIcon;
    public Sprite pauseIcon;

    private bool isPlaying = false;
    private bool waveformShown = false;
    private bool hasPlayedOnce = false;
    private float elapsed = 0f;
    private float speed = 1f;
    private int deleteClickCount = 0;
    private Vector2 scrollPos;
    private Coroutine messageRoutine;
    private Coroutine waveformRoutine;
    private Coroutine timerRoutine;
    private LayoutElement[] barLayouts;

    void Start()
    {
        GenerateWaveform();
        playButton.onClick.AddListener(TogglePlay);
        speedButton.onClick.AddListener(ToggleSpeed);
        deletePopup.SetActive(false);
        popupOverlay.SetActive(false);
        if (messageOverlay != null)
            messageOverlay.SetActive(false);
        if (playIcon != null && playButtonIcon != null)
            playButtonIcon.sprite = playIcon;
        if (timerText != null)
            timerText.text = "00:00.00";
    }

    // ✅ 초기 파형 생성
    void GenerateWaveform()
    {
        float wrapperWidth = waveformWrapper.rect.width;
        barCount = Mathf.Max(200, Mathf.RoundToInt(wrapperWidth / desiredBarSpacing));

        barLayouts = new LayoutElement[barCount];
        for (int i = 0; i < barCount; i++)
        {
            var bar = Instantiate(barPrefab, waveformWrapper);
            bar.SetActive(true);
            barLayouts[i] = bar.GetComponent<LayoutElement>();

            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(desiredBarSpacing - 0.5f, 20f);
        }

        waveformWrapper.anchoredPosition = Vector2.zero;
    }

    // ✅ 플레이 토글
    void TogglePlay()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            playButtonIcon.sprite = pauseIcon;

            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(TimerAnimation());

            if (!hasPlayedOnce)
            {
                hasPlayedOnce = true;
                StartCoroutine(StartPlaybackSequence(true));
            }
            else
            {
                StartCoroutine(StartPlaybackSequence(false));
            }
        }
        else
        {
            playButtonIcon.sprite = playIcon;
            isPlaying = false;
            waveformShown = false;

            if (timerRoutine != null) StopCoroutine(timerRoutine);
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            if (waveformRoutine != null) StopCoroutine(waveformRoutine);
        }
    }

    // ✅ 재생 시퀀스 시작
    IEnumerator StartPlaybackSequence(bool delayed)
    {
        if (delayed)
            yield return new WaitForSeconds(3f);

        if (isPlaying)
        {
            waveformShown = true;
            flatLine.SetActive(false);

            if (messageOverlay != null)
                messageOverlay.SetActive(true);

            waveformRoutine = StartCoroutine(WaveformAnimation());
            messageRoutine = StartCoroutine(MessageSequence());
        }
    }

    // ✅ 타이머 표시
    IEnumerator TimerAnimation()
    {
        while (isPlaying)
        {
            elapsed += Time.deltaTime * speed;
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsed / 60);
                int seconds = Mathf.FloorToInt(elapsed % 60);
                int centiseconds = Mathf.FloorToInt((elapsed * 100) % 100);
                timerText.text = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            }
            yield return null;
        }
    }

    // ✅ 파형 애니메이션
    IEnumerator WaveformAnimation()
    {
        scrollPos = waveformWrapper.anchoredPosition;
        float wrapperWidth = waveformWrapper.rect.width;
        float widthRatio = wrapperWidth / baseWidth;

        float minHeight = 25f;
        float baseAmplitude = 80f;
        float waveFrequency = 4f * widthRatio * 0.8f;
        float noiseStrength = 8f;
        float adjustedScrollSpeed = scrollSpeed * widthRatio * 0.4f;

        // ✅ 감정 파형(리듬 변화) 관련 변수 복원
        float amplitudeFactor = 1f;
        float modulationTimer = 0f;
        float modulationSpeed = 0.4f;
        float targetAmplitudeFactor = Random.Range(0.6f, 1.4f);

        float[] noiseOffsets = new float[barLayouts.Length];
        for (int n = 0; n < noiseOffsets.Length; n++)
            noiseOffsets[n] = Random.Range(0f, 10f);

        while (waveformShown)
        {
            // ❗감정 파형 진폭의 자연스러운 변화 (서서히 커졌다 작아짐)
            modulationTimer += Time.deltaTime * modulationSpeed;
            if (modulationTimer >= 1f)
            {
                modulationTimer = 0f;
                targetAmplitudeFactor = Random.Range(0.6f, 1.4f);
            }
            amplitudeFactor = Mathf.Lerp(amplitudeFactor, targetAmplitudeFactor, 0.05f);

            for (int j = 0; j < barLayouts.Length; j++)
            {
                if (barLayouts[j] == null) continue;
                RectTransform barRect = barLayouts[j].GetComponent<RectTransform>();

                // 기본 파형
                float baseWave = Mathf.Abs(Mathf.Sin(Time.time * waveFrequency + j * 0.15f));

                // 파형에 약간의 노이즈 추가
                float perlin = Mathf.PerlinNoise(Time.time * 0.5f, noiseOffsets[j]) * 0.5f + 0.5f;
                float noise = Random.Range(-noiseStrength, noiseStrength) * 0.5f;

                // 감정 진폭(amplitudeFactor) 반영
                float height = Mathf.Max(minHeight,
                    minHeight + ((baseWave * perlin) * baseAmplitude * amplitudeFactor) + noise);

                Vector2 size = barRect.sizeDelta;
                size.y = height;
                barRect.sizeDelta = size;
            }

            // 스크롤 이동
            scrollPos.x -= adjustedScrollSpeed * speed * Time.deltaTime;
            waveformWrapper.anchoredPosition = scrollPos;

            yield return null;
        }
    }


    // ✅ 메시지 출력 후 삭제 팝업
    IEnumerator MessageSequence()
    {
        string[] messages = {
            "괜찮고 싶어서 계속 노력했는데",
            "미안해",
            "이제는 한계야",
            "네 잘못이 아냐, 이건 내 잘못이야"
        };

        TMP_Text msgText = messageOverlay.GetComponentInChildren<TMP_Text>(true);
        msgText.text = "";
        msgText.alpha = 0f;

        for (int i = 0; i < messages.Length; i++)
        {
            msgText.text = messages[i];
            yield return StartCoroutine(FadeText(msgText, 0f, 1f, 0.4f));
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(FadeText(msgText, 1f, 0f, 0.4f));
        }

        yield return new WaitForSeconds(1f);
        ShowDeletePopup();
    }

    IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
    }

    void ShowDeletePopup()
    {
        isPlaying = false;
        waveformShown = false;
        deletePopup.SetActive(true);
        popupOverlay.SetActive(true);
    }

    // ❗ [OnDeleteClick] 수정: 흔들림/깜빡임 연출 코루틴 호출 복구
    public void OnDeleteClick()
    {
        deleteClickCount++;
        StartCoroutine(ShakeScreen()); // ❗ 화면 흔들림 코루틴 호출
        var popupText = deletePopup.GetComponentInChildren<TMP_Text>();

        if (deleteClickCount >= 3)
        {
            popupText.text = "SYSTEM ERROR";
            popupText.color = Color.red;
            Invoke(nameof(OpenRecodingPanelSmooth), 1f);
        }
        else
        {
            string[] warns = { "ACCESS DENIED", "WARNING: DO NOT DELETE" };
            // ❗ 텍스트/색상 직접 변경 대신, WarningFlash 코루틴 호출
            StartCoroutine(WarningFlash(popupText, warns[deleteClickCount - 1]));
        }
    }

    // ❗ [WarningFlash] 코루틴 추가: 텍스트를 깜빡이는 연출
    IEnumerator WarningFlash(TMP_Text text, string msg)
    {
        string original = text.text;
        text.text = msg;
        text.color = Color.red;
        yield return new WaitForSeconds(1f); // 1초간 경고 메시지 표시
        text.text = original; // 원래 텍스트로 (아마도 "DELETE")
        text.color = new Color(1f, 0.27f, 0.27f); // 원래 색상으로 (빨간 계열)
    }

    // ❗ [ShakeScreen] 코루틴 추가: 화면(컨테이너)을 흔드는 연출
    IEnumerator ShakeScreen()
    {
        Vector3 originalPos = container.localPosition;
        float duration = 0.5f;
        float magnitude = 10f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            container.localPosition = originalPos + new Vector3(x, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        container.localPosition = originalPos;
    }


    // ✅ 페이드 아웃 + RecodingPanel 페이드 인
    void OpenRecodingPanelSmooth()
    {
        StartCoroutine(OpenRecodingPanelSmoothCoroutine());
    }

    IEnumerator OpenRecodingPanelSmoothCoroutine()
    {
        if (recoTextPanel == null || recodingPanel == null)
        {
            Debug.LogError("❌ 패널 참조가 비어있습니다. 인스펙터에서 연결하세요!");
            yield break;
        }

        CanvasGroup textGroup = recodingPanel.GetComponent<CanvasGroup>();
        CanvasGroup recodingGroup = recoTextPanel.GetComponent<CanvasGroup>();

        if (textGroup == null) textGroup = recodingPanel.AddComponent<CanvasGroup>();
        if (recodingGroup == null) recodingGroup = recoTextPanel.AddComponent<CanvasGroup>();

        recoTextPanel.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float a = 1 - t;
            textGroup.alpha = a;        
            recodingGroup.alpha = t;    
            yield return null;
        }

        recodingPanel.SetActive(false);
        Debug.Log("📘 RecodingPanel → RecoTextPanel 전환 완료"); 
    }

    void ToggleSpeed()
    {
        float[] speeds = { 1f, 1.5f, 2f, 0.5f };
        int i = System.Array.IndexOf(speeds, speed);
        speed = speeds[(i + 1) % speeds.Length];
        speedButton.GetComponentInChildren<TMP_Text>().text = $"{speed}x";
    }
}