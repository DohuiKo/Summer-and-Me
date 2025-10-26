using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class RecodingGameManager : MonoBehaviour
{
    [Header("Waveform")]
    public RectTransform waveformWrapper;
    public GameObject barPrefab;
    public float scrollSpeed = 80f;

    [Header("Waveform Auto Fit")]
    [Tooltip("파형의 가로폭을 자동 감지해서 막대 수/속도를 보정합니다.")]
    public float baseWidth = 400f;       // 기준 폭
    public float desiredBarSpacing = 2f; // 막대 간격
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
    public GameObject nextSceneBtn;
    public RectTransform container;

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
    private int waveformPhase = 0;

    void Start()
    {
        GenerateWaveform();
        playButton.onClick.AddListener(TogglePlay);
        speedButton.onClick.AddListener(ToggleSpeed);
        deletePopup.SetActive(false);
        popupOverlay.SetActive(false);
        nextSceneBtn.SetActive(false);

        if (messageOverlay != null)
            messageOverlay.SetActive(false);

        if (playIcon != null && playButtonIcon != null)
            playButtonIcon.sprite = playIcon;

        if (timerText != null)
            timerText.text = "00:00.00";
    }

    // ✅ 폭에 따라 막대 자동 생성
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

    void TogglePlay()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            playButtonIcon.sprite = pauseIcon;

            // 타이머 즉시 시작
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(TimerAnimation());

            // 파형 및 메시지 시퀀스 시작
            if (!hasPlayedOnce)
            {
                hasPlayedOnce = true;
                StartCoroutine(StartPlaybackSequence(true)); // 최초 1회 3초 대기
            }
            else
            {
                StartCoroutine(StartPlaybackSequence(false)); // 이후 즉시 시작
            }
        }
        else
        {
            playButtonIcon.sprite = playIcon;
            isPlaying = false;
            waveformShown = false; // 🔹 파형 루프 완전 종료

            if (timerRoutine != null) StopCoroutine(timerRoutine);
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            if (waveformRoutine != null) StopCoroutine(waveformRoutine);
        }
    }

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

            waveformRoutine = StartCoroutine(WaveformAnimation()); // 🎵 파형 지속
            messageRoutine = StartCoroutine(MessageSequence());    // 💬 텍스트 출력
        }
    }

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
                timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
            }
            yield return null;
        }
    }

    // 🎵 파형 애니메이션 (끊기지 않음)
    IEnumerator WaveformAnimation()
    {
        scrollPos = waveformWrapper.anchoredPosition;
        float wrapperWidth = waveformWrapper.rect.width;
        float widthRatio = wrapperWidth / baseWidth;

        float minHeight = 25f;
        float baseAmplitude = 80f;
        float waveFrequency = 4f * widthRatio * 0.8f; // 🔹 20% 더 긴 파장
        float noiseStrength = 8f;
        float adjustedScrollSpeed = scrollSpeed * widthRatio * 0.4f;

        float amplitudeFactor = 1f;
        float modulationTimer = 0f;
        float modulationSpeed = 0.3f;
        float targetAmplitudeFactor = Random.Range(0.6f, 1.4f);

        float[] noiseOffsets = new float[barLayouts.Length];
        for (int n = 0; n < noiseOffsets.Length; n++)
            noiseOffsets[n] = Random.Range(0f, 10f);

        while (waveformShown) // 🔹 끊기지 않음
        {
            float emotionAmp = baseAmplitude;
            float emotionNoise = noiseStrength;
            float emotionFreq = waveFrequency;

            switch (waveformPhase)
            {
                case 0: emotionAmp = baseAmplitude; emotionNoise = 8f; emotionFreq = 4f * 0.8f; break;
                case 1: emotionAmp = baseAmplitude * 1.3f; emotionNoise = 12f; emotionFreq = 5f * 0.8f; break;
                case 2: emotionAmp = Mathf.Lerp(baseAmplitude, baseAmplitude * 0.4f, Mathf.PingPong(Time.time * 0.3f, 1f)); emotionNoise = 5f; emotionFreq = 3.5f * 0.8f; break;
                case 3: emotionAmp = Mathf.Lerp(baseAmplitude * 0.4f, baseAmplitude * 0.1f, Mathf.PingPong(Time.time * 0.5f, 1f)); emotionNoise = 2f; emotionFreq = 2f * 0.8f; break;
            }

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

                float baseWave = Mathf.Abs(Mathf.Sin(Time.time * emotionFreq + j * 0.15f + waveformPhase));
                float perlin = Mathf.PerlinNoise(Time.time * 0.5f, noiseOffsets[j]) * 0.5f + 0.5f;
                float noise = Random.Range(-emotionNoise, emotionNoise) * 0.5f;
                float height = Mathf.Max(minHeight, minHeight + ((baseWave * perlin) * emotionAmp * amplitudeFactor) + noise);

                Vector2 size = barRect.sizeDelta;
                size.y = height;
                barRect.sizeDelta = size;
            }

            float scrollFactor = 1f;
            if (waveformPhase == 2) scrollFactor = 0.8f;
            if (waveformPhase == 3) scrollFactor = 0.5f;

            scrollPos.x -= adjustedScrollSpeed * speed * Time.deltaTime * scrollFactor;
            waveformWrapper.anchoredPosition = scrollPos;

            yield return null;
        }
    }

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

        float showDuration = 2f;
        float interval = 4f;

        for (int i = 0; i < messages.Length; i++)
        {
            waveformPhase = i;
            msgText.text = messages[i];
            yield return StartCoroutine(FadeText(msgText, 0f, 1f, 0.5f));
            yield return new WaitForSeconds(showDuration);
            yield return StartCoroutine(FadeText(msgText, 1f, 0f, 0.5f));
            msgText.text = "";
            yield return new WaitForSeconds(interval);
        }

        // 마지막 메시지 후 자동 DELETE 팝업
        if (isPlaying)
        {
            yield return new WaitForSeconds(1f);
            ShowDeletePopup();
        }
    }

    IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            text.alpha = alpha;
            yield return null;
        }
        text.alpha = to;
    }

    void ShowDeletePopup()
    {
        isPlaying = false;
        waveformShown = false; // 🔹 팝업 시점에서 완전 정지
        deletePopup.SetActive(true);
        popupOverlay.SetActive(true);
    }

    public void OnDeleteClick()
    {
        deleteClickCount++;
        StartCoroutine(ShakeScreen());
        var popupText = deletePopup.GetComponentInChildren<TMP_Text>();

        if (deleteClickCount >= 3)
        {
            popupText.text = "SYSTEM ERROR";
            popupText.color = Color.red;
            Invoke(nameof(ShowNextSceneBtn), 1f);
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

    void ShowNextSceneBtn()
    {
        deletePopup.SetActive(false);
        popupOverlay.SetActive(false);
        nextSceneBtn.SetActive(true);
    }

    public void GoToNextScene()
    {
        SceneManager.LoadScene("NextSceneName");
    }

    void ToggleSpeed()
    {
        float[] speeds = { 1f, 1.5f, 2f, 0.5f };
        int i = System.Array.IndexOf(speeds, speed);
        speed = speeds[(i + 1) % speeds.Length];
        speedButton.GetComponentInChildren<TMP_Text>().text = $"{speed}x";
    }
}
