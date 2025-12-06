using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class ChapterSelectManager : MonoBehaviour
{
    [Header("UI 참조")]
    public CanvasGroup chapterPanel;
    public Button myBoxButton;
    public Button closeButton;
    public Button[] chapterButtons;

    [Header("애니메이션 설정")]
    public float fadeDuration = 0.5f;

    private bool isOpen = false;        // 패널 열림 여부
    private bool isAnimating = false;   // 🔥 애니메이션 중인지 여부

    private readonly string[] sceneNames = new string[]
    {
        "0_prolog",
        "1_open_the_door",
        "2_the_pieces",
        "3_fragments_of_a_voice",
        "4_what_i_say",
        "5_a_last_track",
        "6_epilogue"
    };

    void Start()
    {
        chapterPanel.alpha = 0;
        chapterPanel.interactable = false;
        chapterPanel.blocksRaycasts = false;
        chapterPanel.gameObject.SetActive(false);

        myBoxButton.onClick.RemoveAllListeners();
        myBoxButton.onClick.AddListener(OnMyBoxClicked);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => StartCoroutine(ClosePanel()));
        }

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            int index = i;
            chapterButtons[i].onClick.RemoveAllListeners();
            chapterButtons[i].onClick.AddListener(() => LoadChapter(index));
        }
    }

    public void OnMyBoxClicked()
    {
        if (isAnimating) return; // 🔥 애니메이션 중이면 클릭 무시

        EventSystem.current.SetSelectedGameObject(null);

        if (!isOpen)
            StartCoroutine(OpenPanel());
        else
            StartCoroutine(ClosePanel());
    }

    private IEnumerator OpenPanel()
    {
        isAnimating = true;    // 🔥 애니메이션 시작
        isOpen = true;

        chapterPanel.gameObject.SetActive(true);
        chapterPanel.interactable = false;
        chapterPanel.blocksRaycasts = false;
        chapterPanel.alpha = 0;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            chapterPanel.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        chapterPanel.alpha = 1f;
        chapterPanel.interactable = true;
        chapterPanel.blocksRaycasts = true;

        isAnimating = false;   // 🔥 애니메이션 종료
    }

    private IEnumerator ClosePanel()
    {
        isAnimating = true;    // 🔥 애니메이션 시작
        isOpen = false;

        chapterPanel.interactable = false;
        chapterPanel.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            chapterPanel.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        chapterPanel.alpha = 0;
        chapterPanel.gameObject.SetActive(false);

        isAnimating = false;   // 🔥 애니메이션 종료
    }

    private void LoadChapter(int index)
    {
        if (index >= 0 && index < sceneNames.Length)
        {
            Debug.Log($"[ChapterSelectManager] ▶ 씬 이동: {sceneNames[index]}");
            SceneManager.LoadScene(sceneNames[index]);
        }
        else
        {
            Debug.LogWarning($"[ChapterSelectManager] ⚠️ 잘못된 챕터 인덱스 {index}");
        }
    }
}
