using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ClickToFade : MonoBehaviour, IPointerClickHandler
{
    [Header("클릭 설정")]
    [Tooltip("몇 번째 클릭에 반응할지 설정합니다.")]
    public int requiredClicks = 5;

    [Header("페이드 효과 설정")]
    [Tooltip("흐릿해지는 효과가 지속되는 시간 (초)")]
    public float fadeDuration = 1.5f;

    [Header("활성화할 오브젝트 설정")]
    [Tooltip("페이드 아웃이 끝나면 활성화할 게임 오브젝트 (예: NextDownArrow)")]
    public GameObject objectToShow;

    private int currentClickCount = 0;
    private CanvasGroup canvasGroup;
    private bool isFading = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isFading)
        {
            return;
        }

        currentClickCount++;
        Debug.Log($"클릭! 현재 횟수: {currentClickCount}");

        if (currentClickCount >= requiredClicks)
        {
            isFading = true;
            StartCoroutine(FadeOutCoroutine());
            
            // ----- ▼▼▼ 이 위치에 있던 버튼 활성화 코드를 삭제합니다. ▼▼▼ -----
            // if (objectToShow != null) { ... } 부분이 여기서 사라졌습니다.
            // ----- ▲▲▲ 코드 삭제 완료 ▲▲▲ -----
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        Debug.Log("5번째 클릭! 페이드 아웃을 시작합니다.");

        float counter = 0f;
        float startAlpha = canvasGroup.alpha;

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, counter / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        
        // ----- ▼▼▼ 페이드 아웃이 끝난 이 시점에 버튼 활성화 코드를 추가합니다. ▼▼▼ -----
        // objectToShow 변수에 오브젝트가 연결되어 있는지 확인합니다.
        if (objectToShow != null)
        {
            // 연결되어 있다면 활성화시킵니다.
            objectToShow.SetActive(true);
            Debug.Log("페이드 아웃 완료! NextDownArrow를 활성화합니다.");
        }
        // ----- ▲▲▲ 코드 추가 완료 ▲▲▲ -----
    }
}