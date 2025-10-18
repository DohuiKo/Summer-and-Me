using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ClothesModal : MonoBehaviour
{
    [Tooltip("모달 페이드 시간(초)")]
    public float fadeDuration = 0.3f;

    private CanvasGroup cg;
    private Coroutine fade;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        // 오브젝트는 항상 Active 상태 유지 + 눈에만 안 보이게
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Open()
    {
        if (fade != null) StopCoroutine(fade);
        if (!gameObject.activeSelf) gameObject.SetActive(true); // 안전장치
        fade = StartCoroutine(Fade(1f, enableInputAtEnd: true));
    }

    public void Close()
    {
        if (!gameObject.activeInHierarchy) return; // 비활성 시 중복 호출 방지
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Fade(0f, enableInputAtEnd: false));
    }

    private IEnumerator Fade(float target, bool enableInputAtEnd)
    {
        float start = cg.alpha;
        float t = 0f;

        // 페이드 중엔 입력 막기
        cg.interactable = false;
        cg.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        cg.alpha = target;
        cg.interactable = enableInputAtEnd;
        cg.blocksRaycasts = enableInputAtEnd;

        fade = null;
    }
}
