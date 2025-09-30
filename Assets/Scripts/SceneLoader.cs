using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬의 화면/소리 페이드 인/아웃 효과와 씬 로드를 관리하는 통합 싱글톤 스크립트입니다.
/// 이 버전은 Inspector 연결이 실패할 경우 GetComponent로 강제 확인합니다.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // ── 화면 페이드 설정 ──
    [Header("Fade Settings (Screen)")]
    [Tooltip("페이드 효과를 줄 Canvas Group (Inspector 연결 필수)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("화면을 가리는 Image 컴포넌트 (검은색 강제 설정용, Inspector 연결 필수)")]
    [SerializeField] private Image faderImage;

    [Tooltip("페이드 인/아웃에 걸리는 기본 시간 (초)")]
    [SerializeField] private float fadeDuration = 1.0f;
    [Tooltip("씬 시작 시 자동으로 페이드 인 할지 여부")]
    [SerializeField] private bool fadeOnStart = true;

    // ── 소리 페이드 설정 ──
    [Header("Fade Settings (Audio)")]
    [Tooltip("페이드 인/아웃 할 Audio Source 컴포넌트 (연결 선택적)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("오디오 페이드의 목표 볼륨 (씬이 완전히 보일 때의 볼륨)")]
    [SerializeField] private float targetVolume = 0.5f;

    // ── 씬 로더 설정 ──
    [Header("Scene Defaults")]
    [SerializeField] private string homeSceneName = "main";
    private const LoadSceneMode loadMode = LoadSceneMode.Single;

    private static SceneLoader instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFader();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFader()
    {
        // 1. 강제 연결 시도 (Inspector 연결이 이미 되어 있어도 다시 시도)
        if (canvasGroup == null)
        {
            // Inspector 연결 실패 시, 오브젝트에서 다시 찾습니다.
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (faderImage == null)
        {
            // Inspector 연결 실패 시, 오브젝트에서 다시 찾습니다.
            faderImage = GetComponent<Image>();
        }

        // 2. 최종 확인
        if (canvasGroup == null || faderImage == null)
        {
            // 여전히 null인 경우에만 오류 출력
            Debug.LogError("[SceneLoader] 치명적인 오류: Canvas Group 또는 Image 컴포넌트가 연결되지 않았습니다! (Inspector 수동 연결 및 컴포넌트 존재 확인 필수)");
            return;
        }

        // 3. 검은색 페이드 배경 강제
        if (faderImage.color != Color.black)
        {
            faderImage.color = Color.black;
        }

        // 4. AudioSource 초기 볼륨 설정
        if (audioSource != null && fadeOnStart && audioSource.playOnAwake)
        {
            audioSource.volume = 0f;
        }

        // 5. 씬 로드 완료 이벤트에 FadeIn 함수 등록
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 6. 씬 시작 시 검은색 (Alpha 1)에서 페이드 인을 시작합니다.
        if (fadeOnStart)
        {
            canvasGroup.alpha = 1f;
            FadeIn();
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
    }

    // ── 페이드 코루틴 및 기타 함수는 동일하게 유지 ──
    private IEnumerator PerformScreenFade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        if (endAlpha > startAlpha) canvasGroup.blocksRaycasts = true;

        while (timer < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (endAlpha < startAlpha) canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator PerformAudioFade(float startVolume, float endVolume, float duration)
    {
        if (audioSource == null) yield break;

        float timer = 0f;

        if (endVolume > startVolume && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        while (timer < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;

        if (endVolume < 0.01f)
        {
            audioSource.Stop();
        }
    }

    public void FadeIn(float duration = -1)
    {
        if (canvasGroup == null) return;
        float fadeTime = (duration > 0) ? duration : fadeDuration;

        StopAllCoroutines();

        StartCoroutine(PerformScreenFade(canvasGroup.alpha, 0f, fadeTime));

        if (audioSource != null)
        {
            StartCoroutine(PerformAudioFade(audioSource.volume, targetVolume, fadeTime));
        }
    }

    public void FadeOutAndLoad(string sceneName, float duration = -1)
    {
        if (canvasGroup == null) return;
        float fadeTime = (duration > 0) ? duration : fadeDuration;

        if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            StartCoroutine(FadeOutAndLoadCoroutine(sceneName, fadeTime));
        }
        else
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' 가 Build Settings에 없습니다.");
        }
    }

    private IEnumerator FadeOutAndLoadCoroutine(string sceneName, float duration)
    {
        StopAllCoroutines();
        Coroutine screenFade = StartCoroutine(PerformScreenFade(canvasGroup.alpha, 1f, duration));

        Coroutine audioFade = null;
        if (audioSource != null)
        {
            audioFade = StartCoroutine(PerformAudioFade(audioSource.volume, 0f, duration));
        }

        yield return screenFade;

        if (audioFade != null) yield return audioFade;

        SceneManager.LoadScene(sceneName, loadMode);
    }

    public void LoadHome() => FadeOutAndLoad(homeSceneName);

    public void LoadByName(string sceneName) => FadeOutAndLoad(sceneName);
    
    public void LoadProlog()
    {
        Debug.Log("[SceneLoader] CanStreamedLevelBeLoaded(0_prolog): " 
            + Application.CanStreamedLevelBeLoaded("0_prolog"));
        FadeOutAndLoad("0_prolog");
    }
}
