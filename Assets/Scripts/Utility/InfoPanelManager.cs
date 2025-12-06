using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class InfoPanelManager : MonoBehaviour
{
    [Header("메인 Info 패널")]
    public CanvasGroup infoSelectPanel;   // InforSelectPanel
    public Button infoButton;             // InfoButton (Information)

    [Header("등장인물 패널")]
    public CanvasGroup characterPanel;    // '등장인물 Panel'
    public Button characterButton;        // '등장인물' 버튼
    public Button characterCloseButton;   // '등장인물 Panel/CloseBtn'

    [Header("게임 소개 패널")]
    public CanvasGroup gameIntroPanel;    // '게임 소개 Panel'
    public Button gameIntroButton;        // '게임 소개' 버튼
    public Button gameIntroCloseButton;   // '게임 소개 Panel/CloseBtn'

    [Header("게임 제작자 패널")]
    public CanvasGroup creatorPanel;      // '게임 제작자 Panel'
    public Button creatorButton;          // '게임 제작자' 버튼
    public Button creatorCloseButton;     // '게임 제작자 Panel/CloseBtn'

    [Header("애니메이션 설정")]
    public float fadeDuration = 0.5f;

    private bool infoOpen = false;
    private bool infoAnimating = false;

    void Start()
    {
        // 모든 패널 기본값 = 숨김
        InitPanel(infoSelectPanel);
        InitPanel(characterPanel);
        InitPanel(gameIntroPanel);
        InitPanel(creatorPanel);

        // Info 버튼 토글
        if (infoButton != null)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(OnInfoButtonClicked);
        }

        // 등장인물
        if (characterButton != null)
        {
            characterButton.onClick.RemoveAllListeners();
            characterButton.onClick.AddListener(() => ShowPanel(characterPanel));
        }
        if (characterCloseButton != null)
        {
            characterCloseButton.onClick.RemoveAllListeners();
            characterCloseButton.onClick.AddListener(() => HidePanel(characterPanel));
        }

        // 게임 소개
        if (gameIntroButton != null)
        {
            gameIntroButton.onClick.RemoveAllListeners();
            gameIntroButton.onClick.AddListener(() => ShowPanel(gameIntroPanel));
        }
        if (gameIntroCloseButton != null)
        {
            gameIntroCloseButton.onClick.RemoveAllListeners();
            gameIntroCloseButton.onClick.AddListener(() => HidePanel(gameIntroPanel));
        }

        // 게임 제작자
        if (creatorButton != null)
        {
            creatorButton.onClick.RemoveAllListeners();
            creatorButton.onClick.AddListener(() => ShowPanel(creatorPanel));
        }
        if (creatorCloseButton != null)
        {
            creatorCloseButton.onClick.RemoveAllListeners();
            creatorCloseButton.onClick.AddListener(() => HidePanel(creatorPanel));
        }
    }

    // ---------------- 메인 Info 패널 토글 ----------------

    public void OnInfoButtonClicked()
    {
        if (infoAnimating) return;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (!infoOpen)
            StartCoroutine(FadeInfoSelect(true));
        else
            StartCoroutine(FadeInfoSelect(false));
    }

    private IEnumerator FadeInfoSelect(bool show)
    {
        infoAnimating = true;

        if (show)
            infoSelectPanel.gameObject.SetActive(true);

        float start = infoSelectPanel.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            infoSelectPanel.alpha = Mathf.Lerp(start, end, lerp);
            yield return null;
        }

        infoSelectPanel.alpha = end;
        infoSelectPanel.interactable = show;
        infoSelectPanel.blocksRaycasts = show;

        if (!show)
            infoSelectPanel.gameObject.SetActive(false);

        infoOpen = show;
        infoAnimating = false;
    }

    // ---------------- 서브 패널 공통 함수 ----------------

    private void InitPanel(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    private void ShowPanel(CanvasGroup panel)
    {
        if (panel == null) return;
        StartCoroutine(FadePanel(panel, true));
    }

    private void HidePanel(CanvasGroup panel)
    {
        if (panel == null) return;
        StartCoroutine(FadePanel(panel, false));
    }

    private IEnumerator FadePanel(CanvasGroup panel, bool show)
    {
        if (show)
            panel.gameObject.SetActive(true);

        float start = panel.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            panel.alpha = Mathf.Lerp(start, end, lerp);
            yield return null;
        }

        panel.alpha = end;
        panel.interactable = show;
        panel.blocksRaycasts = show;

        if (!show)
            panel.gameObject.SetActive(false);
    }
}
