using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GlobalUIManager : MonoBehaviour
{
    public static GlobalUIManager Instance;

    [Header("ESC 확인창 UI")]
    public CanvasGroup confirmPanel;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;
    public CanvasGroup buttonGroup;

    [Header("애니메이션 설정")]
    public float fadeDuration = 0.8f;
    public float typingDelay = 0.05f;
    public float buttonFadeDelay = 0.4f;

    [Header("효과음 설정")]
    public AudioSource uiAudioSource;
    public AudioClip openSFX;   // ESC 눌러 열 때
    public AudioClip closeSFX;  // ESC 눌러 닫을 때
    public AudioClip confirmSFX; // YES 눌렀을 때
    public AudioClip cancelSFX;  // NO 눌렀을 때

    private bool isShowing = false;
    private string fullMessage = "메인으로 돌아가시겠습니까?";

    void Awake()
    {
        // ✅ 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✅ AudioSource 자동 생성 (없을 경우)
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }

        // ✅ UI 초기화
        if (confirmPanel != null)
        {
            confirmPanel.gameObject.SetActive(false);

            if (confirmPanel.GetComponent<Canvas>() == null)
            {
                var canvas = confirmPanel.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                confirmPanel.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    void OnEnable()
    {
        Time.timeScale = 1f; // 씬 전환 시 일시정지 해제
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isShowing)
                StartCoroutine(ShowConfirmPanel());
            else
                StartCoroutine(HideConfirmPanel());
        }
    }

    private IEnumerator ShowConfirmPanel()
    {
        if (confirmPanel == null) yield break;
        if (isShowing) yield break;

        isShowing = true;
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.alpha = 0;
        buttonGroup.alpha = 0;
        messageText.text = "";

        // ⏸ 게임 일시정지
        Time.timeScale = 0f;

        // 🎧 효과음 재생 (열림)
        PlaySFX(openSFX);

        // 🎞 페이드인
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            confirmPanel.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        confirmPanel.alpha = 1f;

        // ✍️ 타이핑
        for (int i = 0; i < fullMessage.Length; i++)
        {
            messageText.text = fullMessage.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(typingDelay);
        }

        // 🎨 버튼 등장
        yield return new WaitForSecondsRealtime(buttonFadeDelay);
        float bt = 0f;
        while (bt < fadeDuration)
        {
            bt += Time.unscaledDeltaTime;
            buttonGroup.alpha = Mathf.Lerp(0f, 1f, bt / fadeDuration);
            yield return null;
        }
        buttonGroup.alpha = 1f;

        // 버튼 이벤트
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            PlaySFX(confirmSFX);
            Time.timeScale = 1f;
            isShowing = false;
            confirmPanel.gameObject.SetActive(false);
            SceneManager.LoadScene("Main");
        });

        noButton.onClick.AddListener(() =>
        {
            PlaySFX(cancelSFX);
            StartCoroutine(HideConfirmPanel());
        });
    }

    private IEnumerator HideConfirmPanel()
    {
        if (confirmPanel == null) yield break;

        // 🎧 효과음 재생 (닫힘)
        PlaySFX(closeSFX);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            confirmPanel.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        confirmPanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isShowing = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }
}
