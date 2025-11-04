using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChapterSelectManager : MonoBehaviour
{
    [Header("UI 참조")]
    public CanvasGroup chapterPanel;  // 챕터 선택 패널
    public Button myBoxButton;        // MY BOX 버튼
    public Button closeButton;        // 닫기 버튼
    public Button[] chapterButtons;   // 0~6 버튼 배열

    [Header("애니메이션 설정")]
    public float fadeDuration = 0.5f;

    private bool isOpen = false;

    // ✅ 씬 이름 배열
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
        if (chapterPanel != null)
        {
            chapterPanel.alpha = 0;
            chapterPanel.interactable = false;
            chapterPanel.blocksRaycasts = false;
            chapterPanel.gameObject.SetActive(false);
        }

        if (myBoxButton != null)
            myBoxButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(() => StartCoroutine(ClosePanel()));

        // 챕터 버튼 연결
        for (int i = 0; i < chapterButtons.Length; i++)
        {
            int index = i; // 지역 변수로 고정
            if (index < sceneNames.Length)
            {
                chapterButtons[i].onClick.AddListener(() => LoadChapter(index));
            }
        }
    }

    private void TogglePanel()
    {
        if (isOpen)
            StartCoroutine(ClosePanel());
        else
            StartCoroutine(OpenPanel());
    }

    private IEnumerator OpenPanel()
    {
        if (chapterPanel == null) yield break;
        isOpen = true;

        chapterPanel.gameObject.SetActive(true);
        chapterPanel.interactable = true;
        chapterPanel.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            chapterPanel.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        chapterPanel.alpha = 1f;
    }

    private IEnumerator ClosePanel()
    {
        if (chapterPanel == null) yield break;
        isOpen = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            chapterPanel.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        chapterPanel.alpha = 0f;
        chapterPanel.interactable = false;
        chapterPanel.blocksRaycasts = false;
        chapterPanel.gameObject.SetActive(false);
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
