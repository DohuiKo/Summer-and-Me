using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class EndingVideoTrigger : MonoBehaviour
{
    [Header("타겟 및 트리거")]
    public RectTransform targetContent;
    public RectTransform viewport;
    public ScrollRect scrollRect;

    [Header("재생 및 UI")]
    public VideoPlayer videoPlayer;
    [Tooltip("숨길 내비게이션 UI (NextSceneBtn)")]
    public CanvasGroup navigationGroup;
    [Tooltip("팀 로고 (CanvasGroup 필요)")]
    public CanvasGroup teamLogo;
    [Tooltip("게임 로고 (CanvasGroup 필요)")]
    public CanvasGroup gameLogo;

    [Header("설정")]
    public float triggerDistance = 20f;          // 트리거 조건 거리
    public float exitStopDistance = 120f;        // 벗어남 감지 거리 (새로 추가)
    public float delayBeforeLogos = 3.0f;
    public float fadeDuration = 1.5f;
    public float logoDisplayTime = 2.5f;

    private bool hasTriggered = false;
    private bool videoPlaying = false;

    void Start()
    {
        // 1. 로고 초기화
        if (teamLogo != null)
        {
            teamLogo.alpha = 0f;
            teamLogo.gameObject.SetActive(false);
        }
        if (gameLogo != null)
        {
            gameLogo.alpha = 0f;
            gameLogo.gameObject.SetActive(false);
        }

        // 2. 스크롤 이벤트 등록
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            OnScrollChanged(scrollRect.normalizedPosition);
        }
        else
        {
            Debug.LogError("EndingVideoTrigger: 'Scroll Rect'가 할당되지 않았습니다!");
        }

        // 3. 비디오 종료 이벤트 등록
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;
    }

    // ✅ 스크롤 변화 감지 (시작 & 벗어남 감지 둘 다 포함)
    private void OnScrollChanged(Vector2 value)
    {
        if (viewport == null || targetContent == null) return;

        float viewportCenterX = viewport.position.x;
        float contentCenterX = targetContent.position.x;
        float distance = Mathf.Abs(viewportCenterX - contentCenterX);

        // 1️⃣ 아직 트리거 안 됐는데 가까워지면 재생 시작
        if (!hasTriggered && distance < triggerDistance)
        {
            hasTriggered = true;
            StartCoroutine(PlayVideoAndShowLogos());
        }

        // 2️⃣ 비디오 재생 중인데 멀어지면 자동 정지
        if (videoPlaying && distance > exitStopDistance)
        {
            StopVideoAndSound();
            Debug.Log("[EndingVideoTrigger] 🚫 페이지 이탈 감지 → 비디오 및 사운드 정지");
        }
    }

    private IEnumerator PlayVideoAndShowLogos()
    {
        // UI 숨김
        if (navigationGroup != null)
        {
            navigationGroup.alpha = 0f;
            navigationGroup.interactable = false;
            navigationGroup.blocksRaycasts = false;
        }

        // 🎬 비디오 재생
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            videoPlaying = true;
            Debug.Log("[EndingVideoTrigger] 🎬 엔딩 비디오 재생 시작");

            // 🎧 마이마이 회전 사운드 재생
            if (Chap6SoundManager.Instance != null)
            {
                Chap6SoundManager.Instance.PlayMymyWindingSFX();
                Debug.Log("[EndingVideoTrigger] 🎧 마이마이 회전 사운드 재생 시작");
            }
        }

        yield return new WaitForSeconds(delayBeforeLogos);

        // 로고 시퀀스
        yield return StartCoroutine(FadeCanvasGroup(teamLogo, true, fadeDuration));
        yield return new WaitForSeconds(logoDisplayTime);
        yield return StartCoroutine(FadeCanvasGroup(teamLogo, false, fadeDuration));

        yield return StartCoroutine(FadeCanvasGroup(gameLogo, true, fadeDuration));
        yield return new WaitForSeconds(logoDisplayTime);
        yield return StartCoroutine(FadeCanvasGroup(gameLogo, false, fadeDuration));

        // NextSceneBtn 페이드 인
        yield return StartCoroutine(FadeCanvasGroup(navigationGroup, true, fadeDuration));
    }

    // 🎞 비디오 종료 시
    private void OnVideoEnd(VideoPlayer vp)
    {
        StopVideoAndSound();
        Debug.Log("[EndingVideoTrigger] ⏹️ 비디오 종료 → 마이마이 회전 사운드 정지");
    }

    // ✅ 공용 정지 메서드 (비디오 + 사운드)
    private void StopVideoAndSound()
    {
        if (videoPlayer != null && videoPlaying)
        {
            videoPlayer.Stop();
            videoPlaying = false;
        }

        if (Chap6SoundManager.Instance != null)
        {
            Chap6SoundManager.Instance.StopMymyWindingSFX();
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, float duration)
    {
        if (cg == null) yield break;

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float t = 0f;

        cg.alpha = startAlpha;

        if (fadeIn)
        {
            cg.gameObject.SetActive(true);
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        else
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
        if (fadeIn)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
