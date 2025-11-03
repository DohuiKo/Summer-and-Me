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
    public GameObject recodingPanel;
    public GameObject recoTextPanel;

    [Header("Button Sprites")]
    public Sprite playIcon;
    public Sprite pauseIcon;

    private bool isPlaying = false;
    private bool waveformShown = false;
    private float elapsed = 0f;
    private float speed = 1f;
    private int deleteClickCount = 0;
    private Vector2 scrollPos;
    private Coroutine messageRoutine;
    private Coroutine waveformRoutine;
    private Coroutine timerRoutine;
    private LayoutElement[] barLayouts;

    // ✅ 이어서 메시지 출력용
    private int messageIndex = 0;

    void Start()
    {
        GenerateWaveform();
        playButton.onClick.AddListener(TogglePlay);
        speedButton.onClick.AddListener(ToggleSpeed);
        deletePopup.SetActive(false);
        popupOverlay.SetActive(false);

        if (messageOverlay != null) messageOverlay.SetActive(false);
        if (playIcon != null && playButtonIcon != null) playButtonIcon.sprite = playIcon;
        if (timerText != null) timerText.text = "00:00.00";
    }

    // ✅ 파형 초기 생성
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

    // ✅ 재생 / 정지 토글
    void TogglePlay()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            playButtonIcon.sprite = pauseIcon;

            // 🔊 루프 사운드 재생
            Chap5SoundManager.Instance?.PlayRecordingLoop();

            // 타이머 재시작
            if (timerRoutine != null)
            {
                StopCoroutine(timerRoutine);
                timerRoutine = null;
            }
            timerRoutine = StartCoroutine(TimerAnimation());

            // 파형이 꺼져있다면 다시 돌리기
            if (waveformRoutine == null)
                waveformRoutine = StartCoroutine(WaveformAnimation());

            // 메시지 코루틴 (이어재생 가능)
            if (messageRoutine == null)
                messageRoutine = StartCoroutine(MessageSequence());
        }
        else
        {
            playButtonIcon.sprite = playIcon;
            Chap5SoundManager.Instance?.StopRecordingLoop();

            isPlaying = false;
            waveformShown = false;

            // 루틴 중단 (다음 재생 시 재개 가능)
            if (timerRoutine != null)
            {
                StopCoroutine(timerRoutine);
                timerRoutine = null;
            }

            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
                messageRoutine = null;
            }
        }
    }

    // ✅ 타이머
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
        float adjustedScrollSpeed = scrollSpeed * widthRatio * 0.25f;

        float amplitudeFactor = 1f;
        float modulationTimer = 0f;
        float modulationSpeed = 0.4f;
        float targetAmplitudeFactor = Random.Range(0.6f, 1.2f);

        float[] noiseOffsets = new float[barLayouts.Length];
        for (int n = 0; n < noiseOffsets.Length; n++)
            noiseOffsets[n] = Random.Range(0f, 10f);

        while (true)
        {
            if (!isPlaying) { yield return null; continue; }

            modulationTimer += Time.deltaTime * modulationSpeed;
            if (modulationTimer >= 1f)
            {
                modulationTimer = 0f;
                targetAmplitudeFactor = Random.Range(0.6f, 1.2f);
            }
            amplitudeFactor = Mathf.Lerp(amplitudeFactor, targetAmplitudeFactor, 0.05f);

            float smoothFactor = Mathf.PingPong(Time.time * 0.2f, 0.8f) + 0.6f;

            for (int j = 0; j < barLayouts.Length; j++)
            {
                if (barLayouts[j] == null) continue;
                RectTransform barRect = barLayouts[j].GetComponent<RectTransform>();

                float baseWave = Mathf.Abs(Mathf.Sin(Time.time * waveFrequency + j * 0.15f));
                float perlin = Mathf.PerlinNoise(Time.time * 0.5f, noiseOffsets[j]) * 0.5f + 0.5f;
                float height = Mathf.Max(minHeight,
                    minHeight + ((baseWave * perlin) * baseAmplitude * amplitudeFactor * smoothFactor));

                barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, height);
            }

            scrollPos.x -= adjustedScrollSpeed * speed * Time.deltaTime * 0.8f;
            waveformWrapper.anchoredPosition = scrollPos;
            yield return null;
        }
    }

    // ✅ 메시지 시퀀스 (이어재생 지원)
    IEnumerator MessageSequence()
    {
        string[] messages = {
            "괜찮고 싶어서 계속 노력했는데",
            "미안해",
            "이제는 한계야",
            "네 잘못이 아냐, 이건 내 잘못이야"
        };

        TMP_Text msgText = messageOverlay.GetComponentInChildren<TMP_Text>(true);
        messageOverlay.SetActive(true);

        for (int i = messageIndex; i < messages.Length; i++)
        {
            while (!isPlaying)
                yield return null;

            msgText.text = messages[i];
            yield return StartCoroutine(FadeText(msgText, 0f, 1f, 0.5f));

            float waitTime = 2.2f / speed;
            float elapsedWait = 0f;
            while (elapsedWait < waitTime)
            {
                if (!isPlaying) yield return null;
                elapsedWait += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(FadeText(msgText, 1f, 0f, 0.6f));
            messageIndex = i + 1;
        }

        yield return new WaitForSeconds(1.2f);
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
        Chap5SoundManager.Instance?.StopRecordingLoop();
        deletePopup.SetActive(true);
        popupOverlay.SetActive(true);
    }

    // 🔁 삭제 클릭 이벤트
    public void OnDeleteClick()
    {
        deleteClickCount++;
        StartCoroutine(ShakeScreen());
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
            StartCoroutine(WarningFlash(popupText, warns[deleteClickCount - 1]));
        }
    }

    IEnumerator WarningFlash(TMP_Text text, string msg)
    {
        string original = text.text;
        text.text = msg;
        text.color = Color.red;
        yield return new WaitForSeconds(1f);
        text.text = original;
        text.color = new Color(1f, 0.27f, 0.27f);
    }

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

    // ✅ 패널 전환
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

    // ✅ 속도 조절 버튼
    void ToggleSpeed()
    {
        float[] speeds = { 1.5f, 2f, 2.5f, 1f };
        int i = System.Array.IndexOf(speeds, speed);
        speed = speeds[(i + 1) % speeds.Length];
        speedButton.GetComponentInChildren<TMP_Text>().text = $"{speed}x";
    }
}
