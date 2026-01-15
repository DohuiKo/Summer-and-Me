using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutPopupManager : MonoBehaviour
{
    public static CutPopupManager Instance { get; private set; }
    public static CutPopupManager GetOrFind()
    {
        if (Instance != null) return Instance;
        Instance = FindObjectOfType<CutPopupManager>(true);
        return Instance;
    }
    public static bool IsShowing
    {
        get
        {
            var inst = GetOrFind();
            return inst != null && inst.isShowing;
        }
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image popupImage;

    [Header("Cuts (0=A start, 1=A end, 2=B end, 3=C end)")]
    [SerializeField] private List<Sprite> cuts = new List<Sprite>();

    [Header("Timing (sec)")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private List<float> holdDurations = new List<float>();

    private bool isShowing = false;
    private Coroutine showCo;
    private int currentIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!popupImage) popupImage = GetComponent<Image>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowCut(int index, Action onComplete = null)
    {
        if (cuts == null || index < 0 || index >= cuts.Count || cuts[index] == null)
        {
            onComplete?.Invoke();
            return;
        }

        currentIndex = index;
        ShowCut(cuts[index], onComplete);
    }

    public void ShowCut(Sprite sprite, Action onComplete = null)
    {
        if (sprite == null)
        {
            onComplete?.Invoke();
            return;
        }

        currentIndex = -1;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!popupImage) popupImage = GetComponent<Image>();
        gameObject.SetActive(true);
        if (popupImage) popupImage.enabled = true;

        if (showCo != null)
            StopCoroutine(showCo);

        showCo = StartCoroutine(ShowRoutine(sprite, onComplete));
    }

    private IEnumerator ShowRoutine(Sprite sprite, Action onComplete)
    {
        isShowing = true;

        if (popupImage != null)
            popupImage.sprite = sprite;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        yield return Fade(0f, 1f, fadeInDuration);
        yield return WaitUnscaled(GetHoldDuration());
        yield return Fade(1f, 0f, fadeOutDuration);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        isShowing = false;
        onComplete?.Invoke();
        showCo = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null || duration <= 0f)
        {
            if (canvasGroup != null) canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            canvasGroup.alpha = Mathf.LerpUnclamped(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    private IEnumerator WaitUnscaled(float duration)
    {
        if (duration <= 0f) yield break;
        float end = Time.unscaledTime + duration;
        while (Time.unscaledTime < end)
            yield return null;
    }

    private float GetHoldDuration()
    {
        if (currentIndex >= 0 && currentIndex < holdDurations.Count)
            return Mathf.Max(0f, holdDurations[currentIndex]);
        return Mathf.Max(0f, holdDuration);
    }
}
