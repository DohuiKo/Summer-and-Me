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
    public float typingDelay = 0.05f;    // 글자 타이핑 속도
    public float buttonFadeDuration = 0.5f; // 버튼 페이드인 속도 (기존 fadeDuration 대체)
    public float buttonFadeDelay = 0.4f; // 타이핑 후 버튼 나올 때까지 대기 시간

    [Header("효과음 설정")]
    public AudioSource uiAudioSource;
    public AudioClip openSFX;
    public AudioClip closeSFX;
    public AudioClip confirmSFX;
    public AudioClip cancelSFX;

    private bool isShowing = false;
    private string fullMessage = "메인으로 돌아가시겠습니까?";

    void Awake()
    {
        // 🔒 싱글톤 중복 완전 차단 (이 로직이 ESC 감지에 가장 안전함)
        if (Instance != null && Instance != this)
        {
            if (confirmPanel != null)
                Destroy(confirmPanel.gameObject);

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }

        if (confirmPanel != null)
        {
            confirmPanel.gameObject.SetActive(false);
            
            // 캔버스 설정 보장
            if (confirmPanel.GetComponent<Canvas>() == null)
            {
                var canvas = confirmPanel.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                confirmPanel.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Instance != this) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isShowing)
            {
                StartCoroutine(ShowConfirmPanel());
            }
            else
            {
                // 닫을 때는 코루틴 없이 즉시 닫기 (반응성 향상)
                HideConfirmPanelImmediate();
            }
        }
    }

    private IEnumerator ShowConfirmPanel()
    {
        if (confirmPanel == null) yield break;

        isShowing = true;

        confirmPanel.gameObject.SetActive(true);
        
        // 🔥 [수정] 페이드 없이 즉시 100% 보이게 설정
        confirmPanel.alpha = 1f; 
        
        buttonGroup.alpha = 0;
        messageText.text = ""; // 텍스트 초기화

        Time.timeScale = 0f;
        PlaySFX(openSFX);

        // 🔥 [삭제됨] 배경 페이드인 while 반복문 제거함

        // 바로 메시지 타이핑 시작
        for (int i = 0; i < fullMessage.Length; i++)
        {
            messageText.text = fullMessage.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(typingDelay);
        }

        // 버튼 등장 대기
        yield return new WaitForSecondsRealtime(buttonFadeDelay);

        // 버튼은 부드럽게 나오는 게 이쁘니까 유지
        float bt = 0f;
        while (bt < buttonFadeDuration)
        {
            bt += Time.unscaledDeltaTime;
            buttonGroup.alpha = Mathf.Lerp(0f, 1f, bt / buttonFadeDuration);
            yield return null;
        }
        buttonGroup.alpha = 1f;

        // 버튼 이벤트 설정
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
            HideConfirmPanelImmediate();
        });
    }

    // 닫을 때는 굳이 애니메이션 볼 필요 없이 즉시 닫음
    private void HideConfirmPanelImmediate()
    {
        if (confirmPanel == null) return;

        PlaySFX(closeSFX);
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