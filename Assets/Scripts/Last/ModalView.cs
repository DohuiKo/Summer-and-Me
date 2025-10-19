using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ModalView : MonoBehaviour
{
    public Image pageImage;
    public float fadeTime = 0.35f;
    public bool useUnscaledTime = true;

    [Header("Optional Audio")]
    public AudioSource bgm;
    public bool playBgmOnShow = true;

    CanvasGroup cg;
    Sprite defaultSprite;

    // 🔸 모달이 완전히 표시(페이드 완료)된 순간을 알리는 이벤트
    public event Action Shown;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (pageImage) defaultSprite = pageImage.sprite;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Show(Sprite page = null)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (pageImage)
        {
            if (page != null) pageImage.sprite = page;
            else if (defaultSprite != null && pageImage.sprite == null) pageImage.sprite = defaultSprite;
        }

        if (bgm && playBgmOnShow && !bgm.isPlaying) bgm.Play();   // ← BGM 재생

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        cg.interactable = false; cg.blocksRaycasts = false;
        float t = 0f;
        while (t < fadeTime)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        cg.alpha = 1f;
        cg.interactable = true; cg.blocksRaycasts = true;

        Shown?.Invoke();   // 🔸 페이드 완료 알림 (중요!)
    }

    IEnumerator FadeOut()
    {
        cg.interactable = false; cg.blocksRaycasts = false;
        float t = fadeTime;
        while (t > 0f)
        {
            t -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        cg.alpha = 0f;
    }
}
