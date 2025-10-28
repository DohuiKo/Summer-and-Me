using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// --- [수정된 SpriteState 클래스 정의] ---
// (별도 파일이 아니라, SpriteToggler.cs 파일 상단에 함께 둡니다)
[System.Serializable]
public class SpriteState
{
    public Sprite sprite;
    
    [Tooltip("체크하면 아래에 설정된 Size 값으로 크기를 변경합니다.")]
    public bool overrideSize = false;
    
    public Vector2 size = new Vector2(100, 100);

    [Tooltip("이 상태로 변경될 때 재생할 오디오 클립 (선택 사항)")]
    public AudioClip audioClip; // <--- 오디오 클립 추가
}
// ------------------------------------------


// --- [SpriteToggler 클래스 본체] ---
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AudioSource))]
public class SpriteToggler : MonoBehaviour
{
    [Header("Sprite & Size Settings")]
    [Tooltip("여기에 스프라이트와 원하는 사이즈를 설정하세요.")]
    public List<SpriteState> spriteStates; // <--- 이제 이 타입을 올바르게 인식합니다.

    [Header("Final State Action")]
    [Tooltip("체크하면 마지막 상태가 되었을 때 아래 오브젝트를 활성화합니다.")]
    public bool enableActionOnLastState = false; 
    [Tooltip("마지막 상태가 되었을 때 활성화할 게임 오브젝트를 연결하세요.")]
    public GameObject objectToShowOnLastState;

    [Header("Fade Transition Settings")]
    [Tooltip("체크하면 스프라이트 변경 시 페이드인/아웃 효과를 사용합니다.")]
    public bool useFadeTransition = false;
    [Tooltip("페이드인/아웃에 걸리는 총 시간 (초)")]
    [Range(0.1f, 2.0f)]
    public float fadeDuration = 0.5f;
    
    [Header("Audio Settings")]
    [Tooltip("체크하면 상태 변경 시 'SpriteState'에 설정된 오디오 클립을 재생합니다.")]
    public bool useAudioOnClick = false;

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
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
        else
        {
            Debug.LogError("Image 컴포넌트가 없습니다!", this.gameObject);
        }
    }

    void Start()
    {
        if (spriteStates != null && spriteStates.Count > 0)
        {
            ApplyState(0);
        }

        if (enableActionOnLastState && objectToShowOnLastState != null)
        {
            objectToShowOnLastState.SetActive(false);
        }
    }

    public void ToggleSpriteAndSize()
    {
        if (spriteStates == null || spriteStates.Count == 0)
        {
            Debug.LogWarning("Sprite States 리스트가 비어있습니다!");
            return;
        }

        if (currentStateIndex >= spriteStates.Count - 1)
        {
            Debug.Log("마지막 스프라이트입니다.");
            return;
        }

        if (useFadeTransition)
        {
            if (!isFading)
            {
                StartCoroutine(FadeAndToggle());
            }
        }
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

        SpriteState state = spriteStates[index]; // <--- 오류가 나던 지점
        
        if (imageComponent != null)
        {
            imageComponent.sprite = state.sprite; // <--- 오류가 나던 지점
        }

        if (state.overrideSize && rectTransform != null) // <--- 오류가 나던 지점
        {
            rectTransform.sizeDelta = state.size; // <--- 오류가 나던 지점
        }

        if (enableActionOnLastState && objectToShowOnLastState != null)
        {
            bool isLastState = (index == spriteStates.Count - 1);
            objectToShowOnLastState.SetActive(isLastState);
        }
    }

    private IEnumerator FadeAndToggle()
    {
        isFading = true; 

        float halfDuration = fadeDuration / 2f;
        float elapsedTime = 0f;
        Color currentColor = originalColor;

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
    
    private void PlayStateAudio(int index)
    {
        if (!useAudioOnClick) return;
        if (audioSource == null) return;
        if (index < 0 || index >= spriteStates.Count) return;

        AudioClip clipToPlay = spriteStates[index].audioClip; // <--- 오류가 나던 지점

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}