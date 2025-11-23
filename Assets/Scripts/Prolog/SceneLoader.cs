using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Fade Settings (Screen)")]
    [SerializeField] private CanvasGroup canvasGroup;   // 🔥 필수: 투명도 조절용
    [SerializeField] private Image faderImage;          // 🔹 옵션: 없어도 동작함 (검은 배경용)
    [SerializeField] private float fadeDuration = 1.0f; // 페이드인 시간
    [SerializeField] private bool fadeOnStart = true;   // 씬 시작 시 자동 페이드인

    [Header("Fade Settings (Audio)")]
    [SerializeField] private AudioSource audioSource;   // 🔹 옵션: 배경음악 페이드용
    [SerializeField] private float targetVolume = 0.5f; // 페이드인 후 최종 볼륨

    [Header("Scene Defaults")]
    [SerializeField] private string homeSceneName = "main";
    private const LoadSceneMode loadMode = LoadSceneMode.Single;

    private void Awake()
    {
        InitializeFader();
    }

    private void InitializeFader()
    {
        // 1. 컴포넌트 자동 찾기 (연결 안 되어 있을 경우)
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (faderImage == null) faderImage = GetComponent<Image>();

        // 2. 🔥 [필수 체크] CanvasGroup은 페이드 기능의 핵심이라 없으면 안 됨
        if (canvasGroup == null)
        {
            Debug.LogError("[SceneLoader] 오류: CanvasGroup 컴포넌트가 없습니다! 페이드 효과를 줄 수 없습니다.", this);
            enabled = false; // 스크립트 끄기
            return;
        }

        // 3. 🔹 [옵션 체크] 이미지는 있으면 세팅하고, 없으면 그냥 넘어감 (버그 방지)
        if (faderImage != null)
        {
            faderImage.sprite = null;       // 이미지 소스 제거 (단색 사용)
            faderImage.color = Color.black; // 검은색으로 설정
            faderImage.raycastTarget = true; // 클릭 방지용
        }
        else
        {
            // 이미지가 없어도 괜찮음. 경고 로그도 띄우지 않음.
            // 그냥 투명도 조절만 수행하게 됨.
        }

        // 4. 초기 상태 설정
        if (fadeOnStart)
        {
            // 시작할 때 화면을 가림 (Alpha 1)
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // 오디오도 있으면 볼륨 0부터 시작
            if (audioSource != null && audioSource.playOnAwake) 
                audioSource.volume = 0f;

            FadeIn(); // 밝아지기 시작
        }
        else
        {
            // 페이드인 안 쓰면 바로 보이게 (Alpha 0)
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // --------- 페이드 로직 ---------
    public void FadeIn(float duration = -1f)
    {
        if (canvasGroup == null) return; // 방어 코드

        float d = (duration > 0f) ? duration : fadeDuration;

        StopAllCoroutines();
        StartCoroutine(PerformScreenFade(canvasGroup.alpha, 0f, d));

        // 오디오 소스가 있을 때만 실행
        if (audioSource != null)
            StartCoroutine(PerformAudioFade(audioSource.volume, targetVolume, d));
    }

    private IEnumerator PerformScreenFade(float from, float to, float duration)
    {
        float t = 0f;
        // 어두워져 있을 때(from > to)는 클릭 막기
        canvasGroup.blocksRaycasts = (from > to);

        while (t < duration)
        {
            t += Time.deltaTime;
            // CanvasGroup만 있으면 되므로 이미지 유무 상관없음
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;

        // 다 밝아지면(0) 클릭 허용
        if (to <= 0f) canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator PerformAudioFade(float from, float to, float duration)
    {
        // 오디오가 없으면 코루틴 즉시 종료 (버그 방지)
        if (audioSource == null) yield break;

        float t = 0f;
        if (to > from && !audioSource.isPlaying) audioSource.Play();

        while (t < duration)
        {
            t += Time.deltaTime;
            if (audioSource != null) // 중간에 파괴되었을 경우 대비
                audioSource.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = to;
            if (to < 0.01f) audioSource.Stop();
        }
    }

    // --------- 씬 이동 기능 ---------
    public void LoadHome() => LoadByName(homeSceneName);

    public void LoadProlog() => LoadByName("0_prolog");

    public void LoadByName(string sceneName)
    {
        // 씬 이름 유효성 검사
        if (string.IsNullOrEmpty(sceneName)) return;

        // Build Settings에 등록된 씬인지 확인 (에러 방지)
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, loadMode);
        }
        else
        {
            Debug.LogError($"[SceneLoader] '{sceneName}' 씬을 찾을 수 없습니다. Build Settings를 확인하세요.");
        }
    }
}