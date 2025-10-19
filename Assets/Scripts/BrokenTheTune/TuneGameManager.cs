using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TuneGameManager : MonoBehaviour
{
    public static TuneGameManager Instance;

    [Header("Prefabs")]
    public GameObject notePrefab;
    public GameObject obstaclePrefab;

    [Header("UI References")]
    public Transform noteContainer;
    public Slider progressBar;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI messageText;
    public Image darkOverlay;
    public Transform tapeDeck;
    public Transform reel1;
    public Transform reel2;
    public Image background;

    [Header("Next Scene UI")]
    public Button nextSceneButton;
    public CanvasGroup nextSceneCanvasGroup;
    public string nextSceneName = "5_recoding";

    [Header("Game Settings")]
    private int currentPhase = 1;
    private List<GameObject> activeNotes = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<TuneNote> correctSequence = new List<TuneNote>();
    private int sequenceIndex = 0;
    private int totalNotes = 65;
    private int clearedNotes = 0;
    private int correctClickCount = 0;
    private float startTime;

    private int[] noteCount = { 15, 20, 30 };
    private string[] phaseMessages = {
        "손끝으로, 음을 지워보세요.",
        "순서대로 클릭하세요 (0→1→2...)",
        "소음은 피해가는게 좋아요"
    };

    void Awake() => Instance = this;

    void Start()
    {
        InitializeGame();
        if (nextSceneButton != null)
            nextSceneButton.gameObject.SetActive(false);
    }

    void InitializeGame()
    {
        startTime = Time.time;
        darkOverlay.gameObject.SetActive(true);
        darkOverlay.color = new Color(0, 0, 0, 0);
        progressBar.value = 0;

        ShowMessage("BROKEN THE TUNE", 3f);
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3.5f);
        StartPhase(1);
    }

    void StartPhase(int phase)
    {
        currentPhase = phase;
        sequenceIndex = 0;
        correctSequence.Clear();
        correctClickCount = 0;

        phaseText.text = $"PHASE {phase}";

        Color bgColor = phase == 1 ? new Color(0.95f, 0.93f, 0.9f) :
                        phase == 2 ? new Color(0.9f, 0.88f, 0.85f) :
                                    new Color(0.85f, 0.82f, 0.78f);
        background.color = bgColor;

        ShowMessage(phaseMessages[phase - 1], 2f);
        StartCoroutine(SpawnNotes());
    }

    // 🎵 순서대로 천천히 등장 + 겹침 방지
    IEnumerator SpawnNotes()
    {
        yield return new WaitForSeconds(2.5f);

        int count = noteCount[currentPhase - 1];
        bool requireSequence = currentPhase >= 2;

        RectTransform containerRect = noteContainer.GetComponent<RectTransform>();
        float xRange = containerRect.rect.width / 2f - 100f;
        float yRange = containerRect.rect.height / 2f - 100f;

        List<Vector2> placedPositions = new List<Vector2>();
        float minDistance = 150f;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos;
            int attempts = 0;
            do
            {
                spawnPos = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));
                attempts++;
            } while (IsTooClose(spawnPos, placedPositions, minDistance) && attempts < 50);

            placedPositions.Add(spawnPos);

            GameObject noteObj = Instantiate(notePrefab, noteContainer);
            RectTransform rect = noteObj.GetComponent<RectTransform>();
            rect.anchoredPosition = spawnPos;

            TuneNote note = noteObj.GetComponent<TuneNote>();
            note.Initialize(requireSequence ? i : -1, false);

            if (requireSequence)
                note.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            else
                note.GetComponentInChildren<TextMeshProUGUI>().text = "♪";

            if (requireSequence) correctSequence.Add(note);
            activeNotes.Add(noteObj);

            CanvasGroup cg = noteObj.GetComponent<CanvasGroup>() ?? noteObj.AddComponent<CanvasGroup>();
            StartCoroutine(FadeIn(cg));

            yield return new WaitForSeconds(0.3f);
        }

        // 장애물 (Phase 3)
        if (currentPhase == 3)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < 8; i++)
            {
                GameObject obstacleObj = Instantiate(obstaclePrefab, noteContainer);
                RectTransform rect = obstacleObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));

                TuneNote obstacle = obstacleObj.GetComponent<TuneNote>();
                obstacle.Initialize(-1, true);
                obstacle.GetComponentInChildren<TextMeshProUGUI>().text = "-";

                activeObstacles.Add(obstacleObj);
                StartCoroutine(PulseObstacle(obstacleObj));

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    bool IsTooClose(Vector2 newPos, List<Vector2> existing, float minDist)
    {
        foreach (var pos in existing)
            if (Vector2.Distance(newPos, pos) < minDist)
                return true;
        return false;
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float elapsed = 0;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, elapsed / 0.8f);
            yield return null;
        }
        cg.alpha = 1;
    }

    // ✅ 순서 체크: 틀리면 사라지지 않고 흔들림만 발생
    public void OnNoteClicked(TuneNote note)
    {
        bool isSequenceMode = currentPhase >= 2;

        if (isSequenceMode)
        {
            if (note.noteIndex == sequenceIndex)
            {
                correctClickCount++;
                ProcessCorrectNote(note);
                sequenceIndex++;
            }
            else
            {
                TuneSoundManager.Instance.PlayErrorSound();
                ShowError("순서가 틀렸다...");
                StartCoroutine(ScreenShake());
                StartCoroutine(ShakeNote(note));
            }
        }
        else
        {
            correctClickCount++;
            ProcessCorrectNote(note);
        }
    }

    IEnumerator ShakeNote(TuneNote note)
    {
        RectTransform rect = note.GetComponent<RectTransform>();
        Vector3 originalPos = rect.localPosition;
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.Sin(elapsed * 80f) * 6f;
            rect.localPosition = originalPos + new Vector3(offsetX, 0, 0);
            yield return null;
        }
        rect.localPosition = originalPos;
    }

    void ProcessCorrectNote(TuneNote note)
    {
        var img = note.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0.25f, 0.55f, 0.25f, 0.8f);

        note.GetComponentInChildren<TextMeshProUGUI>().text = "♩";
        TuneSoundManager.Instance.PlayBrokenSound();

        clearedNotes++;
        UpdateProgress();

        activeNotes.Remove(note.gameObject);
        Destroy(note.gameObject, 0.8f);
        CheckPhaseComplete();
    }

    IEnumerator PulseObstacle(GameObject obstacle)
    {
        CanvasGroup cg = obstacle.GetComponent<CanvasGroup>() ?? obstacle.AddComponent<CanvasGroup>();
        while (obstacle != null)
        {
            float elapsed = 0;
            while (elapsed < 1.2f && obstacle != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0.3f, 1f, elapsed / 1.2f);
                yield return null;
            }
            elapsed = 0;
            while (elapsed < 1.2f && obstacle != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0.3f, elapsed / 1.2f);
                yield return null;
            }
        }
    }

    void ShowError(string text)
    {
        ShowMessage(text, 1f);
        StartCoroutine(DarkFlash());
    }

    IEnumerator ScreenShake()
    {
        Vector3 originalPos = noteContainer.position;
        float duration = 0.4f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-8f, 8f);
            float y = Random.Range(-8f, 8f);
            noteContainer.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }
        noteContainer.position = originalPos;
    }

    IEnumerator DarkFlash()
    {
        float elapsed = 0;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.6f, elapsed / 0.25f));
            yield return null;
        }
        yield return new WaitForSeconds(0.15f);
        elapsed = 0;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0.6f, 0, elapsed / 0.25f));
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0);
    }

    void UpdateProgress() => progressBar.value = (float)clearedNotes / totalNotes;

    void CheckPhaseComplete()
    {
        if (activeNotes.Count == 0)
            StartCoroutine(EndPhase());
    }

    IEnumerator EndPhase()
    {
        foreach (var obstacle in activeObstacles)
            Destroy(obstacle);
        activeObstacles.Clear();

        StartCoroutine(SpinReels());
        yield return new WaitForSeconds(2f);

        if (currentPhase < 3)
        {
            ShowMessage($"PHASE {currentPhase} 완료!\n\nPHASE {currentPhase + 1} 시작", 2.5f);
            yield return new WaitForSeconds(3f);
            StartPhase(currentPhase + 1);
        }
        else EndGame();
    }

    IEnumerator SpinReels()
    {
        float duration = 2f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            reel1.Rotate(0, 0, 360 * Time.deltaTime);
            reel2.Rotate(0, 0, -360 * Time.deltaTime);
            yield return null;
        }
    }

    void EndGame()
    {
        float playTime = Time.time - startTime;
        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);

        ShowMessage($"TUNE BROKEN\n\n 플레이 타임 {minutes}:{seconds:00}\n\n 침묵이 찾아왔다", 5f);
        background.color = new Color(0.75f, 0.72f, 0.69f);
        darkOverlay.color = new Color(0, 0, 0, 0.3f);

        StartCoroutine(FinalBreakEffect());
    }

    IEnumerator FinalBreakEffect()
    {
        // 🎞️ 릴 회전 중지
        float slowdown = 1.5f;
        float elapsed = 0f;
        while (elapsed < slowdown)
        {
            elapsed += Time.deltaTime;
            reel1.Rotate(0, 0, 360 * Time.deltaTime * (1f - elapsed / slowdown));
            reel2.Rotate(0, 0, -360 * Time.deltaTime * (1f - elapsed / slowdown));
            yield return null;
        }

        // 💥 "뚝" 정지 효과 (카세트 정지음이 있다면 여기서 재생)
        TuneSoundManager.Instance.PlayErrorSound(); // 혹은 StopSound()

        // ⚡ 화면 흔들림
        yield return StartCoroutine(ScreenShake());

        // 🌀 노이즈 오버레이 연출
        StartCoroutine(FinalNoiseEffect());

        // 1초 대기 후 NextScene 버튼 등장
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShowNextSceneButton());
    }
    
        IEnumerator FinalNoiseEffect()
    {
        if (darkOverlay == null) yield break;

        float duration = 1.2f;
        float elapsed = 0;
        Color startColor = darkOverlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float noise = Random.Range(0.2f, 0.7f);
            darkOverlay.color = new Color(noise, noise, noise, Mathf.Lerp(0.3f, 0.8f, Mathf.PingPong(elapsed * 4f, 1)));
            yield return null;
        }

        // 🔚 잔잔하게 어두워지며 마무리
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            darkOverlay.color = Color.Lerp(darkOverlay.color, new Color(0, 0, 0, 0.6f), t);
            yield return null;
        }
    }


    IEnumerator ShowNextSceneButton()
    {
        yield return new WaitForSeconds(3f);

        if (nextSceneButton != null)
        {
            nextSceneButton.gameObject.SetActive(true);

            if (nextSceneCanvasGroup != null)
            {
                nextSceneCanvasGroup.alpha = 0;
                nextSceneCanvasGroup.interactable = true;

                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    nextSceneCanvasGroup.alpha = Mathf.Lerp(0, 1, t / 1f);
                    yield return null;
                }
            }

            nextSceneButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }
    }

    void ShowMessage(string text, float duration)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessageAfter(duration));
    }

    IEnumerator HideMessageAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }

    public void OnObstacleClicked()
    {
        TuneSoundManager.Instance.PlayErrorSound();
        ShowError("음악이 제대로 들리지 않아");
        StartCoroutine(ScreenShake());
    }

    // 🎯 Note 클릭 시 처리 (순서 검증 전용)
    public bool TryProcessNote(TuneNote note)
    {
        bool isSequenceMode = currentPhase >= 2;

        if (isSequenceMode)
        {
            if (note.noteIndex == sequenceIndex)
            {
                sequenceIndex++;
                ProcessCorrectNote(note);
                return true;
            }
            else
            {
                TuneSoundManager.Instance.PlayErrorSound();
                ShowError("순서가 잘못됐어.");
                StartCoroutine(ScreenShake());
                StartCoroutine(ShakeNote(note));
                return false;
            }
        }
        else
        {
            ProcessCorrectNote(note);
            return true;
        }
    }
}
