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
    public Image darkOverlay;
    public Transform reel1;
    public Transform reel2;
    public Image background;
    public Transform tapeDeck;

    // ★ 추가됨: Phase 텍스트 UI
    public TextMeshProUGUI phaseText;

    [Header("Next Scene UI")]
    public Button nextSceneButton;
    public CanvasGroup nextSceneCanvasGroup;
    public string nextSceneName = "5_recoding";

    [Header("Game Settings")]
    private int currentPhase = 1;
    private List<GameObject> activeNotes = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();
    private int sequenceIndex = 0;
    private int clearedNotes = 0;
    private float startTime;

    // 난이도 완화: 전체 노트 개수 감소
    private int[] noteCount = { 12, 16, 22 };

    // 난이도 완화: flicker 안정화
    private float baseFlickerSpeed = 1.2f;

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
        darkOverlay.color = new Color(0, 0, 0, 0);
        progressBar.value = 0;
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        StartPhase(1);
    }

    void StartPhase(int phase)
    {
        currentPhase = phase;
        sequenceIndex = 0;
        clearedNotes = 0;
        activeNotes.Clear();
        activeObstacles.Clear();

        // ★ 추가됨: Phase 텍스트 업데이트
        if (phaseText != null)
            phaseText.text = $"Phase {phase}";

        // 난이도 완화: 색 대비 완화
        Color bgColor =
            phase == 1 ? new Color(0.96f, 0.94f, 0.91f) :
            phase == 2 ? new Color(0.92f, 0.89f, 0.87f) :
                        new Color(0.88f, 0.85f, 0.83f);
        background.color = bgColor;

        StartCoroutine(SpawnNotes());
        StartCoroutine(FlickerNotes());
    }

    // -----------------------------
    //      노트 스폰 (난이도 완화)
    // -----------------------------
    IEnumerator SpawnNotes()
    {
        yield return new WaitForSeconds(2.5f);

        int count = noteCount[currentPhase - 1];
        bool requireSequence = currentPhase >= 2;

        RectTransform containerRect = noteContainer.GetComponent<RectTransform>();
        float xRange = containerRect.rect.width / 2f - 100f;
        float yRange = containerRect.rect.height / 2f - 100f;

        List<Vector2> placedPositions = new List<Vector2>();

        float minDistance = 180f;
        RectTransform deckRect = tapeDeck.GetComponent<RectTransform>();

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos;
            int attempts = 0;
            do
            {
                spawnPos = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));
                attempts++;
            }
            while ((IsTooClose(spawnPos, placedPositions, minDistance) || IsInsideTapeDeck(spawnPos, deckRect))
                && attempts < 80);

            placedPositions.Add(spawnPos);

            GameObject noteObj = Instantiate(notePrefab, noteContainer);
            RectTransform rect = noteObj.GetComponent<RectTransform>();
            rect.anchoredPosition = spawnPos;

            rect.localScale = Vector3.zero;
            StartCoroutine(PopIn(rect));

            TuneNote note = noteObj.GetComponent<TuneNote>();
            note.Initialize(requireSequence ? i : -1, false);
            activeNotes.Add(noteObj);

            yield return new WaitForSeconds(0.35f);
        }

        if (currentPhase == 3)
        {
            yield return new WaitForSeconds(1f);

            int obstacleCount = 5;
            for (int i = 0; i < obstacleCount; i++)
            {
                GameObject obstacleObj = Instantiate(obstaclePrefab, noteContainer);
                RectTransform rect = obstacleObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));

                TuneNote obstacle = obstacleObj.GetComponent<TuneNote>();
                obstacle.Initialize(-1, true);
                activeObstacles.Add(obstacleObj);
                StartCoroutine(PulseObstacle(obstacleObj));

                yield return new WaitForSeconds(0.12f);
            }
        }
    }

    IEnumerator PopIn(RectTransform rect)
    {
        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            rect.localScale = new Vector3(k, k, 1);
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    bool IsTooClose(Vector2 newPos, List<Vector2> existing, float minDist)
    {
        foreach (var pos in existing)
            if (Vector2.Distance(newPos, pos) < minDist)
                return true;
        return false;
    }

    bool IsInsideTapeDeck(Vector2 pos, RectTransform deckRect)
    {
        Vector2 deckPos = deckRect.anchoredPosition;
        Vector2 deckSize = deckRect.rect.size;
        Rect deckBounds = new Rect(deckPos.x - deckSize.x / 2f, deckPos.y - deckSize.y / 2f, deckSize.x, deckSize.y);
        return deckBounds.Contains(pos);
    }

    // -----------------------------
    //        노트 클릭 처리
    // -----------------------------
    public bool HandleNoteClickAndReturnResult(TuneNote note)
    {
        if (note == null) return false;

        bool isSequence = currentPhase >= 2;

        if (!isSequence)
        {
            StartCoroutine(ProcessCorrectNote(note));
            return true;
        }

        if (note.noteIndex == sequenceIndex)
        {
            sequenceIndex++;
            StartCoroutine(ProcessCorrectNote(note));
            return true;
        }
        else
        {
            StartCoroutine(ProcessWrongNoteVisual(note));
            return false;
        }
    }

    IEnumerator ProcessCorrectNote(TuneNote note)
    {
        if (note == null) yield break;

        TuneSoundManager.Instance.PlayBrokenSound();

        bool fakeFail = Random.value < 0.20f;
        if (fakeFail)
        {
            StartCoroutine(ScreenShake(0.25f, 8f));
            yield return new WaitForSeconds(0.08f);
        }

        clearedNotes++;
        UpdateProgress();
        activeNotes.Remove(note.gameObject);
        CheckPhaseComplete();
    }

    IEnumerator ProcessWrongNoteVisual(TuneNote note)
    {
        if (note == null) yield break;

        TuneSoundManager.Instance.PlayErrorSound();
        StartCoroutine(ShakeNote(note));
        StartCoroutine(ScreenShake(0.20f, 3.5f));
        yield break;
    }

    IEnumerator ShakeNote(TuneNote note)
    {
        RectTransform rect = note.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 originalPos = rect.localPosition;
        float elapsed = 0;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.Sin(elapsed * 65f) * 5.5f;
            rect.localPosition = originalPos + new Vector3(offsetX, 0, 0);
            yield return null;
        }

        rect.localPosition = originalPos;
    }

    IEnumerator ScreenShake(float duration, float intensity)
    {
        Vector3 originalPos = noteContainer.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            noteContainer.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        noteContainer.position = originalPos;
    }

    IEnumerator PulseObstacle(GameObject obstacle)
    {
        if (obstacle == null) yield break;
        CanvasGroup cg = obstacle.GetComponent<CanvasGroup>() ?? obstacle.AddComponent<CanvasGroup>();

        while (obstacle != null)
        {
            float elapsed = 0;
            while (elapsed < 1.0f && obstacle != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0.35f, 1f, elapsed / 1.0f);
                yield return null;
            }

            elapsed = 0;
            while (elapsed < 1.0f && obstacle != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0.35f, elapsed / 1.0f);
                yield return null;
            }
        }
    }

    // -----------------------------
    //        숫자 깜빡임 (완화)
    // -----------------------------
    IEnumerator FlickerNotes()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.45f, baseFlickerSpeed + 0.6f));

            foreach (var noteObj in activeNotes)
            {
                if (noteObj == null) continue;
                var txt = noteObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt == null) continue;

                txt.alpha = (Random.value < 0.8f) ? 1f : 0f;
            }
        }
    }

    // -----------------------------
    //        진행도 / 클리어 처리
    // -----------------------------
    void UpdateProgress()
        => progressBar.value = (float)clearedNotes / noteCount[currentPhase - 1];

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
            StartPhase(currentPhase + 1);
        else
            EndGame();
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
        StartCoroutine(FinalBreakEffect());
    }

    IEnumerator FinalBreakEffect()
    {
        float slowdown = 1.5f;
        float elapsed = 0f;
        while (elapsed < slowdown)
        {
            elapsed += Time.deltaTime;
            float speed = (1f - elapsed / slowdown);
            reel1.Rotate(0, 0, 360 * Time.deltaTime * speed);
            reel2.Rotate(0, 0, -360 * Time.deltaTime * speed);
            yield return null;
        }

        TuneSoundManager.Instance.PlayErrorSound();
        yield return StartCoroutine(ScreenShake(0.3f, 7f));

        StartCoroutine(ShowNextSceneButton());
    }

    IEnumerator ShowNextSceneButton()
    {
        yield return new WaitForSeconds(2f);

        if (nextSceneButton != null)
        {
            nextSceneButton.gameObject.SetActive(true);
            if (nextSceneCanvasGroup != null)
            {
                nextSceneCanvasGroup.alpha = 0;
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

    public void OnObstacleClicked()
    {
        TuneSoundManager.Instance.PlayErrorSound();
        StartCoroutine(ScreenShake(0.25f, 6f));
    }
}
