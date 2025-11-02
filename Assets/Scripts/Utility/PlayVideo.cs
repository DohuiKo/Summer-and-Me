using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class PlayVideo : MonoBehaviour
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

    private bool hasTriggered = false;

    void Start()
    {
        // 💾 1. Start() 에서는 'navigationGroup'의 알파값을 건드리지 않습니다!
        // 💾    첫 페이지의 NextSlideActivator 스크립트가 정상 동작해야 하기 때문입니다.
        // 💾    대신 null 체크만 수행합니다.
        if (navigationGroup == null)
        {
            Debug.LogError("PlayVideo: 'Navigation Group'이 할당되지 않았습니다!");
        }
        if (nextSlideArrowObject == null)
        {
             Debug.LogError("PlayVideo: 'Next Slide Arrow Object'가 할당되지 않았습니다!");
        }

        // 2. 스크롤 이벤트 리스너 등록
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            OnScrollChanged(scrollRect.normalizedPosition); // 씬 시작 시 위치 체크
        }
        else
        {
            Debug.LogError("PlayVideo: 'Scroll Rect'가 할당되지 않았습니다!");
        }

        if (viewport == null) Debug.LogError("PlayVideo: 'Viewport'가 할당되지 않았습니다!");
        if (targetContent == null) Debug.LogError("PlayVideo: 'Target Content'가 할당되지 않았습니다!");
        if (videoPlayer == null) Debug.LogError("PlayVideo: 'Video Player'가 할당되지 않았습니다!");
    }

    // (OnScrollChanged 함수는 이전과 동일합니다)
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
            StartCoroutine(PlayVideoAndShowButton());
        }
    }

    private IEnumerator PlayVideoAndShowButton()
    {
        // 💾 1. (새로운 단계)
        // 💾 비디오 재생이 트리거되는 이 순간에, 첫 페이지에서 켜져있던
        // 💾 내비게이션 UI를 즉시 숨깁니다. (알파=0)
        if (navigationGroup != null)
        {
            navigationGroup.alpha = 0f;
            navigationGroup.interactable = false;
            navigationGroup.blocksRaycasts = false;
        }

        // 2. 동영상 재생
        if (videoPlayer != null)
        {
            Debug.Log("스크롤 중앙 감지! 비디오를 재생합니다.");
            videoPlayer.Play();
        }

        // 3. 3초 대기
        yield return new WaitForSeconds(delayBeforeButton);

        // 4. 내비게이션 UI 활성화
        if (navigationGroup != null)
        {
            Debug.Log("3초 경과. 내비게이션 UI를 페이드인합니다.");

            // 4a. 버튼 오브젝트를 강제로 켭니다!
            if (nextSlideArrowObject != null)
            {
                nextSlideArrowObject.SetActive(true);
            }

            // 4b. 페이드인 시작 (이제 0에서 시작하는 것이 보장됨)
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                navigationGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            
            // 4c. 페이드인 완료 및 상호작용 활성화
            navigationGroup.alpha = 1f;
            navigationGroup.interactable = true;
            navigationGroup.blocksRaycasts = true;
        }
    }

    // (OnDestroy 함수는 이전과 동일합니다)
    void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
    }
}