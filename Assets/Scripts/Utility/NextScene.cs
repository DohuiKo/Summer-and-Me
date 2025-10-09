// NextScene.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class NextScene : MonoBehaviour
{
    public enum TransitionMode { Instant, FadeCanvasGroup, AsyncWithFade }

    [Header("Target")]
    [Tooltip("이동할 씬 이름 (Build Settings에 등록 필수)")]
    public string sceneName;

    [Header("Transition")]
    public TransitionMode transition = TransitionMode.FadeCanvasGroup;

    [Tooltip("페이드에 사용할 오버레이(CanvasGroup). 없으면 런타임 생성")]
    public CanvasGroup fadeOverlay;
    public float fadeOutTime = 0.6f;
    public float fadeInTime  = 0.0f;   // 필요 시 사용

    [Header("Async Options")]
    public bool allowSceneActivation = true; // false면 0.9에서 멈춤
    public Slider progressBar;               // 선택: 로딩 바

    bool isLoading;

    void Reset()
    {
        // 버튼에 붙였다면 자동으로 OnClick 연결
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.AddListener(Load);
    }

    public void Load()
    {
        if (isLoading) return;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[NextScene] sceneName이 비어 있습니다.");
            return;
        }

        switch (transition)
        {
            case TransitionMode.Instant:
                SceneManager.LoadScene(sceneName);
                break;

            case TransitionMode.FadeCanvasGroup:
                StartCoroutine(LoadWithFade(false));
                break;

            case TransitionMode.AsyncWithFade:
                StartCoroutine(LoadWithFade(true));
                break;
        }
    }

    IEnumerator LoadWithFade(bool async)
    {
        isLoading = true;

        // 오버레이 준비(검은 Image + CanvasGroup)
        if (!fadeOverlay)
        {
            var go = new GameObject("FadeOverlay", typeof(Canvas), typeof(CanvasGroup));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeOverlay = go.GetComponent<CanvasGroup>();
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = true;

            var imgGO = new GameObject("BG", typeof(UnityEngine.UI.Image));
            imgGO.transform.SetParent(go.transform, false);
            var img = imgGO.GetComponent<UnityEngine.UI.Image>();
            img.color = Color.black;
            var rt = img.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // 페이드 아웃
        float t = 0f;
        while (t < fadeOutTime)
        {
            fadeOverlay.alpha = Mathf.Lerp(0f, 1f, t / fadeOutTime);
            t += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.alpha = 1f;

        if (!async)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // 비동기 로딩
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = allowSceneActivation;

        while (!op.isDone)
        {
            // 0f~0.9f 구간을 0~1로 매핑
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar) progressBar.value = p;

            // 자동 활성화 켠 경우, 0.9에 도달하면 씬 전환됨
            if (allowSceneActivation == false && p >= 1f)
            {
                // 외부에서 조건 충족 시 아래 한 줄을 호출하면 즉시 진입
                // op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
