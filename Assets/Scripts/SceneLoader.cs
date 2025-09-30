using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    // Canvas Group 컴포넌트 (UI Image와 같은 오브젝트에 있어야 함)
    [Tooltip("페이드 효과를 줄 Canvas Group (UI Image에 추가)")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    // 페이드 인/아웃에 걸리는 기본 시간 (초)
    [Tooltip("기본 페이드 지속 시간 (초)")]
    [SerializeField] private float defaultFadeDuration = 1.0f;
    
    // 씬 시작 시 자동으로 페이드 인을 수행할지 여부
    [Tooltip("씬 시작 시 자동으로 페이드 인 할지 여부")]
    [SerializeField] private bool fadeOnStart = true;

    private void Awake()
    {
        // CanvasGroup이 연결되었는지 확인합니다.
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("[ScreenFader] Canvas Group 컴포넌트가 필요합니다!");
                return;
            }
        }
        
        // 씬 시작 시 검은색(불투명) 상태로 설정하고, 페이드 인을 시작합니다.
        if (fadeOnStart)
        {
            canvasGroup.alpha = 1f; // 씬 시작 시 화면을 완전히 가립니다.
            FadeIn();
        }
        else
        {
            // 시작 시 페이드 인을 하지 않는다면, 투명하게 유지합니다.
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 화면을 투명하게 (보이게) 페이드 인합니다.
    /// </summary>
    /// <param name="duration">페이드에 걸릴 시간</param>
    public void FadeIn(float duration = -1)
    {
        float fadeTime = (duration > 0) ? duration : defaultFadeDuration;
        StartCoroutine(PerformFade(1f, 0f, fadeTime));
    }

    /// <summary>
    /// 화면을 검은색(가리게) 페이드 아웃합니다.
    /// </summary>
    /// <param name="duration">페이드에 걸릴 시간</param>
    /// <returns>페이드가 완료될 때까지의 대기 시간 (float)</returns>
    public float FadeOut(float duration = -1)
    {
        float fadeTime = (duration > 0) ? duration : defaultFadeDuration;
        StartCoroutine(PerformFade(0f, 1f, fadeTime));
        return fadeTime;
    }

    // ── 핵심 페이드 코루틴 ──
    private IEnumerator PerformFade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        
        // 페이드 이미지를 활성화하여 씬을 가릴 준비를 합니다.
        if (endAlpha > startAlpha) 
            canvasGroup.blocksRaycasts = true;

        while (timer < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        // 정확한 목표 값으로 설정
        canvasGroup.alpha = endAlpha;
        
        // 페이드 아웃(1.0)이 완료되면 광선 충돌을 막고, 페이드 인(0.0)이 완료되면 광선 충돌을 해제합니다.
        canvasGroup.blocksRaycasts = endAlpha > 0.99f;
        // alpha가 0이 되면, Raycast Target을 비활성화하여 마우스 클릭을 막지 않게 할 수 있습니다.
    }
}