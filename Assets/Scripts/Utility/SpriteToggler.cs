using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpriteState
{
    public Sprite sprite;
    public bool overrideSize = false;
    public Vector2 size = new Vector2(100, 100);
    public AudioClip audioClip; // 🎵 각 상태별 오디오
}

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class SpriteToggler : MonoBehaviour
{
    [Header("Sprite & Size Settings")]
    public List<SpriteState> spriteStates;

    [Header("Final State Action")]
    public bool enableActionOnLastState = false;
    public GameObject objectToShowOnLastState;

    [Header("Fade Transition Settings")]
    public bool useFadeTransition = false;
    [Range(0.1f, 2.0f)] public float fadeDuration = 0.5f;

    [Header("Audio Settings")]
    public bool useAudioOnClick = true;

    private Image imageComponent;
    private RectTransform rectTransform;
    private int currentStateIndex = 0;
    private bool isFading = false;
    private Color originalColor;
    private AudioSource audioSource;

    void Awake()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();

        if (imageComponent != null)
            originalColor = imageComponent.color;

        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (spriteStates != null && spriteStates.Count > 0)
            ApplyState(0);

        if (enableActionOnLastState && objectToShowOnLastState != null)
            objectToShowOnLastState.SetActive(false);
    }

    public void ToggleSpriteAndSize()
    {
        if (spriteStates == null || spriteStates.Count == 0)
        {
            Debug.LogWarning("[SpriteToggler] Sprite States 리스트가 비어있습니다!");
            return;
        }

        if (currentStateIndex >= spriteStates.Count - 1)
        {
            Debug.Log("[SpriteToggler] 마지막 스프라이트입니다.");
            return;
        }

        if (useFadeTransition)
            StartCoroutine(FadeAndToggle());
        else
        {
            currentStateIndex++;
            ApplyState(currentStateIndex);
            PlayStateAudio(currentStateIndex);
        }
    }

    private void ApplyState(int index)
    {
        if (index < 0 || index >= spriteStates.Count) return;

        SpriteState state = spriteStates[index];

        if (imageComponent != null && state.sprite != null)
            imageComponent.sprite = state.sprite;

        if (state.overrideSize && rectTransform != null)
            rectTransform.sizeDelta = state.size;

        if (enableActionOnLastState && objectToShowOnLastState != null)
        {
            bool isLastState = (index == spriteStates.Count - 1);
            objectToShowOnLastState.SetActive(isLastState);
        }

        // ✅ 오디오 재생 호출 추가
        PlayStateAudio(index);
    }


    private IEnumerator FadeAndToggle()
    {
        isFading = true;

        float halfDuration = fadeDuration / 2f;
        float elapsedTime = 0f;
        Color currentColor = originalColor;

        // 🔻 Fade-out
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / halfDuration);
            imageComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        imageComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);

        currentStateIndex++;
        ApplyState(currentStateIndex);
        PlayStateAudio(currentStateIndex);

        // 🔺 Fade-in
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalColor.a, elapsedTime / halfDuration);
            imageComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        imageComponent.color = originalColor;
        isFading = false;
    }

    // ======================================================
    // 🎵 오디오 재생 로직 (ChapSoundManager 위임형)
    // ======================================================
    private void PlayStateAudio(int index)
    {
        if (!useAudioOnClick) return;
        if (index < 0 || index >= spriteStates.Count) return;

        AudioClip clip = spriteStates[index].audioClip;
        if (clip == null) return;

        // ✅ Chap5SoundManager에 위임
        if (Chap5SoundManager.Instance != null)
        {
            Chap5SoundManager.Instance.PlaySFX(clip);
            Debug.Log($"[SpriteToggler] 🔊 {clip.name} 재생 요청됨 (via Chap5SoundManager)");
        }
        else
        {
            // Fallback (사운드매니저가 없을 때 직접 재생)
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(clip);
        }
    }
}
