using System.Collections;
using UnityEngine;

public class HomeReturnUIManager : MonoBehaviour
{
    [Header("패널 참조")]
    public GameObject homeReturnPanel;   // HomeReturnPanel 오브젝트
    public CanvasGroup panelCanvasGroup; // HomeReturnPanel에 붙은 CanvasGroup
    public float fadeDuration = 0.3f;

    void Start()
    {
        // 시작할 때는 완전 숨김 상태
        if (homeReturnPanel != null)
            homeReturnPanel.SetActive(false);

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;
    }

    public void ShowPanel()
    {
        if (homeReturnPanel == null || panelCanvasGroup == null) return;

        homeReturnPanel.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, 1f, false));
    }

    public void HidePanel()
    {
        if (homeReturnPanel == null || panelCanvasGroup == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, 0f, true));
    }

    IEnumerator FadeRoutine(float from, float to, bool deactivateAtEnd)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            panelCanvasGroup.alpha = Mathf.Lerp(from, to, lerp);
            yield return null;
        }

        panelCanvasGroup.alpha = to;

        bool visible = to > 0.01f;
        panelCanvasGroup.interactable = visible;
        panelCanvasGroup.blocksRaycasts = visible;

        if (deactivateAtEnd && homeReturnPanel != null)
            homeReturnPanel.SetActive(false);
    }
}
