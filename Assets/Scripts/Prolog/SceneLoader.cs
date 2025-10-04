using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Fade Settings (Screen)")]
    [SerializeField] private CanvasGroup canvasGroup;   // 이 스크립트가 붙은 SceneFader에 있는 CanvasGroup
    [SerializeField] private Image faderImage;          // 같은 오브젝트의 Image (검정)
    [SerializeField] private float fadeDuration = 1.0f; // 페이드인 시간
    [SerializeField] private bool fadeOnStart = true;   // 씬 시작 시 자동 페이드인

    [Header("Fade Settings (Audio)")]
    [SerializeField] private AudioSource audioSource;   // 선택
    [SerializeField] private float targetVolume = 0.5f; // 페이드인 후 최종 볼륨

    [Header("Scene Defaults")]
    [SerializeField] private string homeSceneName = "main";
    private const LoadSceneMode loadMode = LoadSceneMode.Single;

    // ✅ 싱글톤/DDD 없음 — 씬마다 새로 생성/파괴됨
    private void Awake()
    {
        InitializeFader();
    }

    private void InitializeFader()
    {
        // 같은 오브젝트에서 자동 연결(안 되어 있으면)
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (faderImage  == null) faderImage  = GetComponent<Image>();

        if (canvasGroup == null || faderImage == null)
        {
            Debug.LogError("[SceneLoader] SceneFader에 CanvasGroup 또는 Image가 없습니다. 같은 오브젝트에 붙여주세요.", this);
            enabled = false;
            return;
        }

        // 페이드용 이미지 안전값
        faderImage.sprite = null;
        faderImage.color  = Color.black;
        faderImage.raycastTarget = true;

        // 씬 시작 상태 설정
        if (fadeOnStart)
        {
            // 시작은 검정(α=1) → 자동으로 밝아짐
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // 오디오도 서서히 키울 거면 0으로 시작
            if (audioSource != null && audioSource.playOnAwake) audioSource.volume = 0f;

            FadeIn();
        }
        else
        {
            // 페이드인 사용 안 하면 바로 보이게
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // --------- 페이드 구현 ---------
    public void FadeIn(float duration = -1f)
    {
        if (canvasGroup == null) return;

        float d = (duration > 0f) ? duration : fadeDuration;

        StopAllCoroutines();
        StartCoroutine(PerformScreenFade(canvasGroup.alpha, 0f, d));

        if (audioSource != null)
            StartCoroutine(PerformAudioFade(audioSource.volume, targetVolume, d));
    }

    private IEnumerator PerformScreenFade(float from, float to, float duration)
    {
        float t = 0f;
        // 어두운 상태에서 밝아질 때는 입력 막기 유지
        canvasGroup.blocksRaycasts = from > to;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;

        // 밝아진 뒤엔 입력 허용
        if (to <= 0f) canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator PerformAudioFade(float from, float to, float duration)
    {
        float t = 0f;
        if (to > from && audioSource != null && !audioSource.isPlaying) audioSource.Play();

        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        audioSource.volume = to;

        if (to < 0.01f && audioSource != null) audioSource.Stop();
    }

    // --------- 씬 로드 (페이드아웃 없음: 즉시 전환) ---------
    public void LoadHome() => LoadByName(homeSceneName);

    public void LoadProlog() => LoadByName("0_prolog");

    public void LoadByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' 가 Build Settings에 없습니다.");
            return;
        }
        SceneManager.LoadScene(sceneName, loadMode);
        // 새 씬에 있는 SceneLoader가 Awake에서 자동으로 페이드인 처리함
    }
}
