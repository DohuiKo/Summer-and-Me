using UnityEngine;
using UnityEngine.UI; 
using System.Collections; 

public class DeskPageManager : MonoBehaviour
{
    // 1. 인스펙터에 할당할 검은색 UI 패널 (Image 컴포넌트)
    public Image fadePanel;

    // 2. 인스펙터에 할당할 NextSlideArrow 버튼 GameObject
    public GameObject nextSlideArrow; 

    // 3. 페이드 및 딜레이 시간 설정
    public float fadeInDuration = 1.5f; // 밝아지는(Fade In) 데 걸리는 시간
    public float delayAfterFade = 4.0f; // 밝아진 후 딜레이 시간 (4초)

    void Start()
    {
        // 시작 시 화면 연출 코루틴 시작
        StartCoroutine(InitialScreenTransition());
        
        // NextSlideArrow는 초기에는 비활성화 (스크립트에서 제어)
        if (nextSlideArrow != null)
        {
            nextSlideArrow.SetActive(false);
        }
    }

    IEnumerator InitialScreenTransition()
    {
        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel이 DeskPageManager에 할당되지 않았습니다.");
            yield break;
        }

        // 1. 시작: 화면을 완전히 검게 설정 (알파 = 1)
        Color panelColor = fadePanel.color;
        panelColor.a = 1f;
        fadePanel.color = panelColor;

        // 2. 페이드 인 (암전 -> 밝아짐) 연출 시작
        yield return StartCoroutine(Fade(0f, fadeInDuration));

        // 3. 딜레이 (밝아진 상태 유지)
        Debug.Log($"페이드 인 완료. {delayAfterFade}초 딜레이 시작.");
        yield return new WaitForSeconds(delayAfterFade);

        // 4. NextSlideArrow 활성화
        if (nextSlideArrow != null)
        {
            nextSlideArrow.SetActive(true);
            Debug.Log("NextSlideArrow 버튼 활성화.");
        }
    }

    // 알파 값을 부드럽게 보간(Lerp)하는 함수 (Fade Out / Fade In 모두 사용 가능)
    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fadePanel.color.a;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            // 🌟 SmoothStep 이징을 적용하여 부드러운 전환 효과
            float smoothT = Mathf.SmoothStep(0f, 1f, time / duration); 
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
            
            Color panelColor = fadePanel.color;
            panelColor.a = newAlpha;
            fadePanel.color = panelColor;
            
            yield return null;
        }
        
        // 최종적으로 목표 알파 값으로 설정하여 마무리
        Color finalColor = fadePanel.color;
        finalColor.a = targetAlpha;
        fadePanel.color = finalColor;
    }
}