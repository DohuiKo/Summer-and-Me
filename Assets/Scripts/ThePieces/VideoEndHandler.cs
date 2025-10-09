using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;         // Image 컴포넌트 사용을 위해 추가
using System.Collections;     // Coroutine 사용을 위해 추가

public class VideoEndHandler : MonoBehaviour
{
    // 1. 인스펙터에 할당할 비디오 플레이어
    public VideoPlayer videoPlayer;

    // 2. 인스펙터에 할당할 검은색 UI 패널 (Image 컴포넌트)
    public Image fadePanel;

    // 🌟 NextSlideArrow 버튼 GameObject
    public GameObject nextSlideArrowButton; 

    // 3. 페이드 연출 시간 설정
    public float fadeDuration = 1.0f; // 암전(Fade Out)에 걸리는 시간

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        
        if (fadePanel != null)
        {
            // 시작 시 페이드 패널을 투명하게 설정 (A=0)
            Color panelColor = fadePanel.color;
            panelColor.a = 0f;
            fadePanel.color = panelColor;
        }

        // 시작 시 NextSlideArrow 버튼을 비활성화 (숨김)
        if (nextSlideArrowButton != null)
        {
            nextSlideArrowButton.SetActive(false);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // 비디오 재생이 끝나면 암전 코루틴 시작
        StartCoroutine(FadeAndShowArrow());
    }

    // 시간의 흐름에 따라 암전 후 버튼을 활성화하는 코루틴
    IEnumerator FadeAndShowArrow()
    {
        // 1. 암전 (Fade Out): 투명도 0 -> 1로 증가
        // fadeDuration 시간 동안 완전히 어두워집니다.
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // 2. 암전 완료 후 NextSlideArrow 버튼 활성화
        Debug.Log("암전 완료: NextSlideArrow 버튼 활성화");
        if (nextSlideArrowButton != null)
        {
            nextSlideArrowButton.SetActive(true);
        }
        
        // **********************************************
        // 🚨 중요: 여기서 페이지 전환이나 다음 씬 로드 등의
        // 실제 동작을 수행해야 합니다.
        // 예를 들어: SceneManager.LoadScene("NextSceneName");
        // **********************************************
    }
    
    // 알파 값을 부드럽게 보간(Lerp)하는 함수 (이전과 동일)
    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fadePanel.color.a;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            
            Color panelColor = fadePanel.color;
            panelColor.a = newAlpha;
            fadePanel.color = panelColor;
            
            yield return null;
        }
        
        // 최종적으로 목표 알파 값 (1, 완전한 암전)으로 설정하여 마무리
        Color finalColor = fadePanel.color;
        finalColor.a = targetAlpha;
        fadePanel.color = finalColor;
    }
}