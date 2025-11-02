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
    public CanvasGroup navigationGroup; // 'NextSceneBtn'이 연결된 슬롯
    [Tooltip("팀 로고 (CanvasGroup 필요)")]
    public CanvasGroup teamLogo;
    [Tooltip("게임 로고 (CanvasGroup 필요)")]
    public CanvasGroup gameLogo;

    [Header("설정")]
    public float triggerDistance = 20f;
    public float delayBeforeLogos = 3.0f;
    public float fadeDuration = 1.5f;
    public float logoDisplayTime = 2.5f;

    private bool hasTriggered = false;

    void Start()
    {
        // 1. 로고들 초기화 (시작 시 투명하게)
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
        /*
        // 💾 1b. NextSceneBtn도 시작 시 숨깁니다.
        if (navigationGroup != null)
        {
            navigationGroup.alpha = 0f;
            navigationGroup.interactable = false;
            navigationGroup.gameObject.SetActive(false);
        } */

        // 2. 스크롤 이벤트 리스너 등록
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            OnScrollChanged(scrollRect.normalizedPosition);
        }
        else
        {
            Debug.LogError("EndingVideoTrigger: 'Scroll Rect'가 할당되지 않았습니다!");
        }

        // 3. 필수 항목 null 체크 (생략)
    }

    private void OnScrollChanged(Vector2 value)
    {
        if (hasTriggered) return;
        if (viewport == null || targetContent == null) return;

        float viewportCenterX = viewport.position.x;
        float contentCenterX = targetContent.position.x;
        float distance = Mathf.Abs(viewportCenterX - contentCenterX);

        if (distance < triggerDistance)
        {
            hasTriggered = true;
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
            StartCoroutine(PlayVideoAndShowLogos());
        }
    }
    
    // 💾 [수정됨] 로고 시퀀스 및 버튼 활성화 코루틴
    private IEnumerator PlayVideoAndShowLogos()
    {
        // 1. (Start에서 이미 숨겼지만 확인차) NextSceneBtn을 비활성화/투명하게
        if (navigationGroup != null)
        {
            navigationGroup.alpha = 0f;
            navigationGroup.interactable = false;
            navigationGroup.blocksRaycasts = false;
        }

        // 2. 동영상 재생
        if (videoPlayer != null) videoPlayer.Play();

        // 3. 딜레이
        yield return new WaitForSeconds(delayBeforeLogos);

        // 4. 팀 로고 시퀀스 (Fade In -> Wait -> Fade Out)
        yield return StartCoroutine(FadeCanvasGroup(teamLogo, true, fadeDuration));
        yield return new WaitForSeconds(logoDisplayTime);
        yield return StartCoroutine(FadeCanvasGroup(teamLogo, false, fadeDuration));

        // 5. 게임 로고 시퀀스 (Fade In -> Wait -> Fade Out)
        yield return StartCoroutine(FadeCanvasGroup(gameLogo, true, fadeDuration));
        yield return new WaitForSeconds(logoDisplayTime);
        yield return StartCoroutine(FadeCanvasGroup(gameLogo, false, fadeDuration));
        
        // 6. 💾 [새로운 기능] NextSceneBtn 활성화 (Fade In Only)
        yield return StartCoroutine(FadeCanvasGroup(navigationGroup, true, fadeDuration));
    }
    
    // 💾 [수정됨] 더 완벽한 페이드 헬퍼 함수 (상호작용 포함)
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, float duration)
    {
        if (cg == null) yield break;

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float t = 0f;

        cg.alpha = startAlpha;
        
        if (fadeIn) // 페이드인 할 때
        {
            cg.gameObject.SetActive(true); // 우선 켜고
            cg.interactable = false; // 페이드 중에는 클릭 안되게
            cg.blocksRaycasts = false;
        }
        else // 페이드아웃 할 때
        {
            cg.interactable = false; // 즉시 클릭 안되게
            cg.blocksRaycasts = false;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            yield return null;
        }

        // 최종 상태 설정
        cg.alpha = endAlpha;
        if (fadeIn)
        {
            cg.interactable = true; // 페이드인 끝나면 클릭 가능하게
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.gameObject.SetActive(false); // 페이드아웃 끝나면 끄기
        }
    }

    void OnDestroy()
    {
        if (scrollRect != null) scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }
}