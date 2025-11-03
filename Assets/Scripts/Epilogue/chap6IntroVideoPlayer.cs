using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Chap6 시작 시 중앙에서 EndingWalking01 영상을 자동 재생하는 스크립트
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class Chap6IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoClip introClip;                  // 🎬 EndingWalking01 영상 클립
    public RawImage videoScreen;                 // 영상 표시용 UI
    public CanvasGroup chapterPageCanvas;        // 챕터 페이지 (서서히 페이드아웃 가능)
    public float fadeOutDelay = 1.0f;            // 영상 재생 후 챕터 텍스트 사라지기 딜레이
    public float fadeOutDuration = 1.0f;

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (introClip == null)
        {
            Debug.LogWarning("[Chap6IntroVideoPlayer] 🎞 introClip이 연결되지 않았습니다.");
            return;
        }

        // ✅ VideoPlayer 설정
        videoPlayer.clip = introClip;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.Prepare();

        // 준비 완료 후 재생
        videoPlayer.prepareCompleted += (vp) =>
        {
            if (videoScreen != null)
                videoScreen.texture = vp.texture;

            vp.Play();
            Debug.Log("[Chap6IntroVideoPlayer] ▶ 영상 재생 시작");

            // 챕터 텍스트가 있다면 페이드아웃
            if (chapterPageCanvas != null)
                StartCoroutine(FadeOutChapterText());
        };
    }

    IEnumerator FadeOutChapterText()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float t = 0f;
        float startAlpha = chapterPageCanvas.alpha;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            chapterPageCanvas.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration);
            yield return null;
        }

        chapterPageCanvas.alpha = 0f;
        chapterPageCanvas.gameObject.SetActive(false);
    }
}
