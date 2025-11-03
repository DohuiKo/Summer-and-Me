using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class PlayRadioVideo : MonoBehaviour
{
    [Header("타겟 및 트리거")]
    public RectTransform targetContent;
    public RectTransform viewport;
    public ScrollRect scrollRect;

    [Header("재생 및 UI")]
    public VideoPlayer videoPlayer;
    [Tooltip("활성화할 내비게이션 UI (NaviCanvasAlpha의 CanvasGroup)")]
    public CanvasGroup navigationGroup;
    [Tooltip("직접 켤 버튼 오브젝트 (NextSlideArrow GameObject)")]
    public GameObject nextSlideArrowObject;

    [Header("설정")]
    public float triggerDistance = 20f;
    public float delayBeforeButton = 3.0f;
    public float fadeDuration = 1.0f;
    [Tooltip("페이지 벗어남 감지 거리 (이 거리 이상이면 비디오 정지)")]
    public float exitStopDistance = 120f;

    private bool hasTriggered = false;
    private bool videoPlaying = false;

    void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            OnScrollChanged(scrollRect.normalizedPosition);
        }

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnScrollChanged(Vector2 value)
    {
        if (viewport == null || targetContent == null) return;

        float viewportCenterX = viewport.position.x;
        float contentCenterX = targetContent.position.x;
        float distance = Mathf.Abs(viewportCenterX - contentCenterX);

        // 🎬 비디오 트리거
        if (!hasTriggered && distance < triggerDistance)
        {
            hasTriggered = true;
            StartCoroutine(PlayVideoAndShowButton());
        }

        // 🚫 페이지 이탈 감지
        if (videoPlaying && distance > exitStopDistance)
        {
            StopVideo();
            Debug.Log("[PlayRadioVideo] 🚫 페이지 이탈 감지 → 비디오 정지");
        }
    }

    private IEnumerator PlayVideoAndShowButton()
    {
        if (navigationGroup != null)
        {
            navigationGroup.alpha = 0f;
            navigationGroup.interactable = false;
            navigationGroup.blocksRaycasts = false;
        }

        // 🎬 비디오 재생
        if (videoPlayer != null)
        {
            Debug.Log("[PlayRadioVideo] 스크롤 중앙 감지됨 → 비디오 재생 시작");
            videoPlayer.Play();
            videoPlaying = true;
        }

        yield return new WaitForSeconds(delayBeforeButton);

        // 버튼 페이드인
        if (navigationGroup != null)
        {
            Debug.Log("3초 경과 → 내비게이션 UI 페이드인");
            if (nextSlideArrowObject != null)
                nextSlideArrowObject.SetActive(true);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                navigationGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            navigationGroup.alpha = 1f;
            navigationGroup.interactable = true;
            navigationGroup.blocksRaycasts = true;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        StopVideo();
        Debug.Log("[PlayRadioVideo] ⏹ 비디오 종료 → 정지 완료");
    }

    private void StopVideo()
    {
        if (videoPlaying)
        {
            videoPlaying = false;
            if (videoPlayer != null)
                videoPlayer.Stop();
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
