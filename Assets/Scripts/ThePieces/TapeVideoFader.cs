using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class TapeVideoFader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public float fadeInDuration = 1.0f; 

    // Next Scene 버튼 GameObject
    public GameObject nextSceneBtn; 

    private int playbackCount = 0; // 🌟 현재 재생 완료 횟수 카운터
    private const int REQUIRED_PLAYBACKS = 2; // 🌟 버튼 활성화에 필요한 재생 횟수

    void Start()
    {
        // Start()에서 NextSceneBtn 비활성화 (숨김)
        if (nextSceneBtn != null)
        {
            nextSceneBtn.SetActive(false);
        }
        
        // 비디오 재생 완료 이벤트 연결
        if (videoPlayer != null)
        {
            // loopPointReached 이벤트는 영상이 마지막 프레임에 도달했을 때 호출됩니다.
            videoPlayer.loopPointReached += OnVideoPlaybackFinished;
        }
    }

    // 🌟 비디오 재생 완료 시 호출되는 함수 (2회 재생 후 버튼 활성화)
    void OnVideoPlaybackFinished(VideoPlayer vp)
    {
        playbackCount++; // 재생 완료 횟수 증가
        Debug.Log($"Tape 영상 재생 완료 횟수: {playbackCount} / {REQUIRED_PLAYBACKS}");

        // 재생 횟수가 목표치(2회)에 도달했을 때만 버튼 활성화
        if (playbackCount >= REQUIRED_PLAYBACKS)
        {
            if (nextSceneBtn != null)
            {
                nextSceneBtn.SetActive(true);
                Debug.Log("Next Scene 버튼 활성화 완료.");
                
                // (선택 사항) 무한 호출 방지를 위해 이벤트 연결 해제
                // videoPlayer.loopPointReached -= OnVideoPlaybackFinished;
            }
        }
        
        // 🌟 주의: 2회 이상 재생되도록 하려면 VideoPlayer의 Loop 옵션이 활성화되어 있어야 합니다.
    }

    public void StartFadeAndPlay()
    {
        if (videoPlayer == null || rawImage == null)
        {
            Debug.LogError("TapeVideoFader: VideoPlayer 또는 RawImage가 할당되지 않았습니다.");
            return;
        }

        // 코루틴 시작 오류 방지: 오브젝트 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // 1. 비디오 재생 시작
        videoPlayer.Play();
        
        // 2. 페이드인 코루틴 시작
        StartCoroutine(FadeIn(fadeInDuration));
    }

    private IEnumerator FadeIn(float duration)
    {
        float startAlpha = 0f;
        float targetAlpha = 1f;
        float time = 0;
        
        Color color = rawImage.color;
        color.a = 0f;
        rawImage.color = color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT); 
            
            Color newColor = rawImage.color;
            newColor.a = newAlpha;
            rawImage.color = newColor;
            
            yield return null;
        }

        Color finalColor = rawImage.color;
        finalColor.a = targetAlpha;
        rawImage.color = finalColor;
    }
}