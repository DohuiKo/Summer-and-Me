using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
#if UNITY_2023_1_OR_NEWER
using UnityEngine.InputSystem; // 새 Input System
#endif

public class LaundryGameManager : MonoBehaviour
{
    public static LaundryGameManager Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("클리어 UI")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private Button nextSceneButton;
    [SerializeField] private TextMeshProUGUI clearMessageText;

    [Header("클리어 연출")]
    [Tooltip("마지막 빨래를 다 접은 뒤, 클리어 패널(Next 버튼)이 뜨기 전까지 잠깐 멈춰있는 시간(sec)")]
    [SerializeField] private float delayBeforeClearPanel = 2.0f;

    [Header("게임 오브젝트 참조")]
    [SerializeField] private FoldingArea foldingArea;

    [Header("게임 설정")]
    [SerializeField] private int targetScore = 3;
    [SerializeField] private string nextSceneName = "NextScene";

    [Header("파티클 효과 (선택)")]
    [SerializeField] private ParticleSystem celebrationEffect;

    private int score = 0;
    private bool isGameCleared = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (clearPanel != null) clearPanel.SetActive(false);
        if (nextSceneButton != null) nextSceneButton.onClick.AddListener(LoadNextScene);

        UpdateScoreUI();
        UpdateInstructionText();
    }

    void Update()
    {
        if (isGameCleared) return;
        if (CutPopupManager.IsShowing) return;

        // ── 입력 체크 (신·구 병행) ───────────────────────────────
        bool spacePressed = false;
#if UNITY_2023_1_OR_NEWER
        if (Keyboard.current != null)
            spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
        if (!spacePressed)
            spacePressed = Input.GetKeyDown(KeyCode.Space);
        // ─────────────────────────────────────────────────────────

        if (!spacePressed) return;

        bool hasArea = foldingArea != null;
        bool hasItem = hasArea && foldingArea.GetCurrentItem() != null;
        Debug.Log($"[Fold] Space pressed | hasArea={hasArea} hasItem={hasItem}");

        if (!hasItem) return;

        var currentItem = foldingArea.GetCurrentItem();
        Debug.Log($"[Fold] Try fold | isFolding={currentItem.IsFolding()} cur={currentItem.GetCurrentState()} max={currentItem.GetMaxState()}");
        if (!currentItem.IsFolding())
            currentItem.FoldNext();
    }

    public void OnLaundryCompleted()
    {
        score++;
        UpdateScoreUI();
        UpdateInstructionText();
        if (celebrationEffect != null) celebrationEffect.Play();

        if (score >= targetScore)
            StartCoroutine(ShowClearScreen());
    }

    public void ShowFailCutForScore()
    {
        var cutPopup = CutPopupManager.GetOrFind();
        if (cutPopup == null) return;
        if (score < 1) return;
        int cutIndex = Mathf.Clamp(score, 1, 3);
        cutPopup.ShowCut(cutIndex);
    }

    private IEnumerator ShowClearScreen()
    {
        isGameCleared = true;

#if UNITY_2023_1_OR_NEWER
        var piles = Object.FindObjectsByType<LaundryPile>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var piles = FindObjectsOfType<LaundryPile>();
#endif
        foreach (var pile in piles)
            if (pile != null) pile.gameObject.SetActive(false);

        // 🔹 마지막 빨래 다 접고 난 뒤, 클리어 패널 뜨기 전까지 잠깐 홀딩
        if (delayBeforeClearPanel > 0f)
            yield return new WaitForSeconds(delayBeforeClearPanel);

        if (CutPopupManager.IsShowing)
            yield return new WaitUntil(() => !CutPopupManager.IsShowing);

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
            if (clearMessageText != null)
                clearMessageText.text = $"축하합니다!\n{targetScore}개의 빨래를 모두 접었어요! 🎉";
        }
    }

    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        int idx = SceneManager.GetActiveScene().buildIndex;
        if (SceneManager.sceneCountInBuildSettings > idx + 1)
            SceneManager.LoadScene(idx + 1);
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"접은 빨래: {score}/{targetScore}";
    }

    public void UpdateInstructionText()
    {
        if (instructionText == null || isGameCleared) return;

        if (foldingArea != null && foldingArea.GetCurrentItem() != null)
        {
            var item = foldingArea.GetCurrentItem();
            int remaining = Mathf.Max(0, item.GetMaxState() - item.GetCurrentState());
            instructionText.text = $"스페이스바를 눌러 접으세요! ({remaining}단계 남음)";
        }
        else
        {
            int remainingLaundry = Mathf.Max(0, targetScore - score);
            instructionText.text = (remainingLaundry > 0)
                ? "옷더미를 클릭해서 옷을 꺼내세요!"
                : "모든 옷들을 정리했어요.!";
        }
    }

    public bool IsGameCleared() => isGameCleared;
}
