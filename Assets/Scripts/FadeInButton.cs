using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInButton : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public float fadeDuration = 0.5f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // 버튼을 서서히 보이게 만듭니다.
    public void FadeIn()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(DoFade(0f, 1f));
    }

    // 버튼을 서서히 숨깁니다.
    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(1f, 0f));
    }

    private IEnumerator DoFade(float startAlpha, float endAlpha)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(startAlpha, endAlpha, t / fadeDuration);
            canvasGroup.alpha = a;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;

        // 완전히 숨겨졌을 때 게임 오브젝트를 비활성화하여 렌더링을 멈춥니다.
        if (endAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }
}